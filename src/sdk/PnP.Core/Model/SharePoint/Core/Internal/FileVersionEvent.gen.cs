using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a FileVersionEvent object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class FileVersionEvent : BaseDataModel<IFileVersionEvent>, IFileVersionEvent
    {
        public string Editor { get => GetValue<string>(); set => SetValue(value); }

        public string EditorEmail { get => GetValue<string>(); set => SetValue(value); }

        public DateTime Time { get => GetValue<DateTime>(); set => SetValue(value); }
    }
}
