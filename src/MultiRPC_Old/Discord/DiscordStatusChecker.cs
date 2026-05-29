using System.Net.Http.Json;
using MultiRPC.Extensions;
using MultiRPC.UI;

namespace MultiRPC.Discord;

public enum DiscordStatus
{
    Operational, 
    Degraded, 
    PartialOutage, 
    MajorOutage
}
    
public static class DiscordStatusChecker
{
    public static async Task<DiscordStatus> GetStatus()
    {
        var response = await App.HttpClient.GetResponseMessageAsync("https://discordstatus.com/api/v2/components.json");
        if (response is null || !response.IsSuccessStatusCode)
        {
            return DiscordStatus.MajorOutage;
        }

        var data = await response.Content.ReadFromJsonAsync(StatusContext.Default.Status);
        return data?.Components[0].Status ?? DiscordStatus.MajorOutage;
    }
}