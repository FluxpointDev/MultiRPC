using System;
using System.Text;

namespace MultiRPC.Core.Extensions
{
    /// <summary>
    /// Some small but useful logic (as extensions)
    /// </summary>
    public static class UtilsEx
    {
        /// <summary>
        /// Allows you to encode <see cref="plainText"/> into a base64 string
        /// </summary>
        /// <param name="plainText">string to encode</param>
        public static string Base64Encode(this string plainText) =>
            Convert.ToBase64String(Encoding.ASCII.GetBytes(plainText));

        /// <summary>
        /// Allows you to decode <see cref="base64EncodedData"/> into a string
        /// </summary>
        /// <param name="base64EncodedData">string to decode</param>
        public static string Base64Decode(this string base64EncodedData) =>
            Convert.ToBase64String(Convert.FromBase64String(base64EncodedData));
        
        /// <summary>
        /// Gets if the string is under an amount of bytes
        /// </summary>
        /// <param name="s">string to check</param>
        /// <param name="byteLength">amount of bytes it needs to under</param>
        /// <returns></returns>
        public static bool UnderAmountOfBytes(this string s, int byteLength) =>
            byteLength > Encoding.ASCII.GetBytes(s).Length;
    }
}
