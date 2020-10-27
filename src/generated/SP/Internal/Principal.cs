using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Principal class, write your custom code here
    /// </summary>
    [SharePointType("SP.Principal", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class Principal : BaseDataModel<IPrincipal>, IPrincipal
    {
        #region Construction
        public Principal()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool IsHiddenInUI { get => GetValue<bool>(); set => SetValue(value); }

        public string LoginName { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public int PrincipalType { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }


        #endregion

        #region Extension methods
        #endregion
    }
}
