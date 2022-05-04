using System;
using System.Linq;
using System.Security.Cryptography;

namespace PnP.Core.Utilities
{
    internal static class DriveHelper
    {
        private const int ItemIdLength = 34;
        private const int ItemIdPrefixLength = 2;
        private const string ItemIdPrefix_V1 = "01"; 
        private const int SizeOfGuid = 16;

        internal static string EncodeId(Guid siteId, Guid webId, Guid docId)
        {
            byte[] hashBytes = ComputeItemIdHash(siteId, webId);

            byte[] encodingBytes = new byte[SizeOfGuid + 4];

            hashBytes.CopyTo(encodingBytes, 0); // 4 bytes
            docId.ToByteArray().CopyTo(encodingBytes, 4);

            return ItemIdPrefix_V1 + Base32Encoding.ConvertToBase32(encodingBytes);
        }

        internal static Guid DecodeId(string driveItemId)
        {
            // base32 maps to upper case.
            string idPayload = driveItemId.Substring(ItemIdPrefixLength, ItemIdLength - ItemIdPrefixLength);
            byte[] byteIds = Base32Encoding.ConvertFromBase32(idPayload.ToUpperInvariant()).ToArray();

            byte[] hashBytes = new byte[4];
            Array.Copy(byteIds, 0, hashBytes, 0, 4);
            byte[] guidBytes = new byte[SizeOfGuid];
            Array.Copy(byteIds, 4, guidBytes, 0, SizeOfGuid);

            return new Guid(guidBytes);
        }

        private static byte[] ComputeItemIdHash(Guid siteId, Guid webId)
        {
            byte[] hashBytes = new byte[SizeOfGuid * 2];

            siteId.ToByteArray().CopyTo(hashBytes, 0);
            webId.ToByteArray().CopyTo(hashBytes, SizeOfGuid);
            uint hashKey = ComputeHash(hashBytes);
            return BitConverter.GetBytes(hashKey);
        }

        private static uint ComputeHash(byte[] bytes)
        {
            uint value = 0;
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] data = sha256Hash.ComputeHash(bytes);
                for (int i = 0; i < sizeof(int); i++)
                {
                    value <<= 8;
                    value |= data[i];
                }
            }

            return value;
        }

    }
}
