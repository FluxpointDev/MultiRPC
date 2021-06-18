using Microsoft.Extensions.DependencyInjection;
using MultiRPC.Core.Pages;
using MultiRPC.Shared.UI.Pages;
using MultiRPC.Shared.UI.Pages.Custom;

namespace MultiRPC
{
    public static class AddDefaultPages
    {
        public static void AddDefaults(this ServiceCollection services)
        {
            //These pages have to be added first as they have multiple implementations for different things
            //but we need to only have one instance of them pages
            services.AddSingleton<MultiRPCPage>();
            services.AddSingleton<CustomPage>();
            services.AddSingleton<CustomPageContainer>();

            //Add their IRpcPage imp first
            services.AddSingleton<IRpcPage>(x => x.GetRequiredService<MultiRPCPage>());
            services.AddSingleton<IRpcPage>(x => x.GetRequiredService<CustomPage>());

            //Now add their SidePage imp with the other pages
            services.AddSingleton<ISidePage>(x => x.GetRequiredService<MultiRPCPage>());
            services.AddSingleton<ISidePage>(x => x.GetRequiredService<CustomPageContainer>());
            services.AddSingleton<ISidePage, SettingsPage>();
            services.AddSingleton<ISidePage, LoggingPage>();
            services.AddSingleton<ISidePage, CreditsPage>();
            services.AddSingleton<ISidePage, ThemeEditorPage>();

#if DEBUG
            //Add any debugging pages into here
            //ServiceManager.Service.AddSingleton<ISidePage, RPCViewTestPage>();
#endif
            //Add the FileSystemAccess service because UWP be a pain and make their own and not using System.IO
            //like everyone else 😑
            /*ServiceManager.AddSingleton<IFileSystemAccess, FileSystemAccess>();

            //Add our asset processors so we can use assets xP
            ServiceManager.AddSingleton<IAssetProcessor, PageIconProcessor>();
            ServiceManager.AddSingleton<IAssetProcessor, LogoProcessor>();
            ServiceManager.AddSingleton<IAssetProcessor, GifProcessor>();*/
        }
    }
}
