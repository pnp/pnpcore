using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the basic untyped read interface for Domain Model objects that can be read, only used internally
    /// </summary>
    internal interface IDataModelProcess
    {
        /// <summary>
        /// Method which maps from json to model
        /// </summary>
        /// <param name="apiResponse">Json response (when in recursive mapping of json to model), default otherwise</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task ProcessResponseAsync(ApiResponse apiResponse, params Expression<Func<object, object>>[] expressions);

    }
}
