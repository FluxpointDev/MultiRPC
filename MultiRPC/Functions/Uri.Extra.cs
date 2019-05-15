namespace System.Extra
{
    public static class Uri
    {
        public static string Combine(params string[] strings)
        {
            var uri = "";
            for (var i = 0; i < strings.Length; i++) uri += $"{strings[i]}/";
            return uri.Remove(uri.Length - 1);
        }
    }
}