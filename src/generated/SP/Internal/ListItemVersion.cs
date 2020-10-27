using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ListItemVersion class, write your custom code here
    /// </summary>
    [SharePointType("SP.ListItemVersion", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ListItemVersion : BaseDataModel<IListItemVersion>, IListItemVersion
    {
        #region Construction
        public ListItemVersion()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public DateTime Created { get => GetValue<DateTime>(); set => SetValue(value); }

        public bool IsCurrentVersion { get => GetValue<bool>(); set => SetValue(value); }

        public int VersionId { get => GetValue<int>(); set => SetValue(value); }

        public string VersionLabel { get => GetValue<string>(); set => SetValue(value); }

        public IUser CreatedBy { get => GetModelValue<IUser>(); }


        public IFieldCollection Fields { get => GetModelCollectionValue<IFieldCollection>(); }


        public IFileVersion FileVersion { get => GetModelValue<IFileVersion>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
