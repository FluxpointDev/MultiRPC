using System.Text.Json;

namespace MultiRPC.Core
{
    /// <summary>
    /// Contains objects that will not change value at any time thoughout the span of the clients useage
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// MultiRPC Application ID
        /// </summary>
        public const long MultiRPCID = 450894077165043722;

        /// <summary>
        /// Afk's Application ID
        /// </summary>
        public const long AfkID = 469643793851744257;

        /// <summary>
        /// How many times you should attempt downloading files
        /// </summary>
        public const int RetryCount = 10;

        /// <summary>
        /// The Uri to the MultiRPC web page
        /// </summary>
        public const string MultiRPCWebsiteRoot = "https://multirpc.blazedev.me";

        /// <summary>
        /// The app developer
        /// </summary>
        public const string AppDeveloper = "Builderb#0001";

        /// <summary>
        /// The discord server's invite code
        /// </summary>
        public const string ServerInviteCode = "TjF6QDC";

        /// <summary>
        /// Serializer for json
        /// </summary>
        public static JsonSerializerOptions JsonSerializer { get; } = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        /// <summary>
        /// The theme's file extension
        /// </summary>
        public const string ThemeFileExtension = ".multirpctheme";

        /// <summary>
        /// The folder with all the languages
        /// </summary>
        public const string LanguageFolder = "Lang";
    }
}
