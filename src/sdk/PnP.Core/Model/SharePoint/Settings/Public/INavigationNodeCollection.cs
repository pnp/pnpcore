using PnP.Core.Services;
using System;
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
        /// <summary>
        /// This variable will define whether we will be using the top navigation or the quick launch for our API calls
        /// </summary>
        public NavigationType NavigationType { get; }
        
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

        #region Delete Methods

        /// <summary>
        /// Method to delete all navigation nodes from a specific navigation type
        /// </summary>
        /// <returns></returns>
        public void DeleteAllNodes();

        /// <summary>
        /// Method to delete all navigation nodes from a specific navigation type
        /// </summary>
        /// <returns></returns>
        public Task DeleteAllNodesAsync();

        /// <summary>
        /// Method to delete all navigation nodes from a specific navigation type in a batch
        /// </summary>
        /// <returns></returns>
        public void DeleteAllNodesBatch();

        /// <summary>
        /// Method to delete all navigation nodes from a specific navigation type in a batch
        /// </summary>
        /// <returns></returns>
        public Task DeleteAllNodesBatchAsync();

        /// <summary>
        /// Method to delete all navigation nodes from a specific navigation type in a batch
        /// </summary>
        /// <param name="batch">The batch to add this reques to</param>
        /// <returns></returns>
        public void DeleteAllNodesBatch(Batch batch);

        /// <summary>
        /// Method to delete all navigation nodes from a specific navigation type in a batch
        /// </summary>
        /// <param name="batch">The batch to add this reques to</param>
        /// <returns></returns>
        public Task DeleteAllNodesBatchAsync(Batch batch);
        #endregion

        #region Extension Methods

        /// <summary>
        /// Function to move a node after another navigation node
        /// </summary>
        /// <param name="nodeToMove"></param>
        /// <param name="nodeToMoveAfter"></param>
        /// <returns></returns>
        public void MoveNodeAfter(INavigationNode nodeToMove, INavigationNode nodeToMoveAfter);

        /// <summary>
        /// Function to move a node after another navigation node
        /// </summary>
        /// <param name="nodeToMove"></param>
        /// <param name="nodeToMoveAfter"></param>
        /// <returns></returns>
        public Task MoveNodeAfterAsync(INavigationNode nodeToMove, INavigationNode nodeToMoveAfter);
        #endregion
    }
}
