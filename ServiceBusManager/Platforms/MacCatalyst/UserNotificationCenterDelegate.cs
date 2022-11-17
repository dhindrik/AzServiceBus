using System;
using Foundation;
using UserNotifications;

namespace ServiceBusManager.Platforms.MacCatalyst
{
	public class UserNotificationCenterDelegate : UNUserNotificationCenterDelegate
    {
		public UserNotificationCenterDelegate()
		{
		}

        [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
        public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            Shell.Current.GoToAsync("//DeadLetters");
        }
    }
}

