using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FileVersion class, write your custom code here
    /// </summary>
    [SharePointType("SP.FileVersion", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FileVersion : BaseDataModel<IFileVersion>, IFileVersion
    {
        #region Construction
        public FileVersion()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        public string CheckInComment { get => GetValue<string>(); set => SetValue(value); }

        public DateTime Created { get => GetValue<DateTime>(); set => SetValue(value); }

        [SharePointProperty("ID")]
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool IsCurrentVersion { get => GetValue<bool>(); set => SetValue(value); }

        public int Size { get => GetValue<int>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public string VersionLabel { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #region New properties

        public IUser CreatedBy { get => GetModelValue<IUser>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
