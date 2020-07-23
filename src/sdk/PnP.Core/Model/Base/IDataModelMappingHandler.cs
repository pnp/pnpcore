using PnP.Core.Services;
using System;

namespace PnP.Core.Model
{
    /// <summary>
    /// Interface to the define the basic behavior of Domain Model object
    /// that can be mapped from a JSON response coming out of a REST request
    /// </summary>
    public interface IDataModelMappingHandler
    {
        /// <summary>
        /// Handler that will fire when a property mapping does cannot be done automatically
        /// </summary>
        Func<FromJson, object> MappingHandler { get; set; }

        /// <summary>
        /// Handler that will fire after the full json to model operation was done
        /// </summary>
        Action<string> PostMappingHandler { get; set; }
    }
}
