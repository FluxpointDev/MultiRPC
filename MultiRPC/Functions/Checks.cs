using System.Text;

namespace MultiRPC.Functions
{
    public static class Checks
    {
        public static bool UnderAmountOfBytes(string s, int byteLength)
        {
            return byteLength > Encoding.ASCII.GetBytes(s).Length;
        }
    }
}