using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Represents the id of a source item
    /// </summary>
    public interface ISourceItemId
    {
        /// <summary>
        /// Gets the string representation of the item
        /// </summary>
        string Id { get; }
    }
}
