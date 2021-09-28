using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.Core.Tokens
{
    /// <summary>
    /// Resolves the server relative URL of a link targeting the current target site collection
    /// </summary>
    internal class SiteRelativeLinkToken : ITokenDefinition
    {
        public string Name { get => "SiteRelativeLink"; }

        public string GetValue(PnPContext context, string argument)
        {
            return $"{context.Web.ServerRelativeUrl}/{argument}";
        }
    }
}
