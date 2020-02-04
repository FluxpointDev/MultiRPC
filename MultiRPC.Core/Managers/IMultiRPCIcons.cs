using MultiRPC.Core.Enums;

namespace MultiRPC.Core.Managers
{
    /// <summary>
    /// Helper for getting icons
    /// </summary>
    /// <typeparam name="TIcon">What the icon's type in GUI</typeparam>
    public interface IMultiRPCIcons<TIcon>
        where TIcon : class
    {
        /// <summary>
        /// Get the icon from the icons enum to be used in the client
        /// </summary>
        /// <param name="multirpcIcon">Icon enum to get the icon from</param>
        TIcon EnumToIcon(MultiRPCIcons multirpcIcon);

        /// <summary>
        /// Get the icons enum from the icon being used in the client
        /// </summary>
        /// <param name="multirpcIcon">Icon to get the icon enum from</param>
        MultiRPCIcons IconToEnum(TIcon multirpcIcon);

        /// <summary>
        /// Gets a string that can be used for the clients need
        /// </summary>
        /// <param name="multirpcColours">icon to get a string for</param>
        string EnumToString(MultiRPCIcons multirpcColours);
    }
}
