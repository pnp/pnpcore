using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a NavigationNode object
    /// </summary>
    [ConcreteType(typeof(NavigationNode))]
    public interface INavigationNode : IDataModel<INavigationNode>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsDocLib { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsExternal { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ListTemplateType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public INavigationNodeCollection Children { get; }

        #endregion

    }
}
