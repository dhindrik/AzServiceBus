using System.Threading.Tasks;
using AVFoundation;
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
    private const string BgProcessingIdentifier = "se.hindrikes.azservicebus.fetch";

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIKit.UIApplication application, NSDictionary launchOptions)
    {
        base.FinishedLaunching(application, launchOptions);
        var service = Resolver.Resolve<ILogService>();
        try
        {
            UNUserNotificationCenter.Current.Delegate = new UserNotificationCenterDelegate();

            // Request notification permissions from the user
            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert |
                UNAuthorizationOptions.Badge |
                UNAuthorizationOptions.Sound, async (approved, err) =>
                {
                    if (service != null)
                    {
                        if (err != null)
                        {
                            await service!.LogException(new Exception(err.ToString()));
                        }
                    }
                });

            //CheckPremium();
        }
        catch (Exception ex)
        {


            if (service != null)
            {
                service.LogException(ex);
            }
        }

        _ = Task.Run(async () =>
        {
            var premiumService = Resolver.Resolve<IPremiumService>();

            while (true)
            {
#if DEBUG
                await Task.Delay(10000);
#else
                await Task.Delay(300000);
#endif

                if (premiumService!.HasPremium())
                {
                    await service!.LogEvent("StartingAsyncTask");

                    await CheckForDeadLetters();
                }
            }
        });

        return true;
    }

    public override void DidEnterBackground(UIApplication application)
    {
        base.DidEnterBackground(application);

        var service = Resolver.Resolve<ILogService>();

        if (service != null)
        {
            service.LogEvent(nameof(DidEnterBackground));
        }
    }

    private async static Task<bool> CheckForDeadLetters()
    {
        var log = Resolver.Resolve<ILogService>();

        try
        {
            var dateTime = DateTimeOffset.UtcNow;

            var service = Resolver.Resolve<IServiceBusService>();

            if (service == null)
            {
                return false;
            }

            var compareDate = Preferences.Get("LastDeadLetter", dateTime.DateTime);

            var count = await service.CheckNewDeadLetters(new DateTimeOffset(compareDate));

            Preferences.Set("LastDeadLetter", dateTime.DateTime);

            if (count > 0)
            {
                SendNotification(count);
            }

            return true;
        }
        catch (Exception ex)
        {
            if (log != null)
            {
                await log.LogException(ex);
            }
        }

        return false;
    }

    private bool CheckPremium()
    {
        var service = Resolver.Resolve<IPremiumService>();

        if (service == null)
        {
            throw new ArgumentNullException("Add IFeatureService to IoC");
        }

        if (service.HasPremium())
        {

            var result = BGTaskScheduler.Shared.Register(BgProcessingIdentifier, null, async (task) =>
            {
                var log = Resolver.Resolve<ILogService>();

                if (log != null)
                {
                    await log.LogEvent("BackgroundFetchStarting");
                }

                var result = await CheckForDeadLetters();

                task.SetTaskCompleted(result);

                if (log != null)
                {
                    await log.LogEvent("BackgroundFetchCompleted");
                }


                ScheduleBackgroundCheck();
            });

            ScheduleBackgroundCheck();

            return true;
        }
        else
        {
            service.PremiumChanged += Service_FeatureChanged;
        }

        return false;
    }
    //e -l objc -- (void)[[BGTaskScheduler sharedScheduler] _simulateLaunchForTaskWithIdentifier:@"se.hindrikes.azservicebus.fetch"]
    private void ScheduleBackgroundCheck()
    {
        var request = new BGProcessingTaskRequest(BgProcessingIdentifier);
        request.RequiresNetworkConnectivity = true;
        request.RequiresExternalPower = false;

        BGTaskScheduler.Shared.Submit(request, out var err);
        var log = Resolver.Resolve<ILogService>();
        if (err != null)
        {
            if (log != null)
            {
                log.LogException(new Exception(err.ToString()));
            }
        }

        if (log != null)
        {
            log.LogEvent(nameof(ScheduleBackgroundCheck));
        }
    }

    private void Service_FeatureChanged(object? sender, EventArgs e)
    {
        var service = Resolver.Resolve<IPremiumService>();

        if (CheckPremium())
        {
            if (service == null)
            {
                throw new ArgumentNullException("Add IFeatureService to IoC");
            }

            service.PremiumChanged -= Service_FeatureChanged;
        }
    }



    private static void SendNotification(int badgeNumber)
    {
        var shouldSend = Preferences.Default.Get<bool>("Notifications", false);

        if (!shouldSend)
            return;

#if MACCATALYST
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var settings = await UNUserNotificationCenter.Current.GetNotificationSettingsAsync();

            if(settings.AuthorizationStatus != UNAuthorizationStatus.Authorized)
            {
                return;
            }

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
                    var log = Resolver.Resolve<ILogService>();
                    log!.LogException(new Exception(err.ToString()));
                }
            });
        });
#endif
    }
}

