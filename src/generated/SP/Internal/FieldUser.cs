using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldUser class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldUser", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldUser : BaseDataModel<IFieldUser>, IFieldUser
    {
        #region Construction
        public FieldUser()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool AllowDisplay { get => GetValue<bool>(); set => SetValue(value); }

        public bool Presence { get => GetValue<bool>(); set => SetValue(value); }

        public int SelectionGroup { get => GetValue<int>(); set => SetValue(value); }

        public int SelectionMode { get => GetValue<int>(); set => SetValue(value); }

        public string UserDisplayOptions { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
