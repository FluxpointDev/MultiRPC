using Avalonia.Controls;

namespace MultiRPC.Extensions;

public static class ResourceDictionaryExt
{
    public static void UpdateIfDifferent<T>(this IResourceDictionary resourceDictionary, string resourceDictionaryName, T item)
    {
        if (!resourceDictionary.ContainsKey(resourceDictionaryName) || !resourceDictionary[resourceDictionaryName].Equals(item))
        {
            resourceDictionary[resourceDictionaryName] = item;
        }
    }
}