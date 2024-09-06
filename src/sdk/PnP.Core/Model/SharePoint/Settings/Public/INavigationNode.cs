using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents the Navigation
    /// </summary>
    [ConcreteType(typeof(NavigationNode))]
    public interface INavigationNode : IDataModel<INavigationNode>, IDataModelGet<INavigationNode>, IDataModelLoad<INavigationNode>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {
        /// <summary>
        /// The ID of the navigation node
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The link that the navigation node is referring to
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Defines if the navigation node links to an external page or stays within SP
        /// </summary>
        public bool IsExternal { get; }
        
        /// <summary>
        /// Defines if the navigation node refers to a doc lib
        /// </summary>
        public bool IsDocLib { get; }

        /// <summary>
        /// Defines if the navigation node is visible or not
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Title of the navigation node
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The language ID under which the navigation node is created
        /// </summary>
        public int CurrentLCID { get; }

        /// <summary>
        /// The list template type of the navigation node
        /// </summary>
        public ListTemplateType ListTemplateType { get; }

        /// <summary>
        /// Define the up to 1O audiences for this navigation node. Note that Web.NavAudienceTargetingEnabled has to be set to true first.
        /// </summary>
        public List<Guid> AudienceIds { get; set; }

        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }

        #region Methods

        /// <summary>
        /// Method to obtain all the child nodes of a navigation node
        /// </summary>
        /// <param name="selectors"></param>
        /// <returns></returns>
        public Task<List<INavigationNode>> GetChildNodesAsync(params Expression<Func<INavigationNode, object>>[] selectors);
        
        /// <summary>
        /// Method to obtain all the child nodes of a navigation node
        /// </summary>
        /// <param name="selectors"></param>
        /// <returns></returns>
        public List<INavigationNode> GetChildNodes(params Expression<Func<INavigationNode, object>>[] selectors);
        #endregion
    }
}
