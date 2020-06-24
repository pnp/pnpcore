using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a TenantSettings object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class TenantSettings : BaseDataModel<ITenantSettings>, ITenantSettings
    {

        #region New properties

        public string CorporateCatalogUrl { get => GetValue<string>(); set => SetValue(value); }

        public ITenantSettings Current
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new TenantSettings
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ITenantSettings>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        #endregion

    }
}
