using DiscordRPC;

namespace MultiRPC.Core.Extensions
{
    /// <summary>
    /// Extensions for <see cref="RichPresence"/>
    /// </summary>
    public static class RichPresenceEx
    {
        /// <summary>
        /// Gets if the <see cref="RichPresence"/> can be used
        /// </summary>
        /// <param name="richPresence"><see cref="RichPresence"/> to check</param>
        public static bool CanBeSet(this RichPresence richPresence)
        {
            if (richPresence.Details?.Length == 1)
            {
                return false;
            }

            if (richPresence.State?.Length == 1)
            {
                return false;
            }

            if (richPresence.Assets?.SmallImageText?.Length == 1)
            {
                return false;
            }

            if (richPresence.Assets?.LargeImageText?.Length == 1)
            {
                return false;
            }

            return true;
        }
    }
}
