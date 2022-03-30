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
        /// 
        /// </summary>
        public bool IsExternal { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool IsDocLib { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsVisible { get; }

        /// <summary>
        /// Title of the navigation node
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int CurrentLCID { get; }
        /// <summary>
        /// 
        /// </summary>
        public ListTemplateType ListTemplateType { get; }

        /// <summary>
        /// 
        /// </summary>
        public List<Guid> AudienceIds { get; }

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
        public Task<List<INavigationNode>> GetChildNodes(params Expression<Func<INavigationNode, object>>[] selectors);
        #endregion
    }
}
