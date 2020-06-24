using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a Audit object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class Audit : BaseDataModel<IAudit>, IAudit
    {

        #region New properties

        public int AuditFlags { get => GetValue<int>(); set => SetValue(value); }

        #endregion

    }
}
