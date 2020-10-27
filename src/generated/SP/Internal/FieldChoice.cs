using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldChoice class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldChoice", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldChoice : BaseDataModel<IFieldChoice>, IFieldChoice
    {
        #region Construction
        public FieldChoice()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int EditFormat { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
