using DiscordRPC;
using PropertyChanged.SourceGenerator;

namespace MultiRPC.Rpc;

public partial class RpcProfile : IEquatable<RpcProfile>
{
    [Notify] private string? _state;
    [Notify] private string? _details;
    [Notify] private string? _largeKey;
    [Notify] private string? _largeText;
    [Notify] private string? _smallKey;
    [Notify] private string? _smallText;
    [Notify] private bool _showTime;
    [Notify] private string? _button1Text;
    [Notify] private string? _button1Url;
    [Notify] private string? _button2Text;
    [Notify] private string? _button2Url;

    public DiscordRPC.RichPresence ToRichPresence()
    {
        var buttons = new List<Button>();
        if (!string.IsNullOrWhiteSpace(_button1Text) 
            && Uri.TryCreate(_button1Url, UriKind.Absolute, out _))
        {
            buttons.Add(new Button { Label = _button1Text, Url = _button1Url });
        }
        if (!string.IsNullOrWhiteSpace(_button2Text) 
            && Uri.TryCreate(_button2Url, UriKind.Absolute, out _))
        {
            buttons.Add(new Button { Label = _button2Text, Url = _button2Url });
        }
            
        return new DiscordRPC.RichPresence
        {
            State = _state,
            Details = _details,
            Timestamps = _showTime ? Timestamps.Now : null,
            Assets = new Assets
            {
                LargeImageKey = _largeKey,
                LargeImageText = _largeText,
                SmallImageKey = _smallKey,
                SmallImageText = _smallText
            },
            Buttons = buttons.ToArray()
        };
    }

    public bool Equals(RpcProfile? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _state == other._state
               && _details == other._details 
               && _largeKey == other._largeKey 
               && _largeText == other._largeText 
               && _smallKey == other._smallKey 
               && _smallText == other._smallText 
               && _showTime == other._showTime
               && _button1Text == other._button1Text 
               && _button1Url == other._button1Url 
               && _button2Text == other._button2Text 
               && _button2Url == other._button2Url;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((RpcProfile)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(_state);
        hashCode.Add(_details);
        hashCode.Add(_largeKey);
        hashCode.Add(_largeText);
        hashCode.Add(_smallKey);
        hashCode.Add(_smallText);
        hashCode.Add(_showTime);
        hashCode.Add(_button1Text);
        hashCode.Add(_button1Url);
        hashCode.Add(_button2Text);
        hashCode.Add(_button2Url);
        return hashCode.ToHashCode();
    }
}