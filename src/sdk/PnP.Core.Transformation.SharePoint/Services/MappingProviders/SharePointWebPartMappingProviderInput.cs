using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint.Client;
using PnP.Core.Transformation.Model;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;

namespace PnP.Core.Transformation.SharePoint.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a Web Part mapping provider
    /// </summary>
    public class SharePointWebPartMappingProviderInput : WebPartMappingProviderInput
    {
        private ClientContext sourceContext;

        /// <summary>
        /// Creates an instance for the specified context and web part
        /// </summary>
        /// <param name="context">The transformation context</param>
        public SharePointWebPartMappingProviderInput(PageTransformationContext context, 
            ClientContext sourceContext) : base(context)
        {
            this.sourceContext = sourceContext;
        }

        /// <summary>
        /// Defines the SharePoint CSOM client context for the source
        /// </summary>
        public ClientContext SourceContext { get { return this.sourceContext; } }
    }
}
