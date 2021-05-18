using PnP.Core.Transformation.Services.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a mapping provider
    /// </summary>
    public class MappingProviderInput
    {
        /// <summary>
        /// Provides information about the current transformation task
        /// </summary>
        public PageTransformationTask TransformationTask { get; set; }
    }
}
