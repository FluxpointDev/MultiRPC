using System.Threading.Tasks;

namespace MultiRPC.Functions
{
    public static class Updater
    {
        static Updater()
        {
#if NETCORE
            _Updater = new SimpleUpdater();
#else
            _Updater = new ClickOnceUpdater();
#endif
        }

        private static IUpdate _Updater;

        public static bool IsChecking => _Updater.IsChecking;
        public static bool IsUpdating => _Updater.IsUpdating;
        public static bool BeenUpdated => _Updater.BeenUpdated;

        public static async Task Check(bool showNoUpdateMessage = false)
        {
            _Updater?.Check(showNoUpdateMessage);
        }

        public static async Task Update()
        {
            _Updater?.Update();
        }
    }
}