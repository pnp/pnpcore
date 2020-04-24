using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the very basic interface for every object that is somehow requestable
    /// through an external querying system
    /// </summary>
    public interface IRequestable
    {
        /// <summary>
        /// Was this object requested, a collection with 0 items and Requested == false was never loaded
        /// </summary>
        /// <returns></returns>
        bool Requested { get; set; }
    }
}
