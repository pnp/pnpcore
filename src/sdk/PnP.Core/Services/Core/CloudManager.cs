using System;

namespace PnP.Core.Services
{
    internal static class CloudManager
    {
        internal static Microsoft365Environment GetEnvironmentFromUri(Uri url)
        {
            var dnsDomain = url.Authority.Substring(url.Authority.LastIndexOf('.') + 1);

            return dnsDomain switch
            {
                "com" => Microsoft365Environment.Production,
                "us" => Microsoft365Environment.USGovernment,
                "de" => Microsoft365Environment.Germany,
                "cn" => Microsoft365Environment.China,
                _ => Microsoft365Environment.Production,
            };
        }

        /// <summary>
        /// Returns the graph authority for the in use environment. See https://docs.microsoft.com/en-us/graph/deployments 
        /// and https://learn.microsoft.com/en-us/microsoft-365/enterprise/urls-and-ip-address-ranges?view=o365-worldwide for details
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        internal static string GetMicrosoftGraphAuthority(Microsoft365Environment environment)
        {
            return environment switch
            {
                Microsoft365Environment.Production => "graph.microsoft.com",
                Microsoft365Environment.PreProduction => "graph.microsoft.com",
                Microsoft365Environment.USGovernment => "graph.microsoft.com",
                Microsoft365Environment.USGovernmentHigh => "graph.microsoft.us",
                Microsoft365Environment.USGovernmentDoD => "dod-graph.microsoft.us",
                Microsoft365Environment.Germany => "graph.microsoft.de",
                Microsoft365Environment.China => "microsoftgraph.chinacloudapi.cn",
                _ => "graph.microsoft.com"
            };
        }

        /// <summary>
        /// Returns the Azure AD login authority. See https://docs.microsoft.com/en-us/graph/deployments 
        /// and https://learn.microsoft.com/en-us/microsoft-365/enterprise/urls-and-ip-address-ranges?view=o365-worldwide for details
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        internal static string GetAzureADLoginAuthority(Microsoft365Environment environment)
        {
            return environment switch
            {
                Microsoft365Environment.Production => "login.microsoftonline.com",
                Microsoft365Environment.PreProduction => "login.windows-ppe.net",
                Microsoft365Environment.USGovernment => "login.microsoftonline.com",
                Microsoft365Environment.USGovernmentHigh => "login.microsoftonline.us",
                Microsoft365Environment.USGovernmentDoD => "login.microsoftonline.us",
                Microsoft365Environment.Germany => "login.microsoftonline.de",
                Microsoft365Environment.China => "login.chinacloudapi.cn",
                _ => "login.microsoftonline.com"
            };
        }


        internal static string GetGraphBaseUrl(PnPContext context) 
        {
            string graphUrl = PnPConstants.MicrosoftGraphBaseUrl;
            if (context.Environment.HasValue)
            {
                if (context.Environment.Value == Microsoft365Environment.Custom)
                {
                    graphUrl = $"https://{context.MicrosoftGraphAuthority}/";
                }
                else
                {
                    graphUrl = $"https://{GetMicrosoftGraphAuthority(context.Environment.Value)}/";
                }
            }

            return graphUrl;
        }

        internal static Uri GetGraphBaseUri(PnPContext context) 
        { 
            return new Uri(GetGraphBaseUrl(context));
        }

    }
}
