using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PnP.Core.Utilities
{
    internal static class Base32Encoding
    {
        private const int DecodingValueInvalid = -1;

        private const string EncodingMap = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        private static readonly int[] DecodingMap;

        static Base32Encoding()
        {
            DecodingMap = new int[256];

            for (int i = 0; i < DecodingMap.Length; i++)
            {
                DecodingMap[i] = DecodingValueInvalid;
            }

            for (int i = 0; i < EncodingMap.Length; i++)
            {
                DecodingMap[EncodingMap[i]] = i;
            }
        }

        internal static string ConvertToBase32(byte[] data)
        {
            StringBuilder encodedData = new StringBuilder(data.Length * 2);

            int index = 0;
            int bits = 0;
            int needBits = 5;
            int bitsLeft = 8;
            while (index < data.Length)
            {
                if (bitsLeft == 0)
                {
                    index++;
                    bitsLeft = 8;
                    continue;
                }
                int currentBits = (data[index] << (8 - bitsLeft) & 0xff) >> (8 - bitsLeft);
                if (needBits <= bitsLeft)
                {
                    bits = (bits << needBits) + (currentBits >> (bitsLeft - needBits));
                    bitsLeft = bitsLeft - needBits;

                    encodedData.Append(EncodingMap[bits]);
                    bits = 0;
                    needBits = 5;
                }
                else
                {
                    bits = (bits << bitsLeft) + currentBits;
                    needBits = needBits - bitsLeft;
                    bitsLeft = 0;
                }
            }

            if (needBits > 0 && needBits < 5)
            {
                bits = bits << needBits;
                encodedData.Append(EncodingMap[bits]);
            }

            return encodedData.ToString();
        }

        internal static IList<byte> ConvertFromBase32(string encodedData)
        {
            var bytes = new List<byte>();

            int bits = 0;
            int needBits = 8;
            foreach (var currentChar in encodedData)
            {
                if (currentChar > DecodingMap.Length)
                {
                    throw new InvalidDataException();
                }

                int decodedValue = DecodingMap[currentChar];
                if (decodedValue == DecodingValueInvalid)
                {
                    throw new InvalidDataException();
                }

                if (needBits > 5)
                {
                    bits = (bits << 5) + decodedValue;
                    needBits -= 5;
                }
                else
                {
                    int bitsLeft = 5 - needBits;
                    bits = (bits << needBits) + (decodedValue >> bitsLeft);

                    bytes.Add((byte)bits);

                    bits = ((decodedValue << (5 - bitsLeft)) & 0x1f) >> (5 - bitsLeft);
                    needBits = 8 - (5 - needBits);
                }
            }

            return bytes;
        }
    }
}
