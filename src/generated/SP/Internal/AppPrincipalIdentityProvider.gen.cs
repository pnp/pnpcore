using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a AppPrincipalIdentityProvider object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class AppPrincipalIdentityProvider : BaseDataModel<IAppPrincipalIdentityProvider>, IAppPrincipalIdentityProvider
    {

        #region New properties

        public string Id4a81de82eeb94d6080ea5bf63e27023a { get => GetValue<string>(); set => SetValue(value); }

        public IAppPrincipalIdentityProvider External
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new AppPrincipalIdentityProvider
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IAppPrincipalIdentityProvider>();
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
