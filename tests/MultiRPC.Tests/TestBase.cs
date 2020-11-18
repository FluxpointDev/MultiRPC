using System;
using NUnit.Framework;
using Uno.UITest;
using Uno.UITests.Helpers;

namespace MultiRPC.Tests
{
	public class TestBase
	{
		private IApp _app;

		static TestBase()
		{
			// Change this to your android app name
			//AppInitializer.TestEnvironment.AndroidAppName = "com.example.myapp";

			// Change this to the URL of your WebAssembly app, found in launchsettings.json
			AppInitializer.TestEnvironment.WebAssemblyDefaultUri = "http://localhost:CHANGEME";

			// Change this to the bundle ID of your app
			AppInitializer.TestEnvironment.iOSAppName = "com.example.myapp";

			// Change this to the iOS device you want to test on
			//AppInitializer.TestEnvironment.iOSDeviceNameOrId = "iPad Pro (12.9-inch) (3rd generation)";

			// The current platform to test.
			AppInitializer.TestEnvironment.CurrentPlatform = Uno.UITest.Helpers.Queries.Platform.Browser;

#if DEBUG
			// Show the running tests in a browser window
			AppInitializer.TestEnvironment.WebAssemblyHeadless = false;
#endif

			// Start the app only once, so the tests runs don't restart it
			// and gain some time for the tests.
			AppInitializer.ColdStartApp();
		}

		protected IApp App { get; set; }

		[SetUp]
		public void StartApp()
		{
			// Attach to the running application, for better performance
			App = AppInitializer.AttachToApp();
		}
	}
}
