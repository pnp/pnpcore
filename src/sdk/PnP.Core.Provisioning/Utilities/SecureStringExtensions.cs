using System;

namespace PnP.Core.Provisioning.Utilities
{
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Provides extension methods for the type <see cref="SecureString" />.
    /// </summary>
    public static class SecureStringExtensions
    {
        /// <summary>
        /// Determines whether the current <see cref="SecureString"/> is equal to the specified other <see cref="SecureString"/>.
        /// </summary>
        /// <param name="secureString">The secure string.</param>
        /// <param name="otherSecureString">The other secure string.</param>
        /// <returns>
        ///   <c>true</c> if <paramref name="secureString"/> is equal to <paramref name="otherSecureString"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="secureString"/> is <c>null</c>
        /// or
        /// <paramref name="otherSecureString"/> is <c>null</c>
        /// </exception>
        public static bool IsEqualTo(this SecureString secureString, SecureString otherSecureString)
        {
            if (secureString == null)
            {
                throw new ArgumentNullException(nameof(secureString));
            }
            if (otherSecureString == null)
            {
                throw new ArgumentNullException(nameof(otherSecureString));
            }

            if (secureString.Length != otherSecureString.Length)
            {
                return false;
            }

            var ssBstr1Ptr = IntPtr.Zero;
            var ssBstr2Ptr = IntPtr.Zero;

            try
            {
                ssBstr1Ptr = Marshal.SecureStringToBSTR(secureString);
                ssBstr2Ptr = Marshal.SecureStringToBSTR(otherSecureString);

                var str1 = Marshal.PtrToStringBSTR(ssBstr1Ptr);
                var str2 = Marshal.PtrToStringBSTR(ssBstr2Ptr);

                return str1.Equals(str2);
            }
            finally
            {
                if (ssBstr1Ptr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(ssBstr1Ptr);
                }

                if (ssBstr2Ptr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(ssBstr2Ptr);
                }
            }
        }
    }
}
