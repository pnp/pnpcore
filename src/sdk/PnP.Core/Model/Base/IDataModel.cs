using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the very basic interface for every Domain Model object.
    /// Add methods are implemented in their respective model interfaces
    /// </summary>
    /// <typeparam name="TModel">The actual type of the Domain Model object</typeparam>
    public interface IDataModel<TModel> : IDataModelParent, IDataModelWithContext, IDataModelGet<TModel>
    {
        #region Get

        /// <summary>
        /// Retrieves a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task<TModel> GetAsync(params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        TModel Get(Batch batch, params Expression<Func<TModel, object>>[] expressions);


        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        TModel Get(params Expression<Func<TModel, object>>[] expressions);

        #endregion

        #region Other

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
        /// Was this model requested from the back-end
        /// </summary>
        bool Requested { get; set; }
        #endregion
    }
}
