using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Represents the basic infrastructure of a target tsoken that will be processed by the TokenParser
    /// </summary>
    internal abstract class TargetTokenDefinition
    {
        protected PnPContext TargetContext { get; set; }

        public TargetTokenDefinition(PnPContext targetContext)
        {
            this.TargetContext = targetContext ?? throw new ArgumentNullException(nameof(targetContext));
        }

        /// <summary>
        /// Defines the name of the token
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Method to retrieve the value of specialized token instance
        /// </summary>
        /// <param name="argument">A string input argument useful to resolve the token value</param>
        /// <returns></returns>
        public abstract string GetValue(string argument);
    }
}
