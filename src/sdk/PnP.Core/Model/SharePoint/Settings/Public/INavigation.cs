using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents the Navigation
    /// </summary>
    [ConcreteType(typeof(Navigation))]
    public interface INavigation : IDataModel<INavigation>, IDataModelGet<INavigation>, IDataModelLoad<INavigation>, IQueryableDataModel
    {
        /// <summary>
        /// Random property. We need this for the Key property.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// A property that decides whether the navigation is inherited from the site
        /// </summary>
        public bool UseShared { get; }

        /// <summary>
        /// A property that will return the navigation nodes of the Quicklaunch menu of the web
        /// </summary>
        public INavigationNodeCollection QuickLaunch { get; }


        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }
    }
}
