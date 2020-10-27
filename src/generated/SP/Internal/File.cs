using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// File class, write your custom code here
    /// </summary>
    [SharePointType("SP.File", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class File : BaseDataModel<IFile>, IFile
    {
        #region Construction
        public File()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        public string CheckInComment { get => GetValue<string>(); set => SetValue(value); }

        public int CheckOutType { get => GetValue<int>(); set => SetValue(value); }

        public string ContentTag { get => GetValue<string>(); set => SetValue(value); }

        public int CustomizedPageStatus { get => GetValue<int>(); set => SetValue(value); }

        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ETag { get => GetValue<string>(); set => SetValue(value); }

        public bool Exists { get => GetValue<bool>(); set => SetValue(value); }

        public bool IrmEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string LinkingUri { get => GetValue<string>(); set => SetValue(value); }

        public string LinkingUrl { get => GetValue<string>(); set => SetValue(value); }

        public int MajorVersion { get => GetValue<int>(); set => SetValue(value); }

        public int MinorVersion { get => GetValue<int>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public int PageRenderType { get => GetValue<int>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public Guid SiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public DateTime TimeCreated { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime TimeLastModified { get => GetValue<DateTime>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public int UIVersion { get => GetValue<int>(); set => SetValue(value); }

        public string UIVersionLabel { get => GetValue<string>(); set => SetValue(value); }

        public Guid UniqueId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        public IUser Author { get => GetModelValue<IUser>(); }


        public IUser CheckedOutByUser { get => GetModelValue<IUser>(); }


        public IEffectiveInformationRightsManagementSettings EffectiveInformationRightsManagementSettings { get => GetModelValue<IEffectiveInformationRightsManagementSettings>(); }


        public IInformationRightsManagementFileSettings InformationRightsManagementSettings { get => GetModelValue<IInformationRightsManagementFileSettings>(); }


        public IListItem ListItemAllFields { get => GetModelValue<IListItem>(); }


        public IUser LockedByUser { get => GetModelValue<IUser>(); }


        public IUser ModifiedBy { get => GetModelValue<IUser>(); }


        public IPropertyValues Properties { get => GetModelValue<IPropertyValues>(); }


        public IFileVersionEventCollection VersionEvents { get => GetModelCollectionValue<IFileVersionEventCollection>(); }


        public IFileVersionCollection Versions { get => GetModelCollectionValue<IFileVersionCollection>(); }


        #endregion

        #region New properties

        public string VroomDriveID { get => GetValue<string>(); set => SetValue(value); }

        public string VroomItemID { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
