using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a FieldMultiLineText object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class FieldMultiLineText : BaseDataModel<IFieldMultiLineText>, IFieldMultiLineText
    {

        #region New properties

        public bool AllowHyperlink { get => GetValue<bool>(); set => SetValue(value); }

        public bool AppendOnly { get => GetValue<bool>(); set => SetValue(value); }

        public int NumberOfLines { get => GetValue<int>(); set => SetValue(value); }

        public bool RestrictedMode { get => GetValue<bool>(); set => SetValue(value); }

        public bool RichText { get => GetValue<bool>(); set => SetValue(value); }

        public bool UnlimitedLengthInDocumentLibrary { get => GetValue<bool>(); set => SetValue(value); }

        public bool WikiLinking { get => GetValue<bool>(); set => SetValue(value); }

        #endregion

    }
}
