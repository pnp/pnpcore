using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of View objects
    /// </summary>
    public interface IViewCollection : IQueryable<IView>, IDataModelCollection<IView>
    {
        /// <summary>
        /// Asynchronously adds a new view
        /// </summary>
        /// <returns>View</returns>
        /// <param name="viewOptions">View options</param>
        public Task<IView> AddAsync(ViewOptions viewOptions);

        /// <summary>
        /// Adds a new view
        /// </summary>
        /// <param name="viewOptions">View options</param>
        /// <returns>View</returns>
        public IView Add(ViewOptions viewOptions);

        /// <summary>
        /// Add a view part of a batch asynchroniously
        /// </summary>
        /// <param name="viewOptions">View options</param>
        /// <returns>View</returns>
        public Task<IView> AddBatchAsync(ViewOptions viewOptions);

        /// <summary>
        /// Add a view part of a specific batch asynchroniously
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="viewOptions">View options</param>
        /// <returns>View</returns>
        public Task<IView> AddBatchAsync(Batch batch, ViewOptions viewOptions);

        /// <summary>
        /// Add a view part of a batch
        /// </summary>
        /// <param name="viewOptions">View options</param>
        /// <returns>View</returns>
        public IView AddBatch(ViewOptions viewOptions);

        /// <summary>
        /// Add a view part of a specific batch asynchroniously
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="viewOptions">View options</param>
        /// <returns>View</returns>
        public IView AddBatch(Batch batch, ViewOptions viewOptions);

        /// <summary>
        /// Removes a view
        /// </summary>
        /// <param name="id">ID of the View</param>
        public Task RemoveAsync(Guid id);
    }
}