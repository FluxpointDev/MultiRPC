using System.Text;

namespace MultiRPC.Functions
{
    public static class Checks
    {
        public static bool UnderAmountOfBytes(string s, int ByteLength)
        {
            return ByteLength > Encoding.ASCII.GetBytes(s).Length;
        }
    }
}