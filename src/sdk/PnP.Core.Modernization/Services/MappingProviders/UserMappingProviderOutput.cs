using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Modernization.Model.Modern;

namespace PnP.Core.Modernization.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a user mapping provider
    /// </summary>
    public class UserMappingProviderOutput : MappingProviderOutput
    {
        /// <summary>
        /// Defines the target UPN from the mapping
        /// </summary>
        public string UserPrincipalName { get; set; }
    }
}
