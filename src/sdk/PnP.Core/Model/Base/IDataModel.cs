using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the very basic interface for every Domain Model object.
    /// Add methods are implemented in their respective model interfaces
    /// </summary>
    /// <typeparam name="TModel">The actual type of the Domain Model object</typeparam>
    public interface IDataModel<TModel> : IDataModelParent, IDataModelWithContext
    {
        /// <summary>
        /// Defines whether this model object was requested from the back-end
        /// </summary>
        bool Requested { get; set; }

        /// <summary>
        /// Checks if a property on this model object has a value set
        /// </summary>
        /// <param name="propertyName">Property to check</param>
        /// <returns>True if set, false otherwise</returns>
        bool HasValue(string propertyName = "");

        /// <summary>
        /// Checks if a property on this model object has changed
        /// </summary>
        /// <param name="propertyName">Property to check</param>
        /// <returns>True if changed, false otherwise</returns>
        bool HasChanged(string propertyName = "");

        /// <summary>
        /// Checks if a property is loaded or not
        /// </summary>
        /// <param name="expression">Expression listing the property to load</param>
        /// <returns>True if property was loaded, false otherwise</returns>
        bool IsPropertyAvailable(Expression<Func<TModel, object>> expression);

        /// <summary>
        /// Checks if the needed properties were loaded or not
        /// </summary>
        /// <param name="expressions">Expression listing the properties to check</param>
        /// <returns>True if properties were loaded, false otherwise</returns>
        bool ArePropertiesAvailable(params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Checks if the requested properties are loaded for the given model, if not they're loaded right now
        /// </summary>
        /// <param name="expressions">Expressions listing the properties to load</param>
        /// <returns></returns>
        void EnsureProperties(params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Checks if the requested properties are loaded for the given model, if not they're loaded via a GetAsync call
        /// </summary>
        /// <param name="expressions">Expressions listing the properties to load</param>
        /// <returns></returns>
        Task EnsurePropertiesAsync(params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Executes a given API call 
        /// </summary>
        /// <param name="request">API call to execute</param>
        /// <returns>The response of the API call</returns>
        Task<ApiRequestResponse> ExecuteRequestAsync(ApiRequest request);

        /// <summary>
        /// Executes a given API call 
        /// </summary>
        /// <param name="request">API call to execute</param>
        /// <returns>The response of the API call</returns>
        ApiRequestResponse ExecuteRequest(ApiRequest request);

        /// <summary>
        /// Executes a given API call 
        /// </summary>
        /// <param name="request">API call to execute</param>
        /// <returns>The response of the API call</returns>
        Task<IBatchSingleResult<BatchResultValue<string>>> ExecuteRequestBatchAsync(ApiRequest request);

        /// <summary>
        /// Executes a given API call 
        /// </summary>
        /// <param name="request">API call to execute</param>
        /// <returns>The response of the API call</returns>
        IBatchSingleResult<BatchResultValue<string>> ExecuteRequestBatch(ApiRequest request);

        /// <summary>
        /// Executes a given API call 
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="request">API call to execute</param>        
        /// <returns>The response of the API call</returns>
        Task<IBatchSingleResult<BatchResultValue<string>>> ExecuteRequestBatchAsync(Batch batch, ApiRequest request);

        /// <summary>
        /// Executes a given API call 
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="request">API call to execute</param>        
        /// <returns>The response of the API call</returns>
        IBatchSingleResult<BatchResultValue<string>> ExecuteRequestBatch(Batch batch, ApiRequest request);
    }
}
