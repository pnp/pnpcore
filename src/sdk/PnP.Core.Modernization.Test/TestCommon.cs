using Microsoft.SharePoint.Client;
using System;
using System.Configuration;
using System.Net;
using System.Security;

namespace PnP.Core.Modernization.Tests
{
    static class TestCommon
    {
        private static Configuration configuration;

        static TestCommon()
        {
            // Load configuration in a way that's compatible with a .Net Core test project as well
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = @"..\..\..\App.config" //Path to your config file
            };
            configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }
        #region Defaults

        /// <summary>
        /// Common warning that the test is used to perform the process and not yet automated in checks/validation of results.
        /// </summary>
        public static string InconclusiveNoAutomatedChecksMessage { get { return "Does not yet have automated checks, please manually check the results of the test"; } }

        #endregion

        #region methods

        /// <summary>
        /// Returns a connection based on SharePoint version
        /// </summary>
        /// <param name="version"></param>
        /// <param name="isPublishingSite"></param>
        /// <returns></returns>
        public static ClientContext CreateSPPlatformClientContext(SPPlatformVersion version, TransformType transformType)
        {
            
            if (transformType == TransformType.PublishingPage)
            {
                switch (version)
                {
                    case SPPlatformVersion.SP2010:
                        return InternalCreateContext(AppSetting("SPOnPremPublishingSite2010"), SourceContextMode.OnPremises);
                    case SPPlatformVersion.SP2013:
                        return InternalCreateContext(AppSetting("SPOnPremPublishingSite2013"), SourceContextMode.OnPremises);
                    case SPPlatformVersion.SP2016:
                        return InternalCreateContext(AppSetting("SPOnPremPublishingSite2016"), SourceContextMode.OnPremises);
                    case SPPlatformVersion.SP2019:
                        return InternalCreateContext(AppSetting("SPOnPremPublishingSite2019"), SourceContextMode.OnPremises);
                    default:
                    case SPPlatformVersion.SPO:
                        return InternalCreateContext(AppSetting("SPOPublishingSite"), SourceContextMode.SPO);
                }
            }
            else
            {
                switch (version)
                {
                    case SPPlatformVersion.SP2010:
                        return InternalCreateContext(AppSetting("SPOnPremTeamSite2010"), SourceContextMode.OnPremises);
                    case SPPlatformVersion.SP2013:
                        return InternalCreateContext(AppSetting("SPOnPremTeamSite2013"), SourceContextMode.OnPremises);
                    case SPPlatformVersion.SP2016:
                        return InternalCreateContext(AppSetting("SPOnPremTeamSite2016"), SourceContextMode.OnPremises);
                    case SPPlatformVersion.SP2019:
                        return InternalCreateContext(AppSetting("SPOnPremTeamSite2019"), SourceContextMode.OnPremises);
                    default:
                    case SPPlatformVersion.SPO:
                        return InternalCreateContext(AppSetting("SPOTeamSite"), SourceContextMode.SPO);
                }
            }
        }

        public static ClientContext CreateClientContext()
        {
            return InternalCreateContext(AppSetting("SPODevSiteUrl"));
        }

        public static ClientContext CreateClientContext(string url)
        {
            return InternalCreateContext(url, SourceContextMode.SPO);
        }
        
        public static ClientContext CreateOnPremisesClientContext()
        {
            return InternalCreateContext(AppSetting("SPOnPremDevSiteUrl"), SourceContextMode.OnPremises);
        }

        public static ClientContext CreateOnPremisesEnterpriseWikiClientContext()
        {
            return InternalCreateContext(AppSetting("SPOnPremEnterpriseWikiUrl"), SourceContextMode.OnPremises);
        }

        public static ClientContext CreateOnPremisesClientContext(string url)
        {
            return InternalCreateContext(url, SourceContextMode.OnPremises);
        }

        /// <summary>
        /// SharePoint Online Admin Context
        /// </summary>
        /// <returns></returns>
        public static ClientContext CreateTenantClientContext()
        {
            return InternalCreateContext(AppSetting("SPOTenantUrl"), SourceContextMode.SPO);
        }

