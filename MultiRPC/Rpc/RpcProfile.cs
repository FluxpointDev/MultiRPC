using System;
using System.Collections.Generic;
using DiscordRPC;
using Fonderie;

namespace MultiRPC.Rpc
{
    public partial class RpcProfile
    {
        [GeneratedProperty] private string _state = string.Empty;
        [GeneratedProperty] private string _details = string.Empty;
        [GeneratedProperty] private string _largeKey = string.Empty;
        [GeneratedProperty] private string _largeText = string.Empty;
        [GeneratedProperty] private string _smallKey = string.Empty;
        [GeneratedProperty] private string _smallText = string.Empty;
        [GeneratedProperty] private bool _showTime;
        [GeneratedProperty] private string _button1Text = string.Empty;
        [GeneratedProperty] private string _button1Url = string.Empty;
        [GeneratedProperty] private string _button2Text = string.Empty;
        [GeneratedProperty] private string _button2Url = string.Empty;

        public DiscordRPC.RichPresence ToRichPresence()
        {
            var buttons = new List<Button>();
            if (Uri.TryCreate(_button1Url, UriKind.Absolute, out _))
            {
                buttons.Add(new Button { Label = _button1Text, Url = _button1Url });
            }
            if (Uri.TryCreate(_button2Url, UriKind.Absolute, out _))
            {
                buttons.Add(new Button { Label = _button2Text, Url = _button2Url });
            }
            
            return new DiscordRPC.RichPresence
            {
                State = _state,
                Details = _details,
                Timestamps = _showTime ? Timestamps.Now : null,
                Assets = new Assets()
                {
                    LargeImageKey = _largeKey,
                    LargeImageText = _largeText,
                    SmallImageKey = _smallKey,
                    SmallImageText = _smallText
                },
                Buttons = buttons.ToArray()
            };
        }
    }
}