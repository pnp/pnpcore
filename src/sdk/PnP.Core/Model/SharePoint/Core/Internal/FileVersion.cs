using PnP.Core.Model.Security;
using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FileVersion class, write your custom code here
    /// </summary>
    [SharePointType("SP.FileVersion")]
    internal partial class FileVersion : BaseDataModel<IFileVersion>, IFileVersion
    {
        #region Properties
        public string CheckInComment { get => GetValue<string>(); set => SetValue(value); }

        public DateTime Created { get => GetValue<DateTime>(); set => SetValue(value); }

        [SharePointProperty("ID")]
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool IsCurrentVersion { get => GetValue<bool>(); set => SetValue(value); }

        public int Size { get => GetValue<int>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public string VersionLabel { get => GetValue<string>(); set => SetValue(value); }

        public ISharePointUser CreatedBy { get => GetModelValue<ISharePointUser>(); }

        [KeyProperty(nameof(Created))]
        public override object Key { get => Created; set => Created = DateTime.Parse(value.ToString()); }
        #endregion
    }
}
