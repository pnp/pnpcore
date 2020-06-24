using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a CheckedOutFile object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class CheckedOutFile : BaseDataModel<ICheckedOutFile>, ICheckedOutFile
    {

        #region New properties

        public int CheckedOutById { get => GetValue<int>(); set => SetValue(value); }

        public IUser CheckedOutBy
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new User
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IUser>();
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
