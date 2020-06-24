using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a FieldText object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class FieldText : BaseDataModel<IFieldText>, IFieldText
    {

        #region New properties

        public int MaxLength { get => GetValue<int>(); set => SetValue(value); }

        #endregion

    }
}
