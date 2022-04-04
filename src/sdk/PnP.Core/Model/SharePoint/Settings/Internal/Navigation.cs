using System;

namespace PnP.Core.Model.SharePoint
{

    [SharePointType(NavigationConstants.NavigationMetadataType, Target = typeof(Web), Get = baseUri, LinqGet = baseUri)]
    internal sealed class Navigation : BaseDataModel<INavigation>, INavigation
    {
        private const string baseUri = NavigationConstants.NavigationUri;

        #region Properties        

        public Guid Id { get => GetValue<Guid>(); set => Guid.NewGuid(); }

        [SharePointProperty("UseShared")]
        public bool UseShared { get => GetValue<bool>(); set => SetValue(value); }

        public INavigationNodeCollection QuickLaunch { get => GetModelCollectionValue<INavigationNodeCollection>(NavigationType.QuickLaunch.ToString()); }
        
        public INavigationNodeCollection TopNavigationBar { get => GetModelCollectionValue<INavigationNodeCollection>(NavigationType.TopNavigationBar.ToString()); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }

        #endregion
    }
}
