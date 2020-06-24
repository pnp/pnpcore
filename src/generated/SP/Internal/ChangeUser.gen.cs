using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ChangeUser object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ChangeUser : BaseDataModel<IChangeUser>, IChangeUser
    {

        #region New properties

        public bool Activate { get => GetValue<bool>(); set => SetValue(value); }

        public int UserId { get => GetValue<int>(); set => SetValue(value); }

        #endregion

    }
}
