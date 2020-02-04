using MultiRPC.Core.Enums;

namespace MultiRPC.Core.Managers
{
    /// <summary>
    /// Helper for getting colours
    /// </summary>
    /// <typeparam name="TIcon">What the colour's type in GUI</typeparam>
    public interface IMultiRPCColours<TColour>
    {
        /// <summary>
        /// Get the colour from the colour enum to be used in the client
        /// </summary>
        /// <param name="multirpcColours">colour enum to get the colour from</param>
        TColour EnumToColour(MultiRPCColours multirpcColours);

        /// <summary>
        /// Get the colour enum from the colour to be used in the client
        /// </summary>
        /// <param name="multirpcColours">colour to get the colour enum from</param>
        MultiRPCColours ColourToEnum(TColour multirpcColours);

        /// <summary>
        /// Gets a string that can be used for the clients need
        /// </summary>
        /// <param name="multirpcColours">colour to get a string for</param>
        string EnumToString(MultiRPCColours multirpcColours);
    }
}
