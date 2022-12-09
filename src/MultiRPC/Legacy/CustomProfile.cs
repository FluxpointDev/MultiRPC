using MultiRPC.Rpc;

namespace MultiRPC.Legacy;

public class CustomProfile
{
    public string ClientID { get; set; }
    public string LargeKey { get; set; }
    public string LargeText { get; set; }
    public string Name { get; set; }
    public bool ShowTime { get; set; }
    public string SmallKey { get; set; }
    public string SmallText { get; set; }
    public string Text1 { get; set; }
    public string Text2 { get; set; }
    public string? Button1Name { get; set; }
    public string? Button1Url { get; set; }
    public string? Button2Name { get; set; }
    public string? Button2Url { get; set; }

    public Presence? ToRichPresence()
    {
        if (!long.TryParse(ClientID, out var id))
        {
            return null;
        }
        
        return new Presence(Name, id)
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
                Button1Text = Button1Name ?? string.Empty,
                Button1Url = Button1Url ?? string.Empty,
                Button2Text = Button2Name ?? string.Empty,
                Button2Url = Button2Url ?? string.Empty,
            }
        };
    }
}