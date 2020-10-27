using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldComputed class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldComputed", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldComputed : BaseDataModel<IFieldComputed>, IFieldComputed
    {
        #region Construction
        public FieldComputed()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool EnableLookup { get => GetValue<bool>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
