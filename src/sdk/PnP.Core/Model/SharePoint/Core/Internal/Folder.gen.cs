using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a Folder object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class Folder : BaseDataModel<IFolder>, IFolder
    {
        public bool Exists { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsWOPIEnabled { get => GetValue<bool>(); set => SetValue(value); }

        // NOTE: Is implemented only using SPO Rest for now
        //[GraphProperty("folder.childCount")]
        public int ItemCount { get => GetValue<int>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string ProgID { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public DateTime TimeCreated { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime TimeLastModified { get => GetValue<DateTime>(); set => SetValue(value); }

        public Guid UniqueId { get => GetValue<Guid>(); set => SetValue(value); }

        public string WelcomePage { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Files", Expandable = true)]
        public IFileCollection Files
        {
            get
            {
                if (!HasValue(nameof(Files)))
                {
                    var collection = new FileCollection(this.PnPContext, this, nameof(Files));
                    SetValue(collection);
                }
                return GetValue<IFileCollection>();
            }
        }

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


        public IFolder ParentFolder
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new Folder
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IFolder>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        //TODO: To implement...
        //public IPropertyValues Properties
        //{
        //    get
        //    {
        //        if (!NavigationPropertyInstantiated())
        //        {
        //            var propertyValue = new PropertyValues
        //            {
        //                PnPContext = this.PnPContext,
        //                Parent = this,
        //            };
        //            SetValue(propertyValue);
        //            InstantiateNavigationProperty();
        //        }
        //        return GetValue<IPropertyValues>();
        //    }
        //    set
        //    {
        //        InstantiateNavigationProperty();
        //        SetValue(value);                
        //    }
        //}


        //TODO: To implement...
        //public IStorageMetrics StorageMetrics
        //{
        //    get
        //    {
        //        if (!NavigationPropertyInstantiated())
        //        {
        //            var propertyValue = new StorageMetrics
        //            {
        //                PnPContext = this.PnPContext,
        //                Parent = this,
        //            };
        //            SetValue(propertyValue);
        //            InstantiateNavigationProperty();
        //        }
        //        return GetValue<IStorageMetrics>();
        //    }
        //    set
        //    {
        //        InstantiateNavigationProperty();
        //        SetValue(value);                
        //    }
        //}


        [SharePointProperty("Folders", Expandable = true)]
        public IFolderCollection Folders
        {
            get
            {
                if (!HasValue(nameof(Folders)))
                {
                    var collection = new FolderCollection(this.PnPContext, this, nameof(Folders));
                    SetValue(collection);
                }
                return GetValue<IFolderCollection>();
            }
        }

        [KeyProperty("UniqueId")]
        public override object Key { get => this.UniqueId; set => this.UniqueId = Guid.Parse(value.ToString()); }
    }
}
