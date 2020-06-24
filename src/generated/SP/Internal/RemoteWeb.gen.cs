using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a RemoteWeb object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class RemoteWeb : BaseDataModel<IRemoteWeb>, IRemoteWeb
    {

        #region New properties

        public bool CanSendEmail { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShareByEmailEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShareByLinkEnabled { get => GetValue<bool>(); set => SetValue(value); }

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