        private static ClientContext InternalCreateContext(string contextUrl, SourceContextMode sourceContextMode = SourceContextMode.SPO)
        {
            string siteUrl;

            // Read configuration data
            // Trim trailing slashes
            siteUrl = contextUrl.TrimEnd(new[] { '/' });

            if (string.IsNullOrEmpty(siteUrl))
            {
                throw new ConfigurationErrorsException("Site Url in App.config are not set up.");
            }

            ClientContext context = null;

            if (sourceContextMode == SourceContextMode.SPO)
            {
                if (!string.IsNullOrEmpty(AppSetting("SPOCredentialManagerLabel")) && !String.IsNullOrEmpty(AppSetting("AppId")))
                {
                    var tempCred = PnP.Framework.Utilities.CredentialManager.GetCredential(AppSetting("SPOCredentialManagerLabel"));

                    AuthenticationManager am = new AuthenticationManager(AppSetting("AppId"), tempCred.UserName, tempCred.SecurePassword);
                    context = am.GetContext(contextUrl);
                }
                else
                {
                    if (!String.IsNullOrEmpty(AppSetting("SPOUserName")) &&
                        !String.IsNullOrEmpty(AppSetting("SPOPassword")) &&
                        !String.IsNullOrEmpty(AppSetting("AppId")))
                    {
                        AuthenticationManager am = new AuthenticationManager(AppSetting("AppId"), AppSetting("SPOUserName"), GetSecureString(AppSetting("SPOPassword")));
                        context = am.GetContext(contextUrl);
                    }
                    else if (!String.IsNullOrEmpty(AppSetting("AppId")) &&
                             !String.IsNullOrEmpty(AppSetting("AppSecret")))
                    {
                        context = new AuthenticationManager().GetACSAppOnlyContext(contextUrl, AppSetting("AppId"), AppSetting("AppSecret"));
                    }
                    else
                    {
                        throw new ConfigurationErrorsException("Credentials in App.config are not set up.");
                    }
                }
                context.RequestTimeout = 1000 * 60 * 15;
            }


            //if (sourceContextMode == SourceContextMode.OnPremises)
            //{

            //    if (!string.IsNullOrEmpty(AppSetting("SPOnPremCredentialManagerLabel")))
            //    {
            //        var tempCred = PnP.Framework.Utilities.CredentialManager.GetCredential(AppSetting("SPOnPremCredentialManagerLabel"));

            //        // username in format domain\user means we're testing in on-premises
            //        if (tempCred.UserName.IndexOf("\\") > 0)
            //        {
            //            string[] userParts = tempCred.UserName.Split('\\');
            //            context.Credentials = new NetworkCredential(userParts[1], tempCred.SecurePassword, userParts[0]);
            //        }
            //        else
            //        {
            //            throw new ConfigurationErrorsException("Credentials in App.config are not set up for on-premises.");
            //        }
            //    }
            //    else
            //    {
            //        if (!String.IsNullOrEmpty(AppSetting("SPOnPremUserName")) &&
            //            !String.IsNullOrEmpty(AppSetting("SPOnPremPassword")))
            //        {
            //            string[] userParts = AppSetting("SPOnPremUserName").Split('\\');
            //            context.Credentials = new NetworkCredential(userParts[1], GetSecureString(AppSetting("SPOnPremPassword")), userParts[0]);
            //        }
            //        else if (!String.IsNullOrEmpty(AppSetting("SPOnPremAppId")) &&
            //                 !String.IsNullOrEmpty(AppSetting("SPOnPremAppSecret")))
            //        {
            //            AuthenticationManager am = new AuthenticationManager();
            //            context = am.GetAppOnlyAuthenticatedContext(contextUrl, AppSetting("AppId"), AppSetting("AppSecret"));
            //        }
            //        else
            //        {
            //            throw new ConfigurationErrorsException("Tenant credentials in App.config are not set up.");
            //        }
            //    }

            //}

            return context;
        }
        #endregion


        #region Utility

        /// <summary>
        /// Secure Passwords
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static SecureString GetSecureString(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input string is empty and cannot be made into a SecureString", "input");

            var secureString = new SecureString();
            foreach (char c in input.ToCharArray())
                secureString.AppendChar(c);

            return secureString;
        }


        /// <summary>
        /// Get Settings from Config files
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AppSetting(string key)
        {
            try
            {
                return configuration.AppSettings.Settings[key].Value;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        

        /// <summary>
        /// Get SharePoint Version
        /// </summary>
        /// <param name="clientContext"></param>
        /// <returns></returns>
        /// <remarks>
        ///  This is a copy from the Base transform class in transform but without caching.
        /// </remarks>
        public static SPPlatformVersion GetVersion(ClientRuntimeContext clientContext)
        {
            Uri urlUri = new Uri(clientContext.Url);
            
            try
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{urlUri.Scheme}://{urlUri.DnsSafeHost}:{urlUri.Port}/_vti_pvt/service.cnf");
                request.UseDefaultCredentials = true;

                var response = request.GetResponse();

                using (var dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(dataStream))
                    {
                        string version = reader.ReadToEnd().Split('|')[2].Trim();

                        if (Version.TryParse(version, out Version v))
                        {
                            if (v.Major == 14)
                            {
                                return SPPlatformVersion.SP2010;
                            }
                            else if (v.Major == 15)
                            {
                                return SPPlatformVersion.SP2013;
                            }
                            else if (v.Major == 16)
                            {
                                if (v.MinorRevision < 6000)
                                {
                                    return SPPlatformVersion.SP2016;
                                }
                                else if (v.MinorRevision > 10300 && v.MinorRevision < 19000)
                                {
                                    return SPPlatformVersion.SP2019;
                                }
                                else
                                {
                                    return SPPlatformVersion.SPO;
                                }
                            }
                        }
                    }
                }
            }
            catch (WebException)
            {
                // Ignore
            }
            
            return SPPlatformVersion.SPO;
        }

        /// <summary>
        /// Appends SharePoint Version to ASPX pages
        /// </summary>
        /// <param name="version"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string UpdatePageToIncludeVersion(SPPlatformVersion version, string fileName)
        {
            string versionToAppend = string.Empty;
            switch (version)    
            {
                case SPPlatformVersion.SP2010:
                    versionToAppend = "2010";
                    break;
                case SPPlatformVersion.SP2013:
                    versionToAppend = "2013";
                    break;
                case SPPlatformVersion.SP2016:
                    versionToAppend = "2016";
                    break;
                case SPPlatformVersion.SP2019:
                    versionToAppend = "2019";
                    break;
                case SPPlatformVersion.SPO:
                    versionToAppend = "SPO";
                    break;
                default:
                    versionToAppend = "NA";
                    break;
            }

            return fileName.Replace(".aspx", $"-{versionToAppend}.aspx");
        }
    }

    public enum SourceContextMode
    {
        SPO,
        OnPremises
    }

    /// <summary>
    /// SP Version
    /// </summary>
    /// <remarks> Avoid clash with SPVersion enum</remarks>
    public enum SPPlatformVersion
    {
        SP2010,
        SP2013,
        SP2016,
        SP2019,
        SPO
    }

    public enum TransformType
    {
        PublishingPage,
        WebPartPage,
        WikiPage
    }
}
