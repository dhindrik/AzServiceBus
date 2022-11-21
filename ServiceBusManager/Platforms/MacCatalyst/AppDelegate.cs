using System;
using BackgroundTasks;
using Foundation;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using ServiceBusManager.Platforms.MacCatalyst;
using UIKit;
using UserNotifications;

namespace ServiceBusManager;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIKit.UIApplication application, NSDictionary launchOptions)
    {
        base.FinishedLaunching(application, launchOptions);

        try
        {
            UNUserNotificationCenter.Current.Delegate = new UserNotificationCenterDelegate();

            // Check we're at least v10.14
            if (NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(10, 14, 0)))
            {
                // Request notification permissions from the user
                UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert |
                    UNAuthorizationOptions.Badge |
                    UNAuthorizationOptions.Sound, (approved, err) =>
                    {

                    });
            }

            CheckFeature();
        }
        catch (Exception ex)
        {
            var service = Resolver.Resolve<ILogService>();

            if(service != null)
            {
                service.LogException(ex);
            }
        }

        return true;
    }


    private bool CheckFeature()
    {
        var service = Resolver.Resolve<IFeatureService>();

        if (service == null)
        {
            throw new ArgumentNullException("Add IFeatureService to IoC");
        }

        if (service.HasFeature(Constants.Features.Premium))
        {
            var path = $"{FileSystem.AppDataDirectory}/bglog.txt";

            BGTaskScheduler.Shared.Register("se.hindrikes.azservicebus.fetch", null, (task) =>
            {
                var log = Resolver.Resolve<ILogService>();

                if (log != null)
                {
                    log.LogEvent("BackgroundFetchStarting");
                }

                File.AppendAllLines(path, new List<string>() { DateTime.Now.ToString() });


                var dateTime = DateTimeOffset.UtcNow;

                var service = Resolver.Resolve<IServiceBusService>();
               

                if (service == null)
                {
                    task.SetTaskCompleted(false);
                    return;
                }

                var compareDate = Preferences.Get("LastDeadLetter", dateTime.DateTime);

                var countTask = service.CheckNewDeadLetters(new DateTimeOffset(compareDate));
                countTask.RunSynchronously();

                if (countTask.Result > 0)
                {
                    SendNotification(countTask.Result);
                }


                Preferences.Set("LastDeadLetter", dateTime.DateTime);

                task.SetTaskCompleted(true);

                if (log != null)
                {
                    log.LogEvent("BackgroundFetchCompleted");
                }


                ScheduleAppRefresh();
            });

            ScheduleAppRefresh();

            return true;
        }
        else
        {
            service.FeatureChanged += Service_FeatureChanged;
        }

        return false;
    }

    private void ScheduleAppRefresh()
    {
        var request = new BGAppRefreshTaskRequest("se.hindrikes.azservicebus.fetch");
        request.EarliestBeginDate = DateTime.Now.AddSeconds(30).ToNSDate();

        BGTaskScheduler.Shared.Submit(request, out var err);
        var log = Resolver.Resolve<ILogService>();
        if (err != null)
        {
            if (log != null)
            {
                log.LogException(new Exception(err.ToString()));
            }
        }

        

        if(log != null)
        {
            log.LogEvent(nameof(ScheduleAppRefresh));
        }
    }

    private void Service_FeatureChanged(object? sender, EventArgs e)
    {
        var service = Resolver.Resolve<IFeatureService>();

        if (CheckFeature())
        {
            if (service == null)
            {
                throw new ArgumentNullException("Add IFeatureService to IoC");
            }

            service.FeatureChanged -= Service_FeatureChanged;
        }
    }



    private void SendNotification(int badgeNumber)
    {
        var shouldSend = Preferences.Default.Get<bool>("Notifications", false);

        if (!shouldSend)
            return;

#if MACCATALYST
        // Version check

        var content = new UNMutableNotificationContent();
        content.Title = "New dead letters";
        content.Body = "There are new dead letters in your Service Bus";
        content.Badge = badgeNumber;

        var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(5, false);

        var requestID = "deadLettersRequest";
        var request = UNNotificationRequest.FromIdentifier(requestID, content, trigger);

        UIApplication.SharedApplication.ApplicationIconBadgeNumber = 1;

        UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) =>
        {
            if (err != null)
            {
                // Do something with error...
            }
        });
#endif
    }
}

