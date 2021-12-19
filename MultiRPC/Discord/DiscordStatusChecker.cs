using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MultiRPC.Discord.Status;
using MultiRPC.UI;
using TinyUpdate.Http.Extensions;

namespace MultiRPC.Discord
{
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
            var response = await App.HttpClient.GetResponseMessage(new HttpRequestMessage(HttpMethod.Get,
                "https://discordstatus.com/api/v2/components.json"));

            if (response is null || !response.IsSuccessStatusCode)
            {
                return DiscordStatus.MajorOutage;
            }

            var data = await response.Content.ReadFromJsonAsync(DataContext.Default.Data);
            var status = data?.Components[0].Status switch
            {
                "operational" => DiscordStatus.Operational,
                "degraded_performance" => DiscordStatus.Degraded,
                "partial_outage" => DiscordStatus.PartialOutage,
                "major_outage" => DiscordStatus.MajorOutage,
                _ => DiscordStatus.MajorOutage
            };

            return status;
        }
    }
}