namespace MultiRPC.Core.Enums
{
    /// <summary>
    /// What we should start when we load up the clinet
    /// </summary>
    public enum AutoStart
    {
        /// <summary>
        /// Don't load up anything
        /// </summary>
        No,
        /// <summary>
        /// Load up MultiRPC
        /// </summary>
        MultiRPC,
        /// <summary>
        /// Load up Custom
        /// </summary>
        Custom
    }
}
