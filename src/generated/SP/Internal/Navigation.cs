using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Navigation class, write your custom code here
    /// </summary>
    [SharePointType("SP.Navigation", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class Navigation : BaseDataModel<INavigation>, INavigation
    {
        #region Construction
        public Navigation()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool UseShared { get => GetValue<bool>(); set => SetValue(value); }

        public INavigationNodeCollection QuickLaunch { get => GetModelCollectionValue<INavigationNodeCollection>(); }


        public INavigationNodeCollection TopNavigationBar { get => GetModelCollectionValue<INavigationNodeCollection>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
