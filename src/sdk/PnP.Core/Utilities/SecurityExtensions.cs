using System;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PnP.Core
{
    /// <summary>
    /// Extensions class that support certificate based encryption/decryption and SecureString protection
    /// </summary>
    internal static class SecurityExtensions
    {

        /// <summary>
        /// Encrypt a piece of text based on a given certificate
        /// </summary>
        /// <param name="stringToEncrypt">Text to encrypt</param>
        /// <param name="thumbPrint">Thumbprint of the certificate to use</param>
        /// <returns>Encrypted text</returns>
        internal static string Encrypt(this string stringToEncrypt, string thumbPrint)
        {
            X509Certificate2 certificate = X509CertificateUtility.LoadCertificate(StoreName.My, StoreLocation.LocalMachine, thumbPrint);

            if (certificate == null)
            {
                return string.Empty;
            }

            byte[] encoded = Encoding.UTF8.GetBytes(stringToEncrypt);
            byte[] encrypted;

            try
            {
                encrypted = X509CertificateUtility.Encrypt(encoded, true, certificate);
            }
            catch (Exception)
            {
                return string.Empty;
            }

            string encryptedString = Convert.ToBase64String(encrypted);
            return encryptedString;
        }

        /// <summary>
        /// Decrypt a piece of text based on a given certificate
        /// </summary>
        /// <param name="stringToDecrypt">Text to decrypt</param>
        /// <param name="thumbPrint">Thumbprint of the certificate to use</param>
        /// <returns>Decrypted text</returns>
        internal static string Decrypt(this string stringToDecrypt, string thumbPrint)
        {
            X509Certificate2 certificate = X509CertificateUtility.LoadCertificate(StoreName.My, StoreLocation.LocalMachine, thumbPrint);

            if (certificate == null)
            {
                return string.Empty;
            }

            byte[] encrypted;
            byte[] decrypted;
            encrypted = Convert.FromBase64String(stringToDecrypt);

            try
            {
                decrypted = X509CertificateUtility.Decrypt(encrypted, true, certificate);
            }
            catch (Exception)
            {
                return string.Empty;
            }

            string decryptedString = Encoding.UTF8.GetString(decrypted);
            return decryptedString;
        }

        /// <summary>
        /// Converts a string to a SecureString
        /// </summary>
        /// <param name="input">String to convert</param>
        /// <returns>SecureString representation of the passed in string</returns>
        internal static SecureString ToSecureString(this string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input string is empty and cannot be made into a SecureString", nameof(input));

            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        /// <summary>
        /// Converts a SecureString to a "regular" string
        /// </summary>
        /// <param name="input">SecureString to convert</param>
        /// <returns>A "regular" string representation of the passed SecureString</returns>
        internal static string ToInsecureString(this SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }
    }
}
