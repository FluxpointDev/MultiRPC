using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TinyUpdate.Core.Logging;
using TinyUpdate.Http.Extensions;

namespace MultiRPC.Rpc
{
    public static class IDChecker
    {
        private static readonly ILogging Logger = LoggingCreator.CreateLogger(nameof(Language));
        private static readonly HttpClient Client = new HttpClient();

        /// <summary>
        /// This checks the ID that is given to us
        /// </summary>
        /// <param name="id">ID to check</param>
        /// <returns>Fail: returns false with error message
        /// Success: returns true with name that is linked to that ID</returns>
        public static async Task<(bool Successful, string? ResultMessage)> Check(long id) 
        {
            HttpResponseMessage? responseMessage =
                await Client.GetResponseMessage(
                    new HttpRequestMessage(HttpMethod.Get, 
                        $"https://discordapp.com/api/v6/oauth2/applications/{id}/rpc")); ;

            if (responseMessage is null or { IsSuccessStatusCode: false })
            {
                return (false, Language.GetText("DiscordAPIDown"));
            }
            if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                return (false, Language.GetText("ClientIDIsNotValid"));
            }

            var response = JsonSerializer.Deserialize<ClientCheckResult>(await responseMessage.Content.ReadAsStreamAsync());

            return string.IsNullOrEmpty(response?.Message) ?
                (false, $"{Language.GetText("ClientIDIsNotValid")}\r\n{response?.Message}")
                : (true, response.Name);
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
