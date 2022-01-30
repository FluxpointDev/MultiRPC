﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MultiRPC.Extensions;
using MultiRPC.UI;

namespace MultiRPC.Discord;

//Thanks to https://gist.github.com/SilverCory/99c99f2dbafb3cbdafe60edd9c9db121 for showing the endpoints we're gonna need for this!
public class ProfileAssetsManager
{
    private static readonly List<ProfileAssetsManager> Managers = new List<ProfileAssetsManager>();
    private readonly string _baseUrl;
    private readonly long _id;
    public ProfileAssetsManager(long id)
    {
        _id = id;
        _baseUrl = "https://discordapp.com/api/oauth2/applications/" + id + "/assets";
        _lazyAssets = new Lazy<DiscordAsset[]?>(GetAssets);
    }

    public static ProfileAssetsManager GetOrAddManager(long id)
    {
        var manager = Managers.FirstOrDefault(x => x._id == id);
        if (manager == null)
        {
            manager = new ProfileAssetsManager(id);
            Managers.Add(manager);
        }

        return manager;
    }

    private Lazy<DiscordAsset[]?> _lazyAssets;
    public DiscordAsset[]? Assets => _lazyAssets.Value;

    public DiscordAsset[]? GetAssets()
    {
        HttpResponseMessage response;
        try
        {
            response = App.HttpClient.Send(new HttpRequestMessage(HttpMethod.Get, _baseUrl));
            if (response is not { IsSuccessStatusCode: true })
            {
                return null;
            }
        }
        catch (Exception e)
        {
            //TODO: Log
            return null;
        }

        var assetsStream = response.Content.ReadAsStream();
        return JsonSerializer.Deserialize<DiscordAsset[]>(assetsStream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
    
    public async Task<DiscordAsset[]?> GetAssetsAsync()
    {
        if (_lazyAssets.IsValueCreated)
        {
            return _lazyAssets.Value;
        }
        
        var response = await App.HttpClient.GetResponseMessage(_baseUrl);
        if (response is not { IsSuccessStatusCode: true })
        {
            return null;
        }

        var assets = await response.Content.ReadFromJsonAsync<DiscordAsset[]>();
        _lazyAssets = new Lazy<DiscordAsset[]?>(assets);
        return assets;
    }

    public Uri? GetUri(ulong? assetId) => DiscordAsset.GetUri(_id.ToString(), string.Empty, assetId);
    public Uri? GetUri(string assetName)
    {
        var assetId = Assets?.FirstOrDefault(x => x.Name == assetName)?.ID;
        return assetId == null ? null : DiscordAsset.GetUri(_id.ToString(), string.Empty, assetId);
    }
}

public class DiscordAsset
{
    [JsonPropertyName("id")]
    public ulong ID { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }
    
    public static Uri? GetUri(string applicationId, string? key, ulong? assetId)
    {
        if (assetId.HasValue)
        {
            return new Uri("https://cdn.discordapp.com/app-assets/" + applicationId + "/" + assetId + ".png");
        }

        if (!string.IsNullOrWhiteSpace(key)
            && key.StartsWith("mp:external/")
            && Uri.TryCreate("https://media.discordapp.net/external/" + key[12..], UriKind.Absolute, out var url))
        {
            return url;
        }

        return null;
    }
}