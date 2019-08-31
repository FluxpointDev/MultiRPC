namespace System.Extra
{
    public static class Uri
    {
        public static string Combine(this string[] strings)
        {
            var uri = "";
            for (var i = 0; i < strings.Length; i++)
            {
                uri += $"{strings[i]}/";
            }

            return uri.Remove(uri.Length - 1);
        }

        public static System.Uri CombineToUri(this string[] strings)
        {
            return new System.Uri(strings.Combine());
        }
    }
}