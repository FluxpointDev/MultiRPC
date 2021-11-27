using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MultiRPC.UI;
using TinyUpdate.Http.Extensions;

namespace MultiRPC.Discord
{
    public static class IDChecker
    {
        /// <summary>
        /// This checks the ID that is given to us
        /// </summary>
        /// <param name="id">ID to check</param>
        /// <returns>Fail: returns false with error message
        /// Success: returns true with the name that is linked to that ID</returns>
        public static async Task<(bool Successful, string? ResultMessage)> Check(long id) 
        {
            var responseMessage =
                await App.HttpClient.GetResponseMessage(
                    new HttpRequestMessage(HttpMethod.Get, 
                        $"https://discordapp.com/api/v6/oauth2/applications/{id}/rpc")); ;

            if (responseMessage is null or { IsSuccessStatusCode: false })
            {
                return (false, Language.GetText(LanguageText.DiscordAPIDown));
            }
            if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                return (false, Language.GetText(LanguageText.ClientIDIsNotValid));
            }

            var response = JsonSerializer.Deserialize<ClientCheckResult>(await responseMessage.Content.ReadAsStreamAsync());

            return !string.IsNullOrEmpty(response?.Message) ?
                (false, $"{Language.GetText(LanguageText.ClientIDIsNotValid)}\r\n{response.Message}")
                : (true, response!.Name);
        }
    }

    public class ClientCheckResult 
    {
        [JsonConstructor]
        public ClientCheckResult(string message, string name)
        {
            Message = message;
            Name = name;
        }

        [JsonPropertyName("message")]
        public string Message { get; }

        [JsonPropertyName("name")]
        public string Name { get; }
    }
}
