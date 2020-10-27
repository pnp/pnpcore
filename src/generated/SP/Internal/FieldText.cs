using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldText class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldText", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldText : BaseDataModel<IFieldText>, IFieldText
    {
        #region Construction
        public FieldText()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int MaxLength { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
