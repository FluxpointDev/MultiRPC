using System;
using System.Linq;
using MultiRPC.Core.Rpc;
using MultiRPC.Core.Page;
using Microsoft.Extensions.DependencyInjection;

namespace MultiRPC.Core
{
    /// <summary>
    /// This manages all the services that we currently have
    /// </summary>
    public static class ServiceManager
    {
        /// <summary>Collection of all the services we need to process</summary>
        private static readonly ServiceCollection Service = new ServiceCollection();

        /// <summary>
        /// This is the provider for all our services that we currently have
        /// </summary>
        public static IServiceProvider? ServiceProvider;

        private static bool HasServiceProvider => ServiceProvider != null;

        //TODO: Use source generator for this so we don't have to
        //update this when we have a new Required service
        private static readonly Type[] RequiredServices =
        {
            typeof(ISidePage)
        };
        
        public static void AddSingleton<T, T2>() 
            where T : class
            where T2 : class, T
        {
            Service.AddSingleton<T, T2>();
        }
        
        public static void AddSingleton<T>(T x) 
            where T : class
        {
            Service.AddSingleton(y => x);
        }
        
        public static void AddSingleton<T>(Func<IServiceProvider, T> x) 
            where T : class
        {
            Service.AddSingleton<T>(x);
        }
        
        public static void AddSingleton<T>() 
            where T : class
        {
            Service.AddSingleton<T>();
        }

        public static void AddTransient<T>() 
            where T : class
        {
            Service.AddTransient<T>();
        }

        
        public static void AddScoped<T>() 
            where T : class
        {
            Service.AddScoped<T>();
        }

        /// <summary>
        /// This processes all the services that been given and prepares them for being used
        /// </summary>
        /// <exception cref="Exception">We already been processed</exception>
        public static void ProcessService()
        {
            if (HasServiceProvider)
            {
                throw new Exception($"{nameof(ServiceProvider)} has already been created");
            }
            AddSingleton(new RpcClient());

            //Lets check for any services that we are missing and tell them what we are missing
            var missingServices = RequiredServices.Where(x => Service.All(y => x.Name != y.ServiceType.Name));
            if (missingServices.Any())
            {
                throw new Exception($"We are missing required services, the missing services are\r\n* " +
                    $"{string.Join("\r\n* ", missingServices)}");
            }

            ServiceProvider = Service.BuildServiceProvider();
        }
    }
}
