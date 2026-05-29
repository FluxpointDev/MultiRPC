using TinyUpdate.Core.Logging;
using TinyUpdate.Http.Extensions;

namespace MultiRPC.Extensions;

public static class HttpExt
{
    private static readonly ILogging Logging = LoggingCreator.CreateLogger(nameof(HttpExt));
    
    public static Task<HttpResponseMessage?> GetResponseMessageAsync(this HttpClient httpClient, string url)
    {
        return httpClient.GetResponseMessage(new HttpRequestMessage(HttpMethod.Get, url));
    }
    
    public static Task<HttpResponseMessage?> GetResponseMessageAsync(this HttpClient httpClient, Uri url)
    { 
        return httpClient.GetResponseMessage(new HttpRequestMessage(HttpMethod.Get, url));
    }

    public static HttpResponseMessage? GetResponseMessage(this HttpClient httpClient, string url)
    {
        try
        {
            return httpClient.Send(new HttpRequestMessage(HttpMethod.Get, url));
        }
        catch (Exception e)
        {
            Logging.Error(e);
            return null;
        }
    }

    public static HttpResponseMessage? GetResponseMessage(this HttpClient httpClient, Uri url) =>
        GetResponseMessage(httpClient, url.AbsoluteUri);
}