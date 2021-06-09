using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// HubSite class, write your custom code here
    /// </summary>
    [SharePointType("SP.HubSite", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class HubSite : BaseDataModel<IHubSite>, IHubSite
    {
        #region Construction
        public HubSite()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public bool EnablePermissionsSync { get => GetValue<bool>(); set => SetValue(value); }

        public string EnforcedECTs { get => GetValue<string>(); set => SetValue(value); }

        public int EnforcedECTsVersion { get => GetValue<int>(); set => SetValue(value); }

        public bool HideNameInNavigation { get => GetValue<bool>(); set => SetValue(value); }

        public Guid ID { get => GetValue<Guid>(); set => SetValue(value); }

        public string LogoUrl { get => GetValue<string>(); set => SetValue(value); }

        public Guid ParentHubSiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public int PermissionsSyncTag { get => GetValue<int>(); set => SetValue(value); }

        public bool RequiresJoinApproval { get => GetValue<bool>(); set => SetValue(value); }

        public Guid SiteDesignId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid SiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public string SiteUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Targets { get => GetValue<string>(); set => SetValue(value); }

        public Guid TenantInstanceId { get => GetValue<Guid>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
