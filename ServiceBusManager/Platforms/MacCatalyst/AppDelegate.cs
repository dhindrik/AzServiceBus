using Foundation;
using UIKit;

namespace ServiceBusManager;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIKit.UIApplication application, NSDictionary launchOptions)
    {
        base.FinishedLaunching(application, launchOptions);


        UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(1800);

        if (launchOptions != null)
        {
           
            if (launchOptions.ContainsKey(UIApplication.LaunchOptionsLocalNotificationKey))
            {
                var localNotification = launchOptions[UIApplication.LaunchOptionsLocalNotificationKey] as UILocalNotification;

                if (localNotification != null)
                {              
                    UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

                    Shell.Current.GoToAsync("//DeadLetters");
                }
            }
        }

        return true;
    }


    [Export("application:performFetchWithCompletionHandler:")]
    public void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
    {
        var dateTime = DateTimeOffset.UtcNow;       


        var service = Resolver.Resolve<IServiceBusService>();
        var log = Resolver.Resolve<ILogService>();

        if (service == null)
        {
            completionHandler(UIBackgroundFetchResult.Failed);
            return;
        }                  

        var compareDate = Preferences.Get("LastDeadLetter", dateTime.DateTime);

        var countTask = service.CheckNewDeadLetters(new DateTimeOffset(compareDate));
        countTask.RunSynchronously();

        if(countTask.Result > 0)
        {
            var notification = new UILocalNotification();

            notification.FireDate = NSDate.FromTimeIntervalSinceNow(1);

            notification.AlertAction = "New dead letters";
            notification.AlertBody = "There are new dead letters in your ServiceBus";

            notification.ApplicationIconBadgeNumber += countTask.Result;

            notification.SoundName = UILocalNotification.DefaultSoundName;

            
            UIApplication.SharedApplication.ScheduleLocalNotification(notification);

            completionHandler(UIBackgroundFetchResult.NewData);
        }
        else
        {
            completionHandler(UIBackgroundFetchResult.NoData);
        }

        Preferences.Set("LastDeadLetter", dateTime.DateTime);
    }
}

