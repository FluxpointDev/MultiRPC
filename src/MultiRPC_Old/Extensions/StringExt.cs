using System.Text;

namespace MultiRPC.Extensions;

public static class StringExt
{
    public static bool CheckBytes(this string s, int max) => Encoding.UTF8.GetByteCount(s) <= max;
        
    public static string Base64Encode(this string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    public static string Base64Decode(this string base64EncodedData)
    {
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }
}