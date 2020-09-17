using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MultiRPC.Core
{
    public class ServiceManager
    {
        public static ServiceCollection Service = new ServiceCollection();

        public static IServiceProvider? ServiceProvider;

        static bool HasServiceProvider => ServiceProvider != null;

        //TODO: Use source generator for this so we don't have to
        //update this when we have a new Required service
        static Type[] RequiredServices = new[]
        {
            typeof(IAssetProcessor),
            typeof(IFileSystemAccess),
            typeof(ISidePage)
        };

        public static void ProcessService()
        {
            if (HasServiceProvider)
            {
                throw new Exception($"{nameof(ServiceProvider)} has already been created");
            }

            //Lets check for any services that we are missing and tell them what we are missing
            var missingServices = RequiredServices.Where(x => !Service.Any(y => x.Name == y.ServiceType.Name));
            if (missingServices.Any())
            {
                throw new Exception($"We are missing required services, the missing services are\r\n* " +
                    $"{string.Join("\r\n* ", missingServices)}");
            }

            ServiceProvider = Service.BuildServiceProvider();
        }
    }
}
