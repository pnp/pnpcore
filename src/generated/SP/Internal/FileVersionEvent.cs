using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FileVersionEvent class, write your custom code here
    /// </summary>
    [SharePointType("SP.FileVersionEvent", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FileVersionEvent : BaseDataModel<IFileVersionEvent>, IFileVersionEvent
    {
        #region Construction
        public FileVersionEvent()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        public string Editor { get => GetValue<string>(); set => SetValue(value); }

        public string EditorEmail { get => GetValue<string>(); set => SetValue(value); }

        public DateTime Time { get => GetValue<DateTime>(); set => SetValue(value); }

        #endregion

        #region New properties

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
