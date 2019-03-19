namespace System.Extra
{
    public static class Uri
    {
        public static string Combine(params string[] strings)
        {
            string uri = "";
            foreach (var s in strings)
            {
                uri += $"{s}/";
            }
            return uri.Remove(uri.Length - 1);
        }
    }
}
