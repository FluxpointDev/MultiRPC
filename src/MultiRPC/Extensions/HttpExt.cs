using System;
using System.Net.Http;
using System.Threading.Tasks;
using TinyUpdate.Http.Extensions;

namespace MultiRPC.Extensions;

public static class HttpExt
{
    public static Task<HttpResponseMessage?> GetResponseMessage(this HttpClient httpClient, string url)
    {
        return httpClient.GetResponseMessage(new HttpRequestMessage(HttpMethod.Get, url));
    }
    
    public static Task<HttpResponseMessage?> GetResponseMessage(this HttpClient httpClient, Uri url)
    {
        return httpClient.GetResponseMessage(new HttpRequestMessage(HttpMethod.Get, url));
    }
}