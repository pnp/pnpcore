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
        private const string DriveCompositeIdPrefix = @"b!";
        private const int SizeOfGuid = 16;
        private const char Base64PadCharacter = '=';
        private const string DoubleBase64PadCharacter = "==";
        private const char Base64Character62 = '+';
        private const char Base64Character63 = '/';
        private const char Base64UrlCharacter62 = '-';
        private const char Base64UrlCharacter63 = '_';

        #region DriveItemId

        internal static string EncodeDriveItemId(Guid siteId, Guid webId, Guid docId)
        {
            byte[] hashBytes = ComputeItemIdHash(siteId, webId);

            byte[] encodingBytes = new byte[SizeOfGuid + 4];

            hashBytes.CopyTo(encodingBytes, 0); // 4 bytes
            docId.ToByteArray().CopyTo(encodingBytes, 4);

            return ItemIdPrefix_V1 + Base32Encoding.ConvertToBase32(encodingBytes);
        }

        internal static Guid DecodeDriveItemId(string driveItemId)
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

        internal static string EncodeDriveId(Guid siteId, Guid webId, Guid docLibId)
        {
            byte[] compositeIdBytes = new byte[SizeOfGuid * 3];
            Buffer.BlockCopy(siteId.ToByteArray(), 0, compositeIdBytes, 0, SizeOfGuid);
            Buffer.BlockCopy(webId.ToByteArray(), 0, compositeIdBytes, SizeOfGuid, SizeOfGuid);
            Buffer.BlockCopy(docLibId.ToByteArray(), 0, compositeIdBytes, SizeOfGuid * 2, SizeOfGuid);
            return DriveCompositeIdPrefix + Base64UrlEncode(compositeIdBytes);
        }

        internal static (Guid siteId, Guid webId, Guid docLibId) DecodeDriveId(string driveId)
        {
            byte[] rawCompositeId = FromBase64UrlEncodeString(driveId.Substring(DriveCompositeIdPrefix.Length));

            byte[] scratch = new byte[SizeOfGuid];

            // SiteId
            Buffer.BlockCopy(rawCompositeId, 0, scratch, 0, SizeOfGuid);
            Guid siteId = new Guid(scratch);

            // WebId
            Buffer.BlockCopy(rawCompositeId, SizeOfGuid, scratch, 0, SizeOfGuid);
            Guid webId = new Guid(scratch);

            // DocLibId
            Buffer.BlockCopy(rawCompositeId, SizeOfGuid * 2, scratch, 0, SizeOfGuid);
            Guid docLibId = new Guid(scratch);

            return (siteId, webId, docLibId);
        }

        /// <summary>
        /// Translates a URL to a SharePoint item to id value that can be used with the /shares endpoint (GET /shares/{shareIdOrEncodedSharingUrl})
        /// </summary>
        /// <param name="url">Url to item in SharePoint</param>
        /// <returns>Encoded sharing url</returns>
        internal static string EncodeSharingUrl(string url)
        {
            // See https://docs.microsoft.com/en-us/graph/api/shares-get?view=graph-rest-1.0&tabs=http#encoding-sharing-urls
            string base64Value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url));
            string encodedUrl = "u!" + base64Value.TrimEnd('=').Replace('/', '_').Replace('+', '-');
            return encodedUrl;
        }

        #endregion

        private static string Base64UrlEncode(byte[] value)
        {
            string s = Convert.ToBase64String(value);
            s = s.Split(Base64PadCharacter)[0]; // Remove any trailing padding
            s = s.Replace(Base64Character62, Base64UrlCharacter62);  // 62nd char of encoding
            s = s.Replace(Base64Character63, Base64UrlCharacter63);  // 63rd char of encoding

            return s;
        }

        private static byte[] FromBase64UrlEncodeString(string value)
        {
            string s = value;
            s = s.Replace(Base64UrlCharacter62, Base64Character62); // 62nd char of encoding
            s = s.Replace(Base64UrlCharacter63, Base64Character63); // 63rd char of encoding
            switch (s.Length % 4) // Pad 
            {
                case 0:
                    break; // No pad chars in this case
                case 2:
                    s += DoubleBase64PadCharacter;
                    break; // Two pad chars
                case 3:
                    s += Base64PadCharacter;
                    break; // One pad char
                default:
                    throw new ArgumentException("Invalid string to base64 decode");
            }
            return Convert.FromBase64String(s); // Standard base64 decoder
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
