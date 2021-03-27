using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// HubSite class, write your custom code here
    /// </summary>
#pragma warning disable CA2243 // Attribute string literals should parse correctly
    [SharePointType("SP.HubSite", Uri = "_api/HubSites/GetById?hubSiteId='{Site.HubSiteId}'", Get = "_api/HubSites/GetById?hubSiteId='{Site.HubSiteId}'")]
#pragma warning restore CA2243 // Attribute string literals should parse correctly
    internal partial class HubSite : BaseDataModel<IHubSite>, IHubSite
    {
        #region Construction
        public HubSite()
        {
            GetApiCallOverrideHandler = async (ApiCallRequest api) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                if (!PnPContext.Site.IsPropertyAvailable(p => p.HubSiteId) || PnPContext.Site.HubSiteId == Guid.Empty)
                {
                    api.CancelRequest("There is no hubsite associated with this site");
                }
                                           
                return api;
            };
        }
        #endregion

        #region Properties
       

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public bool EnablePermissionsSync { get => GetValue<bool>(); set => SetValue(value); }

        public string EnforcedECTs { get => GetValue<string>(); set => SetValue(value); }

        public int EnforcedECTsVersion { get => GetValue<int>(); set => SetValue(value); }

        public bool HideNameInNavigation { get => GetValue<bool>(); set => SetValue(value); }

        [SharePointProperty("ID")]
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string LogoUrl { get => GetValue<string>(); set => SetValue(value); }

        public Guid ParentHubSiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public bool RequiresJoinApproval { get => GetValue<bool>(); set => SetValue(value); }

        public int PermissionsSyncTag { get => GetValue<int>(); set => SetValue(value); }

        public Guid SiteDesignId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid SiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public string SiteUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Targets { get => GetValue<string>(); set => SetValue(value); }

        public Guid TenantInstanceId { get => GetValue<Guid>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (Guid)value; }

        #endregion

    }
}
