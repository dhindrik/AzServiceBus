using System;
//using Microsoft.AppCenter.Analytics;
//using Microsoft.AppCenter.Crashes;

namespace ServiceBusManager.Services
{
	public sealed class AppCenterLogService : ILogService
	{
		public AppCenterLogService()
		{
		}

        public Task LogEvent(string eventName, Dictionary<string, string>? properties = null)
        {
           // _ = Task.Run(() => Analytics.TrackEvent(eventName, properties));

            return Task.CompletedTask;
        }

        public Task LogException(Exception ex)
        {
            //_ = Task.Run(() => Crashes.TrackError(ex));

            return Task.CompletedTask;
        }

        public Task LogPageView(string pageName)
        {
            var properties = new Dictionary<string, string>()
            {
                {"PageName", pageName}
            };

            //_ = Task.Run(() => Analytics.TrackEvent("PageView", properties));

            return Task.CompletedTask;
        }
    }
}

