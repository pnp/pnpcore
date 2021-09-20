using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldMultiLineText class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldMultiLineText", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldMultiLineText : BaseDataModel<IFieldMultiLineText>, IFieldMultiLineText
    {
        #region Construction
        public FieldMultiLineText()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool AllowHyperlink { get => GetValue<bool>(); set => SetValue(value); }

        public bool AppendOnly { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsLongHyperlink { get => GetValue<bool>(); set => SetValue(value); }

        public int NumberOfLines { get => GetValue<int>(); set => SetValue(value); }

        public bool RestrictedMode { get => GetValue<bool>(); set => SetValue(value); }

        public bool RichText { get => GetValue<bool>(); set => SetValue(value); }

        public bool UnlimitedLengthInDocumentLibrary { get => GetValue<bool>(); set => SetValue(value); }

        public bool WikiLinking { get => GetValue<bool>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
