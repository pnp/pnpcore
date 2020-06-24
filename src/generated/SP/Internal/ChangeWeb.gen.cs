using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ChangeWeb object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ChangeWeb : BaseDataModel<IChangeWeb>, IChangeWeb
    {

        #region New properties

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        #endregion

    }
}
