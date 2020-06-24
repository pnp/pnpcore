using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a AppContextSite object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class AppContextSite : BaseDataModel<IAppContextSite>, IAppContextSite
    {

        #region New properties

        public string Id4a81de82eeb94d6080ea5bf63e27023a { get => GetValue<string>(); set => SetValue(value); }

        public ISite Site
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new Site
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ISite>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IWeb Web
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new Web
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IWeb>();
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
