using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// AlternateUrl class, write your custom code here
    /// </summary>
    [SharePointType("SP.AlternateUrl", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class AlternateUrl : BaseDataModel<IAlternateUrl>, IAlternateUrl
    {
        #region Construction
        public AlternateUrl()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Uri { get => GetValue<string>(); set => SetValue(value); }

        public int UrlZone { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
