using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;

namespace MultiRPC.Core.Rpc
{
    public static class ClientIDChecker
    {
        private static readonly HttpClient Client = new HttpClient();

        /// <summary>
        /// This checks the ID that is given to us
        /// </summary>
        /// <param name="id">ID to check</param>
        /// <returns>Fail: returns false with error message
        /// Success: returns true with name that is linked to that ID</returns>
        public static async Task<(bool Successful, string? resultMessage)> CheckID(long id) 
        {
            HttpResponseMessage? responseMessage;
            try
            {
                responseMessage = await Client.GetAsync($"https://discordapp.com/api/v6/oauth2/applications/{id}/rpc");
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
                return (false, LanguagePicker.GetLineFromLanguageFile("DiscordAPIDown"));
            }

            if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                return (false, LanguagePicker.GetLineFromLanguageFile("ClientIDIsNotValid"));
            }

            var responseJson = await responseMessage.Content.ReadAsStringAsync();
            var response = System.Text.Json.JsonSerializer.Deserialize<ClientCheckResult>(responseJson);

            if (!string.IsNullOrEmpty(response?.Message))
            {
                return (false, $"{LanguagePicker.GetLineFromLanguageFile("ClientIDIsNotValid")}\r\n{response.Message}");
            }

            return (true, response?.Name);
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
