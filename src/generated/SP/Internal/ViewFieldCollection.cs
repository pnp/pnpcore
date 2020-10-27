using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ViewFieldCollection class, write your custom code here
    /// </summary>
    [SharePointType("SP.ViewFieldCollection", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ViewFieldCollection : BaseDataModel<IViewFieldCollection>, IViewFieldCollection
    {
        #region Construction
        public ViewFieldCollection()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        public string SchemaXml { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #region New properties

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
