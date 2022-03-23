using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using Microsoft.Win32.SafeHandles;
using PnP.Core.Auth;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services.Builder.Configuration;
using File = System.IO.File;

namespace PnP.Core.Transformation.Test.Utilities
{
    public static class TestCommon
    {

        /// <summary>
        /// Name of the default test target site configuration
        /// </summary>
        internal static string TargetTestSite => "TargetTestSite";

        public static IConfigurationRoot GetConfigurationSettings()
        {
            // Define the test environment by: 
            // - Copying env.sample to env.txt  
            // - Putting the test environment name in env.txt ==> this should be same name as used in your settings file:
            //   When using appsettings.mine.json then you need to put mine as content in env.txt
            var environmentName = LoadTestEnvironment();

            if (string.IsNullOrEmpty(environmentName))
            {
                throw new Exception("Please ensure you've a env.txt file in the root of the test project. This file should contain the name of the test environment you want to use.");
            }

            // The settings file is stored in the root of the test project, no need to configure the file to be copied over the bin folder
            var jsonSettingsFile = Path.GetFullPath($"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}appsettings.{environmentName}.json");

            var configuration = new ConfigurationBuilder()
            .AddJsonFile(jsonSettingsFile, optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            return configuration;
        }

        public static IServiceProvider AddTestPnPCore(this IServiceCollection services)
        {
            var configuration = GetConfigurationSettings();

            services
                // Configuration
                .AddScoped<IConfiguration>(_ => configuration)
                // Logging service, get config from appsettings + add debug output handler
                .AddLogging(configure =>
                {
                    configure.AddConfiguration(configuration.GetSection("Logging"));
                    configure.AddDebug();
                })
                // Add the PnP Core SDK library services configuration from the appsettings.json file
                .Configure<PnPCoreOptions>(configuration.GetSection("PnPCore"))
                .Configure<PnPCoreAuthenticationOptions>(configuration.GetSection("PnPCore"))
                .AddPnPCoreAuthentication()
                // Add the PnP Core SDK Authentication Providers
                .AddPnPCore();

            // The default configuration has to use credential manager for auth

            var defaultConfiguration = configuration.GetSection("PnPCore:Credentials:DefaultConfiguration")?.Value;

            var clientId = configuration.GetSection($"PnPCore:Credentials:Configurations:{defaultConfiguration}:ClientId")?.Value;
            var tenantId = configuration.GetSection($"PnPCore:Credentials:Configurations:{defaultConfiguration}:TenantId")?.Value;
            var credentialManager = configuration.GetSection($"PnPCore:Credentials:Configurations:{defaultConfiguration}:CredentialManager:CredentialManagerName")?.Value;

            var resource = $"https://{new Uri(configuration["SourceTestSite"]).Authority}";

            var cmap = new CredentialManagerAuthenticationProvider(clientId, tenantId, credentialManager);
            var accessToken = cmap.GetAccessTokenAsync(new Uri(resource)).GetAwaiter().GetResult();

            services.AddTransient(p => {
                var clientContext = new ClientContext(configuration["SourceTestSite"]);
                clientContext.ExecutingWebRequest += (sender, args) =>
                {

                    if (cmap != null)
                    {
                        args.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + accessToken;
                    }
                };
                return clientContext;
            });

            return services.BuildServiceProvider();
        }

        public static IServiceProvider AddTargetTestPnPCore(this IServiceCollection services)
        {
            var configuration = GetConfigurationSettings();

            services
                // Configuration
                .AddScoped<IConfiguration>(_ => configuration)
                // Logging service, get config from appsettings + add debug output handler
                .AddLogging(configure =>
                {
                    configure.AddConfiguration(configuration.GetSection("Logging"));
                    configure.AddDebug();
                })
                // Add the PnP Core SDK library services configuration from the appsettings.json file
                .Configure<PnPCoreOptions>(configuration.GetSection("PnPCore"))
                .Configure<PnPCoreAuthenticationOptions>(configuration.GetSection("PnPCore"))
                .AddPnPCoreAuthentication()
                // Add the PnP Core SDK Authentication Providers
                .AddPnPCore();

            // The default configuration has to use credential manager for auth

            var defaultConfiguration = configuration.GetSection("PnPCore:Credentials:DefaultConfiguration")?.Value;

            var clientId = configuration.GetSection($"PnPCore:Credentials:Configurations:{defaultConfiguration}:ClientId")?.Value;
            var tenantId = configuration.GetSection($"PnPCore:Credentials:Configurations:{defaultConfiguration}:TenantId")?.Value;
            var credentialManager = configuration.GetSection($"PnPCore:Credentials:Configurations:{defaultConfiguration}:CredentialManager:CredentialManagerName")?.Value;

            var resource = $"https://{new Uri(configuration["TargetTestSite"]).Authority}";

            var cmap = new CredentialManagerAuthenticationProvider(clientId, tenantId, credentialManager);
            var accessToken = cmap.GetAccessTokenAsync(new Uri(resource)).GetAwaiter().GetResult();

            services.AddTransient(p => {
                var clientContext = new ClientContext(configuration["TargetTestSite"]);
                clientContext.ExecutingWebRequest += (sender, args) =>
                {

                    if (cmap != null)
                    {
                        args.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + accessToken;
                    }
                };
                return clientContext;
            });

            return services.BuildServiceProvider();
        }

        private static string LoadTestEnvironment()
        {
            // Detect if we're running in a github workflow            
            if (RunningInGitHubWorkflow())
            {
                return "ci";
            }
            else
            {
                string testEnvironmentFile = "..\\..\\..\\env.txt";
                if (File.Exists(testEnvironmentFile))
                {
                    string content = File.ReadAllText(testEnvironmentFile);
                    if (!string.IsNullOrEmpty(content))
                    {
                        return content.Trim();
                    }
                }

                return null;
            }
        }


        internal static bool RunningInGitHubWorkflow()
        {
            var runningInCI = Environment.GetEnvironmentVariable("CI");
            if (!string.IsNullOrEmpty(runningInCI))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal const string PnPCoreSDKTestPrefix = "PNP_SDK_TEST_";
        internal static string GetPnPSdkTestAssetName(string name)
        {
            return name.StartsWith(PnPCoreSDKTestPrefix) ? name : $"{PnPCoreSDKTestPrefix}{name}";
        }




        /*
         *  Temporary copy of the credential read from the auth assembly.
         *  This project needs to be signed inorder to allow this project to 
         *  acccess internal classes.
         *  
         */


        internal static NetworkCredential ReadWindowsCredentialManagerEntry(string applicationName)
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


        // TEMP
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

        /// <summary>
        /// Converts a string to a SecureString
        /// </summary>
        /// <param name="input">String to convert</param>
        /// <returns>SecureString representation of the passed in string</returns>
        internal static SecureString ToSecureString(this string input)
        {
            return new NetworkCredential("", input).SecurePassword;
        }
    }
}
