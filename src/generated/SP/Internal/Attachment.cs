using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Attachment class, write your custom code here
    /// </summary>
    [SharePointType("SP.Attachment", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class Attachment : BaseDataModel<IAttachment>, IAttachment
    {
        #region Construction
        public Attachment()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string FileName { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
