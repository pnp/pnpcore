using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Auth.Test.Utilities
{
    internal static class TestGlobals
    {
        internal static Uri GraphResource = new Uri("https://graph.microsoft.com");
        internal static string GraphMeRequest = "https://graph.microsoft.com/v1.0/me";

        internal static string ConfigurationBasePath = "PnPCore:Credentials:Configurations";
    }
}
