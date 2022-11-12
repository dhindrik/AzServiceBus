using System;

namespace ServiceBusManager.Services
{
	public class AppCenterLogService : ILogService
	{
		public AppCenterLogService()
		{
		}

        public Task LogEvent(string eventName, Dictionary<string, string>? properties = null)
        {
            return Task.CompletedTask;
        }

        public Task LogException(Exception ex)
        {
            return Task.CompletedTask;
        }

        public Task LogPageView(string pageName, Dictionary<string, string>? properties = null)
        {
            return Task.CompletedTask;
        }
    }
}

