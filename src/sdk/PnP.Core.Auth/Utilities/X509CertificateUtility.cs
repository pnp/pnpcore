using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PnP.Core.Auth
{
    /// <summary>
    /// Supporting class for certificate based operations
    /// </summary>
    internal static class X509CertificateUtility
    {
        /// <summary>
        /// Loads a certificate from a given certificate store
        /// </summary>
        /// <param name="storeName">Name of the certificate store</param>
        /// <param name="storeLocation">Location of the certificate store</param>
        /// <param name="thumbprint">Thumbprint of the certificate to load</param>
        /// <returns>An <see cref="X509Certificate2"/> certificate</returns>
        internal static X509Certificate2 LoadCertificate(StoreName storeName, StoreLocation storeLocation, string thumbprint)
        {
            // The following code gets the cert from the keystore
            using (X509Store store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certCollection =
                        store.Certificates.Find(X509FindType.FindByThumbprint,
                        thumbprint, false);

                X509Certificate2Enumerator enumerator = certCollection.GetEnumerator();

                X509Certificate2 cert = null;

                while (enumerator.MoveNext())
                {
                    cert = enumerator.Current;
                }

                return cert;
            }
        }

        /// <summary>
        /// Encrypts data based on the RSACryptoServiceProvider
        /// </summary>
        /// <param name="plainData">Bytes to encrypt</param>
        /// <param name="certificate">Certificate to use</param>
        /// <returns>Encrypted bytes</returns>
        internal static byte[] Encrypt(byte[] plainData, X509Certificate2 certificate)
        {
            if (plainData == null)
            {
                throw new ArgumentNullException(nameof(plainData));
            }

            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

#if NET5_0_OR_GREATER
            using (RSA provider = (RSA)certificate.GetRSAPublicKey())
#else
            using (RSA provider = (RSA)certificate.PublicKey.Key)
#endif
            {
                // We use the publickey to encrypt.
                return provider.Encrypt(plainData, RSAEncryptionPadding.OaepSHA1);
            }
        }

        /// <summary>
        /// Decrypts data based on the RSACryptoServiceProvider
        /// </summary>
        /// <param name="encryptedData">Bytes to decrypt</param>
        /// <param name="certificate">Certificate to use</param>
        /// <returns>Decrypted bytes</returns>
        internal static byte[] Decrypt(byte[] encryptedData, X509Certificate2 certificate)
        {
            if (encryptedData == null)
            {
                throw new ArgumentNullException(nameof(encryptedData));
            }

            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

#if NET5_0_OR_GREATER
            using (RSA provider = (RSA)certificate.GetRSAPrivateKey())
#else
            using (RSA provider = (RSA)certificate.PrivateKey)
#endif
            {
                // We use the private key to decrypt.
                return provider.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA1);
            }
        }

    }
}
