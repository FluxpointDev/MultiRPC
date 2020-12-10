using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;

namespace MultiRPC.Core.Rpc
{
    public static class ClientIDChecker
    {
        static HttpClient Client = new HttpClient();

        /// <summary>
        /// This checks the ID that is given to us
        /// </summary>
        /// <param name="id">ID to check</param>
        /// <returns>Fail: returns false with error message
        /// Success: returns true with name that is linked to that ID</returns>
        public static async Task<(bool Successful, string resultMessage)> CheckID(long id) 
        {
            HttpResponseMessage responseMessage = null;
            try
            {
                responseMessage = await Client.GetAsync($"https://discordapp.com/api/v6/oauth2/applications/{id}/rpc");
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
                return (false, await LanguagePicker.GetLineFromLanguageFile("DiscordAPIDown"));
            }

            if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                return (false, await LanguagePicker.GetLineFromLanguageFile("ClientIDIsNotValid"));
            }

            var responseJSON = await responseMessage.Content.ReadAsStringAsync();
            var response = JObject.Parse(responseJSON);
            var error = response?.Value<string>("messgae");

            if (!string.IsNullOrEmpty(error))
            {
                return (false, $"{await LanguagePicker.GetLineFromLanguageFile("ClientIDIsNotValid")}\r\n{error}");
            }

            return (true, response?.Value<string>("name"));
        }
    }
}
