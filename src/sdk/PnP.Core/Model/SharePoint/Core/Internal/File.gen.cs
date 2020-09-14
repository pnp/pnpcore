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

        public IListItem ListItemAllFields
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new ListItem
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IListItem>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        // TODO: Shouldn't ComplexType enforce expand without the need to mark it here?
        [SharePointProperty("EffectiveInformationRightsManagementSettings", Expandable = true)]
        public IEffectiveInformationRightsManagementSettings EffectiveInformationRightsManagementSettings
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new EffectiveInformationRightsManagementSettings();
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IEffectiveInformationRightsManagementSettings>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        [SharePointProperty("InformationRightsManagementSettings", Expandable = true)]
        public IInformationRightsManagementFileSettings InformationRightsManagementSettings
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new InformationRightsManagementFileSettings();
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IInformationRightsManagementFileSettings>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        // TODO: Shouldn't ComplexType enforce expand without the need to mark it here?
        [SharePointProperty("Properties", Expandable = true)]
        public IPropertyValues Properties
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new PropertyValues();
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IPropertyValues>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        [SharePointProperty("VersionEvents", Expandable = true)]
        public List<IFileVersionEvent> VersionEvents
        {
            get
            {
                if (!HasValue(nameof(VersionEvents)))
                {
                    var collection = new List<IFileVersionEvent>();
                    SetValue(collection);
                }
                return GetValue<List<IFileVersionEvent>>();
            }
        }

        [SharePointProperty("Versions", Expandable = true)]
        public List<IFileVersion> Versions
        {
            get
            {
                if (!HasValue(nameof(Versions)))
                {
                    var collection = new List<IFileVersion>();
                    SetValue(collection);
                }
                return GetValue<List<IFileVersion>>();
            }
        }

        [SharePointProperty("Author", Expandable = true)]
        public ISharePointUser Author
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new SharePointUser
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ISharePointUser>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        [SharePointProperty("CheckedOutByUser", Expandable = true)]
        public ISharePointUser CheckedOutByUser
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new SharePointUser
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ISharePointUser>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        [SharePointProperty("LockedByUser", Expandable = true)]
        public ISharePointUser LockedByUser
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new SharePointUser
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ISharePointUser>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        [SharePointProperty("ModifiedBy", Expandable = true)]
        public ISharePointUser ModifiedBy
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new SharePointUser
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ISharePointUser>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        [KeyProperty("UniqueId")]
        public override object Key { get => this.UniqueId; set => this.UniqueId = Guid.Parse(value.ToString()); }
    }
}
