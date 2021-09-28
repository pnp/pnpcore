using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Represents the basic signature of a target token definition that will be processed by the TokenParser
    /// </summary>
    internal interface ITokenDefinition
    {
        /// <summary>
        /// Defines the name of the token
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Method to retrieve the value of specialized token instance
        /// </summary>
        /// <param name="context">The target PnP Context</param>
        /// <param name="argument">A string input argument useful to resolve the token value</param>
        /// <returns></returns>
        string GetValue(PnPContext context, string argument);
    }
}
