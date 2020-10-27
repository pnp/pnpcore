using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Navigation object
    /// </summary>
    [ConcreteType(typeof(Navigation))]
    public interface INavigation : IDataModel<INavigation>, IDataModelGet<INavigation>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool UseShared { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public INavigationNodeCollection QuickLaunch { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public INavigationNodeCollection TopNavigationBar { get; }

        #endregion

    }
}
