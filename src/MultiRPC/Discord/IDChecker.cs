using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MultiRPC.Extensions;
using MultiRPC.UI;

namespace MultiRPC.Discord;

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
            await App.HttpClient.GetResponseMessageAsync($"https://discordapp.com/api/v6/oauth2/applications/{id}/rpc");

        if (responseMessage is null or { IsSuccessStatusCode: false })
        {
            return (false, Language.GetText(LanguageText.DiscordAPIDown));
        }
        if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
        {
            return (false, Language.GetText(LanguageText.ClientIDIsNotValid));
        }

        var response = await responseMessage.Content
            .ReadFromJsonAsync(ClientCheckResultContext.Default.ClientCheckResult);

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