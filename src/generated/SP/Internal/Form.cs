using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Form class, write your custom code here
    /// </summary>
    [SharePointType("SP.Form", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class Form : BaseDataModel<IForm>, IForm
    {
        #region Construction
        public Form()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public int FormType { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }


        #endregion

        #region Extension methods
        #endregion
    }
}
