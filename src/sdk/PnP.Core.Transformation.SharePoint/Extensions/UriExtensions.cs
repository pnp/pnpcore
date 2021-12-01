namespace System
{
    /// <summary>
    /// Extensions methods for uri
    /// </summary>
    internal static class UriExtensions
    {
        /// <summary>
        /// Combines a uri with a relative path
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static Uri Combine(this Uri uri, string path)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (path == null) throw new ArgumentNullException(nameof(path));

            return new Uri($"{uri.ToString().TrimEnd('/')}/{path}");
        }
    }
}
