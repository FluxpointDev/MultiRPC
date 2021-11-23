using MultiRPC.Rpc;
using RichPresence = DiscordRPC.RichPresence;

namespace MultiRPC.Extensions
{
    public static class RpcExt
    {
        public static RpcProfile ToProfile(this RichPresence presence)
        {
            return new RpcProfile
            {
                State = presence.State,
                Details = presence.Details,
                ShowTime = presence.Timestamps != null,
                SmallKey = presence.Assets.SmallImageKey,
                SmallText = presence.Assets.SmallImageText,
                LargeKey = presence.Assets.LargeImageKey,
                LargeText = presence.Assets.LargeImageText,
                Button1Text = presence.Buttons[0].Label,
                Button1Url = presence.Buttons[0].Url,
                Button2Text = presence.Buttons[1].Label,
                Button2Url = presence.Buttons[1].Url,
            };
        }
    }
}