using Microsoft.Win32.SafeHandles;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace PnP.Core.Auth
{
    internal static class CredentialManager
    {
        internal static bool AddCredential(string name, string username, SecureString password, bool overwrite)
        {
            if (OperatingSystem.IsWindows())
            {
                WriteWindowsCredentialManagerEntry(name, username, password, overwrite);
            }
            else if (OperatingSystem.IsMacOS())
            {
                WriteMacOSKeyChainEntry(name, username, password, overwrite);
            }
            return true;
        }

        internal static bool AddCredential(string name, string username, string password, bool overwrite)
        {
            using (var securePassword = StringToSecureString(password))
            {
                if (OperatingSystem.IsWindows())
                {
                    WriteWindowsCredentialManagerEntry(name, username, securePassword, overwrite);
                }
                else if (OperatingSystem.IsMacOS())
                {
                    WriteMacOSKeyChainEntry(name, username, securePassword, overwrite);
                }
                return true;
            }
        }

        internal static NetworkCredential GetCredential(string name)
        {
            if (OperatingSystem.IsWindows())
            {
                var cred = ReadWindowsCredentialManagerEntry(name);
                return cred;
            }
            if (OperatingSystem.IsMacOS())
            {
                var cred = ReadMacOSKeyChainEntry(name);
                return cred;
            }
            return null;
        }

        private static bool DeleteWindowsCredentialManagerEntry(string applicationName)
        {
            bool success = CredDelete(applicationName, CRED_TYPE.GENERIC, 0);
            return success;
        }

        private static NetworkCredential ReadWindowsCredentialManagerEntry(string applicationName)
        {

            bool success = CredRead(applicationName, CRED_TYPE.GENERIC, 0, out IntPtr credPtr);
            if (success)
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                var critCred = new CriticalCredentialHandle(credPtr);
#pragma warning restore CA2000 // Dispose objects before losing scope
                var cred = critCred.GetCredential();
                var username = cred.UserName;
#pragma warning disable CA2000 // Dispose objects before losing scope
                var securePassword = StringToSecureString(cred.CredentialBlob);
#pragma warning restore CA2000 // Dispose objects before losing scope
                return new NetworkCredential(username, securePassword);
            }
            return null;
        }

        private static SecureString StringToSecureString(string inputString)
        {
            var securityString = new SecureString();
            char[] chars = inputString.ToCharArray();
            foreach (var c in chars)
            {
                securityString.AppendChar(c);
            }
            return securityString;
        }

        private static string SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        private static void WriteWindowsCredentialManagerEntry(string applicationName, string userName, SecureString securePassword, bool overwrite)
        {
            var existingCredential = GetCredential(applicationName);

            bool doWrite = false;

            if (existingCredential != null)
            {
                if (overwrite)
                {
                    DeleteWindowsCredentialManagerEntry(applicationName);
                    doWrite = true;
                }
            }
            else
            {
                doWrite = true;
            }

            if (doWrite)
            {
                var password = SecureStringToString(securePassword);

                byte[] byteArray = password == null ? null : Encoding.Unicode.GetBytes(password);
                if (Environment.OSVersion.Version < new Version(6, 1))
                {
                    if (byteArray != null && byteArray.Length > 512)
                        throw new ArgumentOutOfRangeException("password", "The password has exceeded 512 bytes.");
                }
                else
                {
                    if (byteArray != null && byteArray.Length > 512 * 5)
                        throw new ArgumentOutOfRangeException("password", "The password has exceeded 2560 bytes.");
                }

                NativeCredential credential = new NativeCredential
                {
                    AttributeCount = 0,
                    Attributes = IntPtr.Zero,
                    Comment = IntPtr.Zero,
                    TargetAlias = IntPtr.Zero,
                    Type = CRED_TYPE.GENERIC,
                    Persist = 3,
                    CredentialBlobSize = (uint)(byteArray == null ? 0 : byteArray.Length),
                    TargetName = Marshal.StringToCoTaskMemUni(applicationName),
                    CredentialBlob = Marshal.StringToCoTaskMemUni(password),
                    UserName = Marshal.StringToCoTaskMemUni(userName ?? Environment.UserName)
                };

                bool written = CredWrite(ref credential, 0);
                Marshal.FreeCoTaskMem(credential.TargetName);
                Marshal.FreeCoTaskMem(credential.CredentialBlob);
                Marshal.FreeCoTaskMem(credential.UserName);

                if (!written)
                {
                    int lastError = Marshal.GetLastWin32Error();
                    throw new ClientException($"CredWrite failed with the error code {lastError}");
                }
            }
        }

        private static void WriteMacOSKeyChainEntry(string applicationName, string username, SecureString password, bool overwrite)
        {
            var pw = SecureStringToString(password);
            var cmd = $"/usr/bin/security add-generic-password -a '{username}' -w '{pw}' -s '{applicationName}'";
            if (overwrite)
            {
                cmd += " -U";
            }
            Shell.Bash(cmd);
        }

        private static NetworkCredential ReadMacOSKeyChainEntry(string name)
        {
            var cmd = $"/usr/bin/security find-generic-password -s '{name}'";
            var output = Shell.Bash(cmd);
            string username = null;
            string password = null;
            foreach (var line in output)
            {
                if (line.Trim().StartsWith(@"""acct""", StringComparison.InvariantCultureIgnoreCase))
                {
                    var acctline = line.Trim().Split(new string[] { "<blob>=" }, StringSplitOptions.None);
                    username = acctline[1].Trim(new char[] { '"' });
                }
            }
            cmd = $"/usr/bin/security find-generic-password -s '{name}' -w";
            output = Shell.Bash(cmd);
            if (output.Count == 1)
            {
                password = output[0];
            }
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                return new NetworkCredential(username, StringToSecureString(password));
#pragma warning restore CA2000 // Dispose objects before losing scope
            }
            return null;
        }


        #region UNMANAGED
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct NativeCredential
        {
            public UInt32 Flags;
            public CRED_TYPE Type;
            public IntPtr TargetName;
            public IntPtr Comment;
            public FILETIME LastWritten;
            public UInt32 CredentialBlobSize;
            public IntPtr CredentialBlob;
            public UInt32 Persist;
            public UInt32 AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            public IntPtr UserName;

            internal static NativeCredential GetNativeCredential(Credential cred)
            {
                NativeCredential ncred = new NativeCredential
                {
                    AttributeCount = 0,
                    Attributes = IntPtr.Zero,
                    Comment = IntPtr.Zero,
                    TargetAlias = IntPtr.Zero,
                    Type = CRED_TYPE.GENERIC,
                    Persist = 1,
                    CredentialBlobSize = cred.CredentialBlobSize,
                    TargetName = Marshal.StringToCoTaskMemUni(cred.TargetName),
                    CredentialBlob = Marshal.StringToCoTaskMemUni(cred.CredentialBlob),
                    UserName = Marshal.StringToCoTaskMemUni(Environment.UserName)
                };
                return ncred;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct Credential
        {
            public UInt32 Flags;
            public CRED_TYPE Type;
            public string TargetName;
            public string Comment;
            public FILETIME LastWritten;
            public UInt32 CredentialBlobSize;
            public string CredentialBlob;
            public UInt32 Persist;
            public UInt32 AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
        }

        internal enum CRED_PERSIST : uint
        {
#pragma warning disable CA1712
            CRED_PERSIST_SESSION = 1,
            CRED_PERSIST_LOCAL_MACHINE = 2,

            CRED_PERSIST_ENTERPRISE = 3
#pragma warning restore
        }
        internal enum CRED_TYPE : uint
        {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            GENERIC_CERTIFICATE = 5,
            DOMAIN_EXTENDED = 6,
            MAXIMUM = 7,      // Maximum supported cred type
            MAXIMUM_EX = (MAXIMUM + 1000),  // Allow new applications to run on old OSes
        }

        internal class CriticalCredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid
        {
            public CriticalCredentialHandle(IntPtr preexistingHandle)
            {
                SetHandle(preexistingHandle);
            }

            public Credential GetCredential()
            {
                if (!IsInvalid)
                {
                    NativeCredential ncred = (NativeCredential)Marshal.PtrToStructure(handle,
                          typeof(NativeCredential));
                    Credential cred = new Credential
                    {
                        CredentialBlobSize = ncred.CredentialBlobSize,
                        CredentialBlob = Marshal.PtrToStringUni(ncred.CredentialBlob,
                          (int)ncred.CredentialBlobSize / 2),
                        UserName = Marshal.PtrToStringUni(ncred.UserName),
                        TargetName = Marshal.PtrToStringUni(ncred.TargetName),
                        TargetAlias = Marshal.PtrToStringUni(ncred.TargetAlias),
                        Type = ncred.Type,
                        Flags = ncred.Flags,
                        Persist = ncred.Persist
                    };
                    return cred;
                }
                else
                {
                    throw new InvalidOperationException("Invalid CriticalHandle!");
                }
            }

            override protected bool ReleaseHandle()
            {
                if (!IsInvalid)
                {
                    CredFree(handle);
                    SetHandleAsInvalid();
                    return true;
                }
                return false;
            }
        }



        [DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredWriteW", CharSet = CharSet.Unicode)]
        private static extern bool CredWrite([In] ref NativeCredential userCredential, [In] UInt32 flags);

        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredRead(string target, CRED_TYPE type, int reservedFlag, out IntPtr CredentialPtr);

        [DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
        private static extern bool CredFree([In] IntPtr cred);

        [DllImport("Advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CredDelete(string target, CRED_TYPE type, int reservedFlag);
        #endregion
    }
}
