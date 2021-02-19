using System.IO;

namespace PnP.Core
{
    internal static class StreamExtensions
    {
        /// <summary>
        /// Create a string from a copy of the stream
        /// Does not affect the state of the original stream
        /// </summary>
        /// <param name="stream">The stream to get a string from</param>
        /// <returns>The value of the stream as string</returns>
        internal static string CopyAsString(this Stream stream)
        {
            long initialPosition = stream.Position;
            using (var ms = new MemoryStream())
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(ms);
                stream.Seek(initialPosition, SeekOrigin.Begin);
                // Set position to start after copying to ensure we can read the string
                ms.Position = 0;
                using (var reader = new StreamReader(ms))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
