using PnP.Framework.Http;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Transform
{
    internal static class SystemImageSize
    {
        private const int HeaderLimit = 65536;

        internal static (int Width, int Height) Get(Uri imageUrl)
        {
            // Layout images are static resources, not library files. Do not send site credentials.
            using (var client = PnPHttpClient.Instance.GetHttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, imageUrl))
            {
                request.Headers.Range = new RangeHeaderValue(0, HeaderLimit - 1);
                try
                {
                    using (var response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult())
                    {
                        response.EnsureSuccessStatusCode();
                        using (var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                        {
                            var header = new byte[HeaderLimit];
                            int count = 0;
                            int read;
                            while (count < header.Length && (read = stream.Read(header, count, header.Length - count)) > 0)
                            {
                                count += read;
                            }
                            return Read(header, count);
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    throw new IOException("The system image dimension request timed out.", ex);
                }
            }
        }

        internal static (int Width, int Height) Read(byte[] header, int count)
        {
            if (count >= 10 && header[0] == 'G' && header[1] == 'I' && header[2] == 'F' &&
                header[3] == '8' && (header[4] == '7' || header[4] == '9') && header[5] == 'a')
            {
                return Validate(header[6] | (header[7] << 8), header[8] | (header[9] << 8));
            }
            if (count >= 24 && header[0] == 137 && header[1] == 80 && header[2] == 78 && header[3] == 71 &&
                header[4] == 13 && header[5] == 10 && header[6] == 26 && header[7] == 10 &&
                ReadInt32(header, 8) == 13 &&
                header[12] == 'I' && header[13] == 'H' && header[14] == 'D' && header[15] == 'R')
            {
                return Validate(ReadInt32(header, 16), ReadInt32(header, 20));
            }
            if (count >= 2 && header[0] == 0xff && header[1] == 0xd8)
            {
                int offset = 2;
                while (offset < count && header[offset++] == 0xff)
                {
                    while (offset < count && header[offset] == 0xff)
                    {
                        offset++;
                    }
                    if (offset >= count) break;
                    int marker = header[offset++];
                    if (marker == 0xd9 || marker == 0xda) break;
                    if (marker == 0x01 || (marker >= 0xd0 && marker <= 0xd7)) continue;
                    if (offset + 2 > count) break;
                    int length = (header[offset] << 8) | header[offset + 1];
                    if (length < 2 || length > count - offset) break;
                    if (marker >= 0xc0 && marker <= 0xcf && marker != 0xc4 && marker != 0xc8 && marker != 0xcc)
                    {
                        if (length < 8) break;
                        return Validate((header[offset + 5] << 8) | header[offset + 6],
                            (header[offset + 3] << 8) | header[offset + 4]);
                    }
                    offset += length;
                }
            }
            throw new InvalidDataException("Could not read GIF, PNG, or JPEG system image dimensions from the first 64 KiB. Specify an image width to preserve its size.");
        }

        private static int ReadInt32(byte[] header, int offset)
        {
            return (header[offset] << 24) | (header[offset + 1] << 16) |
                (header[offset + 2] << 8) | header[offset + 3];
        }

        private static (int Width, int Height) Validate(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new InvalidDataException("The system image header contains invalid dimensions.");
            }
            return (width, height);
        }
    }
}
