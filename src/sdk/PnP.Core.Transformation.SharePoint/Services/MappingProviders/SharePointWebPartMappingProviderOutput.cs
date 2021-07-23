using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint.Client;
using PnP.Core.Transformation.Model;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.MappingFiles;

namespace PnP.Core.Transformation.SharePoint.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a Web Part mapping provider for SharePoint
    /// </summary>
    public class SharePointWebPartMappingProviderOutput : WebPartMappingProviderOutput
    {
        private Mapping mapping;

        /// <summary>
        /// Creates an instance for the specified context and web part
        /// </summary>
        /// <param name="mapping">The web part mapping result</param>
        public SharePointWebPartMappingProviderOutput(Mapping mapping)
        {
            this.mapping = mapping;
        }

        /// <summary>
        /// Provides the mapping information for the mapped web part
        /// </summary>
        public Mapping Mapping { get { return this.mapping; } }
    }
}
