using PnP.Core.Transformation.Services.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a mapping provider
    /// </summary>
    public class MappingProviderInput : BaseMappingProviderInput
    {
        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="context"></param>
        public MappingProviderInput(PageTransformationContext context): base(context)
        {

        }
    }
}
