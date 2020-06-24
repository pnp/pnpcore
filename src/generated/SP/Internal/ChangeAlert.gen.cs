using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ChangeAlert object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ChangeAlert : BaseDataModel<IChangeAlert>, IChangeAlert
    {

        #region New properties

        public Guid AlertId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        #endregion

    }
}
