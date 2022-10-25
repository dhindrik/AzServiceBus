using System;
namespace ServiceBusManager;

public static class Resolver
{
    private static IServiceProvider? services;

    public static void Init(IServiceProvider serviceProvider)
    {
        services = serviceProvider;
    }

    public static T? Resolve<T>() where T:class
    {
        if(services == null)
        {
            throw new NullReferenceException("You have to run the Init method first.");
        }

        return services.GetService(typeof(T)) as T;
    }
}

