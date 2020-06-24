using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ListItem object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ListItem : BaseDataModel<IListItem>, IListItem
    {

        #region Existing properties

        public bool CommentsDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #region New properties

        public int CommentsDisabledScope { get => GetValue<int>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public int FileSystemObjectType { get => GetValue<int>(); set => SetValue(value); }

        public string IconOverlay { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRedirectedEmbedUri { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRedirectedEmbedUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Client_Title { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("AttachmentFiles", Expandable = true)]
        public IAttachmentCollection AttachmentFiles
        {
            get
            {
                if (!HasValue(nameof(AttachmentFiles)))
                {
                    var collection = new AttachmentCollection(this.PnPContext, this, nameof(AttachmentFiles));
                    SetValue(collection);
                }
                return GetValue<IAttachmentCollection>();
            }
        }

        public IContentType ContentType
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new ContentType
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IContentType>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IDlpPolicyTip GetDlpPolicyTip
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new DlpPolicyTip
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IDlpPolicyTip>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IFieldStringValues FieldValuesAsHtml
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new FieldStringValues
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IFieldStringValues>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IFieldStringValues FieldValuesAsText
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new FieldStringValues
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IFieldStringValues>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IFieldStringValues FieldValuesForEdit
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new FieldStringValues
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IFieldStringValues>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IFile File
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new File
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IFile>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IFolder Folder
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


        public IlikedByInformation LikedByInformation
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new likedByInformation
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IlikedByInformation>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IList ParentList
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new List
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IList>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IPropertyValues Properties
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new PropertyValues
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
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


        [SharePointProperty("Versions", Expandable = true)]
        public IListItemVersionCollection Versions
        {
            get
            {
                if (!HasValue(nameof(Versions)))
                {
                    var collection = new ListItemVersionCollection(this.PnPContext, this, nameof(Versions));
                    SetValue(collection);
                }
                return GetValue<IListItemVersionCollection>();
            }
        }

        #endregion

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = (int)value; }


    }
}
