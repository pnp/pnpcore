using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a Navigation object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class Navigation : BaseDataModel<INavigation>, INavigation
    {

        #region New properties

        public bool UseShared { get => GetValue<bool>(); set => SetValue(value); }

        [SharePointProperty("QuickLaunch", Expandable = true)]
        public INavigationNodeCollection QuickLaunch
        {
            get
            {
                if (!HasValue(nameof(QuickLaunch)))
                {
                    var collection = new NavigationNodeCollection(this.PnPContext, this, nameof(QuickLaunch));
                    SetValue(collection);
                }
                return GetValue<INavigationNodeCollection>();
            }
        }

        [SharePointProperty("TopNavigationBar", Expandable = true)]
        public INavigationNodeCollection TopNavigationBar
        {
            get
            {
                if (!HasValue(nameof(TopNavigationBar)))
                {
                    var collection = new NavigationNodeCollection(this.PnPContext, this, nameof(TopNavigationBar));
                    SetValue(collection);
                }
                return GetValue<INavigationNodeCollection>();
            }
        }

        #endregion

    }
}
