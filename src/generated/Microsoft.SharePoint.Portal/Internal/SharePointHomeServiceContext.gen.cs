using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a SharePointHomeServiceContext object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class SharePointHomeServiceContext : BaseDataModel<ISharePointHomeServiceContext>, ISharePointHomeServiceContext
    {

        #region New properties

        public string CompanyPortalContext { get => GetValue<string>(); set => SetValue(value); }

        public string Payload { get => GetValue<string>(); set => SetValue(value); }

        public ITokenResponse Token
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new TokenResponse
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ITokenResponse>();
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
