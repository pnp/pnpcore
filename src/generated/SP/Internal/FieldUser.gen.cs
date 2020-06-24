using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a FieldUser object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class FieldUser : BaseDataModel<IFieldUser>, IFieldUser
    {

        #region New properties

        public bool AllowDisplay { get => GetValue<bool>(); set => SetValue(value); }

        public bool Presence { get => GetValue<bool>(); set => SetValue(value); }

        public int SelectionGroup { get => GetValue<int>(); set => SetValue(value); }

        public int SelectionMode { get => GetValue<int>(); set => SetValue(value); }

        #endregion

    }
}
