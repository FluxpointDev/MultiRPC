using System.Text;

namespace MultiRPC.Extensions
{
    public static class StringExt
    {
        public static bool CheckBytes(this string s, int max) => Encoding.UTF8.GetByteCount(s) <= max;
    }
}