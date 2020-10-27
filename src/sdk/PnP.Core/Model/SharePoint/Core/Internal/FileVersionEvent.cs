using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FileVersionEvent class, write your custom code here
    /// </summary>
    [SharePointType("SP.FileVersionEvent")]
    internal partial class FileVersionEvent : BaseDataModel<IFileVersionEvent>, IFileVersionEvent
    {
        #region Properties
        public string Editor { get => GetValue<string>(); set => SetValue(value); }

        public string EditorEmail { get => GetValue<string>(); set => SetValue(value); }

        public DateTime Time { get => GetValue<DateTime>(); set => SetValue(value); }

        [KeyProperty(nameof(Time))]
        public override object Key { get => Time; set => Time = DateTime.Parse(value.ToString()); }
        #endregion
    }
}
