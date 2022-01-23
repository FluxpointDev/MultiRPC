using MultiRPC.Rpc;

namespace MultiRPC.Legacy;

public class CustomProfile
{
    public string ClientID { get; set; } = string.Empty;
    public string LargeKey { get; set; } = string.Empty;
    public string LargeText { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool ShowTime { get; set; }
    public string SmallKey { get; set; } = string.Empty;
    public string SmallText { get; set; } = string.Empty;
    public string Text1 { get; set; } = string.Empty;
    public string Text2 { get; set; } = string.Empty;
    public string Button1Name { get; set; } = string.Empty;
    public string Button1Url { get; set; } = string.Empty;
    public string Button2Name { get; set; } = string.Empty;
    public string Button2Url { get; set; } = string.Empty;

    public RichPresence? ToRichPresence()
    {
        if (!long.TryParse(ClientID, out var id))
        {
            return null;
        }
        
        return new RichPresence(Name, id)
        {
            Profile = new RpcProfile()
            {
                LargeKey = LargeKey,
                LargeText = LargeText,
                ShowTime = ShowTime,
                SmallKey = SmallKey,
                SmallText = SmallText,
                Details = Text1,
                State = Text2,
                Button1Text = Button1Name,
                Button1Url = Button1Url,
                Button2Text = Button2Name,
                Button2Url = Button2Url,
            }
        };
    }
}