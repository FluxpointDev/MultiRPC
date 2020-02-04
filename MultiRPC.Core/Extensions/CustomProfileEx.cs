using DiscordRPC;

namespace MultiRPC.Core.Extensions
{
    /// <summary>
    /// Extensions for <see cref="CustomProfile"/>
    /// </summary>
    public static class CustomProfileEx
    {
        /// <summary>
        /// Makes this into a <see cref="RichPresence"/>
        /// </summary>
        /// <param name="custom"><see cref="CustomProfile"/> to make into a <see cref="RichPresence"/></param>
        public static RichPresence ToRichPresence(this CustomProfile custom) =>
        new RichPresence
        {
            Assets = new Assets
            {
                LargeImageKey = custom.LargeKey,
                LargeImageText = custom.LargeText,
                SmallImageKey = custom.SmallKey,
                SmallImageText = custom.SmallText
            },
            Details = custom.Text1,
            State = custom.Text2,
            Timestamps = custom.ShowTime ? new Timestamps
            {
                Start = Rpc.Rpc.RpcStartTime
            } : null
        };
    }
}
