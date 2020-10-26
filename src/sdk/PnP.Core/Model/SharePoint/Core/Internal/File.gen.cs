using PnP.Core.Model.Security;
using System;
using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a File object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class File : BaseDataModel<IFile>, IFile
    {
        public string CheckInComment { get => GetValue<string>(); set => SetValue(value); }

        public CheckOutType CheckOutType { get => GetValue<CheckOutType>(); set => SetValue(value); }

        public string ContentTag { get => GetValue<string>(); set => SetValue(value); }

        public CustomizedPageStatus CustomizedPageStatus { get => GetValue<CustomizedPageStatus>(); set => SetValue(value); }

        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ETag { get => GetValue<string>(); set => SetValue(value); }

        public bool Exists { get => GetValue<bool>(); set => SetValue(value); }

        public bool IrmEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string LinkingUri { get => GetValue<string>(); set => SetValue(value); }

        public string LinkingUrl { get => GetValue<string>(); set => SetValue(value); }

        public int MajorVersion { get => GetValue<int>(); set => SetValue(value); }

        public int MinorVersion { get => GetValue<int>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public ListPageRenderType PageRenderType { get => GetValue<ListPageRenderType>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public Guid SiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public DateTime TimeCreated { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime TimeLastModified { get => GetValue<DateTime>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public int UIVersion { get => GetValue<int>(); set => SetValue(value); }

        public string UIVersionLabel { get => GetValue<string>(); set => SetValue(value); }

        public Guid UniqueId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        public IListItem ListItemAllFields { get => GetModelValue<IListItem>(); }

        public IEffectiveInformationRightsManagementSettings EffectiveInformationRightsManagementSettings { get => GetModelValue<IEffectiveInformationRightsManagementSettings>(); }
        
        public IInformationRightsManagementFileSettings InformationRightsManagementSettings { get => GetModelValue<IInformationRightsManagementFileSettings>(); }        

        public IPropertyValues Properties { get => GetModelValue<IPropertyValues>(); }

        public IFileVersionEventCollection VersionEvents { get => GetModelCollectionValue<IFileVersionEventCollection>(); }
       
        public IFileVersionCollection Versions { get => GetModelCollectionValue<IFileVersionCollection>(); }

        public ISharePointUser Author { get => GetModelValue<ISharePointUser>(); }
        
        public ISharePointUser CheckedOutByUser { get => GetModelValue<ISharePointUser>(); }

        public ISharePointUser LockedByUser { get => GetModelValue<ISharePointUser>(); }

        public ISharePointUser ModifiedBy { get => GetModelValue<ISharePointUser>(); }

        [KeyProperty(nameof(UniqueId))]
        public override object Key { get => this.UniqueId; set => this.UniqueId = Guid.Parse(value.ToString()); }
    }
}
