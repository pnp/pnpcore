using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents the Navigation
    /// </summary>
    [ConcreteType(typeof(NavigationNodeCollection))]
    public interface INavigationNodeCollection : IQueryable<INavigationNode>, IDataModelCollection<INavigationNode>, ISupportModules<INavigationNodeCollection>
    {
        // Add extension methods here
        #region Get Methods

        /// <summary>
        /// Method to select a specific Navigation Node
        /// </summary>
        /// <param name="id">The Id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The navigation node, if any</returns>
        public INavigationNode GetById(int id, params Expression<Func<INavigationNode, object>>[] selectors);

        /// <summary>
        /// Method to select a specific Navigation Node
        /// </summary>
        /// <param name="id">The Id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The navigation node, if any</returns>
        public Task<INavigationNode> GetByIdAsync(int id, params Expression<Func<INavigationNode, object>>[] selectors);

        /// <summary>
        /// Method to select a all navigation nodes with a specific title
        /// </summary>
        /// <param name="title">The title to search for</param>
        /// <returns>The list of navigation nodes, if any</returns>
        public List<INavigationNode> GetByTitle(string title, params Expression<Func<INavigationNode, object>>[] selectors);

        /// <summary>
        /// Method to select a all navigation nodes with a specific title
        /// </summary>
        /// <param name="title">The title to search for</param>
        /// <returns>The list of navigation nodes, if any</returns>
        public Task<List<INavigationNode>> GetByTitleAsync(string title, params Expression<Func<INavigationNode, object>>[] selectors);

        #endregion

        #region Add Methods
        /// <summary>
        /// Method to add a Navigation Node
        /// </summary>
        /// <param name="navigationNodeOptions">The options for the navigation node</param>
        /// <returns>The navigation node</returns>
        public INavigationNode Add(NavigationNodeOptions navigationNodeOptions);
        
        /// <summary>
        /// Method to add a Navigation Node
        /// </summary>
        /// <param name="navigationNodeOptions">The options for the navigation node</param>
        /// <returns>The navigation node</returns>
        public Task<INavigationNode> AddAsync(NavigationNodeOptions navigationNodeOptions);
        #endregion
    }
}
