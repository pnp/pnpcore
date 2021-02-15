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
            using (var securePassword = password.ToSecureString())
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
                var securePassword = cred.CredentialBlob.ToSecureString();
                return new NetworkCredential(username, securePassword);
            }
            return null;
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
                var password = securePassword.ToInsecureString();

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
            var pw = password.ToInsecureString();
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
                return new NetworkCredential(username, password.ToSecureString());
            }
            return null;
        }


        #region UNMANAGED
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct NativeCredential
        {
            public uint Flags;
            public CRED_TYPE Type;
            public IntPtr TargetName;
            public IntPtr Comment;
            public FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            public IntPtr UserName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct Credential
        {
            public uint Flags;
            public CRED_TYPE Type;
            public string TargetName;
            public string Comment;
            public FILETIME LastWritten;
            public uint CredentialBlobSize;
            public string CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
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
        private static extern bool CredWrite([In] ref NativeCredential userCredential, [In] uint flags);

        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredRead(string target, CRED_TYPE type, int reservedFlag, out IntPtr CredentialPtr);

        [DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
        private static extern bool CredFree([In] IntPtr cred);

        [DllImport("Advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CredDelete(string target, CRED_TYPE type, int reservedFlag);
        #endregion
    }
}
