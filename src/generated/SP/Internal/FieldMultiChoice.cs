using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldMultiChoice class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldMultiChoice", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldMultiChoice : BaseDataModel<IFieldMultiChoice>, IFieldMultiChoice
    {
        #region Construction
        public FieldMultiChoice()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool FillInChoice { get => GetValue<bool>(); set => SetValue(value); }

        public string Mappings { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
