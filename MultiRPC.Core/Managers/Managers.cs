namespace MultiRPC.Core.Managers
{
    /// <summary>
    /// Where you should contain all your Manager classes
    /// </summary>
    public class Managers<TPage, TIcon> 
        where TPage : class
        where TIcon : class
    {
        /// <summary>
        /// The <see cref="Managers{TPage, TIcon}"/> that is being used
        /// </summary>
        public static Managers<TPage, TIcon> Current { get; private set; }

        /// <summary>
        /// The manager for the main page of the client
        /// </summary>
        public IMainPageManager<TPage> MainPageManager { get; private set; }

        /// <summary>
        /// Logic for the icons that the client might use
        /// </summary>
        public IMultiRPCIcons<TIcon> MultiRPCIcon { get; private set; }

        /// <summary>
        /// Sets up <see cref="Current"/>, can only be set up once by the client
        /// </summary>
        /// <param name="mainPageManager"><see cref="IMainPageManager{TPage}"/> to use</param>
        /// <param name="multiRpcIcons"><see cref="IMultiRPCIcons{TIcon}"/> to use</param>
        public static void Setup(IMainPageManager<TPage> mainPageManager, IMultiRPCIcons<TIcon> multiRpcIcons) =>
            Current ??= new Managers<TPage, TIcon>
            {
                MainPageManager = mainPageManager,
                MultiRPCIcon = multiRpcIcons,
            };
    }
}
