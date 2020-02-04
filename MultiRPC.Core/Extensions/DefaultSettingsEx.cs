using DiscordRPC;
using System.Linq;

namespace MultiRPC.Core.Extensions
{
    /// <summary>
    /// Extensions for <see cref="DefaultSettings"/>
    /// </summary>
    public static class DefaultSettingsEx
    {
        /// <summary>
        /// Makes this into a <see cref="RichPresence"/>
        /// </summary>
        /// <param name="defaultSettings"><see cref="DefaultSettings"/> to make into a <see cref="RichPresence"/></param>
        public static RichPresence ToRichPresence(this DefaultSettings defaultSettings) =>
        new RichPresence
        {
            Assets = new Assets
            {
                LargeImageKey = Data.MultiRPCImages.Keys.ElementAt(defaultSettings.LargeKey).GetCorrectString(),
                LargeImageText = defaultSettings.LargeText,
                SmallImageKey = Data.MultiRPCImages.Keys.ElementAt(defaultSettings.SmallKey).GetCorrectString(),
                SmallImageText = defaultSettings.SmallText
            },
            Details = defaultSettings.Text1,
            State = defaultSettings.Text2,
            Timestamps = defaultSettings.ShowTime ? new Timestamps
            {
                Start = Rpc.Rpc.RpcStartTime
            } : null
        };

        private static string GetCorrectString(this string s) 
        {
            if (s == LanguagePicker.GetLineFromLanguageFile("Christmas"))
            {
                return "christmas";
            }
            else if (s == LanguagePicker.GetLineFromLanguageFile("Present"))
            {
                return "present";
            }
            else if (s == LanguagePicker.GetLineFromLanguageFile("Popcorn"))
            {
                return "popcorn";
            }
            else if (s == LanguagePicker.GetLineFromLanguageFile("Games"))
            {
                return "games";
            }

            return s.ToLower();
        }
    }
}
