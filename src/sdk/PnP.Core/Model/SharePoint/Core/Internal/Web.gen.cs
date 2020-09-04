using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a Web object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class Web : BaseDataModel<IWeb>, IWeb
    {
        [GraphProperty("sharepointIds", JsonPath = "webId")]
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        [GraphProperty("name")]
        public string Title { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("description")]
        public string Description { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("webUrl")]
        public Uri Url { get => GetValue<Uri>(); set => SetValue(value); }

        // Below attribute was used as sample to test the UseCustomMapping feature
        //[SharePointFieldMapping(FieldName = "NoCrawl", UseCustomMapping = true)]
        public bool NoCrawl { get => GetValue<bool>(); set => SetValue(value); }

        public string RequestAccessEmail { get => GetValue<string>(); set => SetValue(value); }

        public string WelcomePage { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("SiteLogoUrl")]
        public string SiteLogo { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("AlternateCssUrl")]
        public string AlternateCSS { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("MasterUrl")]
        public string MasterPageUrl { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("CustomMasterUrl")]
        public string CustomMasterPageUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool CommentsOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool QuickLaunchEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsMultilingual { get => GetValue<bool>(); set => SetValue(value); }

        public bool OverwriteTranslationsOnChange { get => GetValue<bool>(); set => SetValue(value); }

        public bool ExcludeFromOfflineClient { get => GetValue<bool>(); set => SetValue(value); }

        public bool MembersCanShare { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableFlows { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableAppViews { get => GetValue<bool>(); set => SetValue(value); }

        public bool HorizontalQuickLaunch { get => GetValue<bool>(); set => SetValue(value); }

        // Type mapping needed
        public SearchScope SearchScope { get => GetValue<SearchScope>(); set => SetValue(value); }

        // Type mapping needed
        public SearchBoxInNavBar SearchBoxInNavBar { get => GetValue<SearchBoxInNavBar>(); set => SetValue(value); }

        // Not in Web object, requires extra work to load
        public string SearchCenterUrl { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Fields", Expandable = true)]
        public IFieldCollection Fields
        {
            get
            {
                if (!HasValue(nameof(Fields)))
                {
                    var fields = new FieldCollection(this.PnPContext, this, nameof(Fields));
                    SetValue(fields);
                }
                return GetValue<IFieldCollection>();
            }
        }

        //[SharePointProperty("AvailableFields", Expandable = true)]
        //public IFieldCollection AvailableFields
        //{
        //    get
        //    {
        //        if (!HasValue(nameof(Fields)))
        //        {
        //            var fields = new FieldCollection(this.PnPContext, this, nameof(AvailableFields));
        //            SetValue(fields);
        //        }
        //        return GetValue<IFieldCollection>();
        //    }
        //}

        [SharePointProperty("Lists", Expandable = true)]
        // Graph currently is not returning all lists, this option can only be used once that's fixed
        [GraphProperty("lists", Expandable = true)]
        public IListCollection Lists
        {
            get
            {
                if (!HasValue(nameof(Lists)))
                {
                    var lists = new ListCollection(this.PnPContext, this, nameof(Lists));
                    SetValue(lists);
                }
                return GetValue<IListCollection>();
            }
        }

        [SharePointProperty("ContentTypes", Expandable = true)]
        public IContentTypeCollection ContentTypes
        {
            get
            {
                if (!HasValue(nameof(ContentTypes)))
                {
                    var contentTypes = new ContentTypeCollection(this.PnPContext, this, nameof(ContentTypes));
                    SetValue(contentTypes);
                }
                return GetValue<IContentTypeCollection>();
            }
        }

        //[SharePointProperty("AvailableContentTypes", Expandable = true)]
        //public IContentTypeCollection AvailableContentTypes
        //{
        //    get
        //    {
        //        if (!HasValue(nameof(ContentTypes)))
        //        {
        //            var contentTypes = new ContentTypeCollection(this.PnPContext, this, nameof(AvailableContentTypes));
        //            SetValue(contentTypes);
        //        }
        //        return GetValue<IContentTypeCollection>();
        //    }
        //}

        [SharePointProperty("Webs", Expandable = true)]
        public IWebCollection Webs
        {
            get
            {
                if (!HasValue(nameof(Webs)))
                {
                    var webs = new WebCollection(this.PnPContext, this);
                    SetValue(webs);
                }
                return GetValue<IWebCollection>();
            }
        }

        [SharePointProperty("SiteUserInfoList", Expandable = true)]
        public IList SiteUserInfoList
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

        [SharePointProperty("Features", Expandable = true)]
        public IFeatureCollection Features
        {
            get
            {
                if (!HasValue(nameof(Features)))
                {
                    var features = new FeatureCollection(this.PnPContext, this, nameof(Features));
                    SetValue(features);
                }
                return GetValue<IFeatureCollection>();
            }
        }


        [SharePointProperty("RootFolder", Expandable = true)]
        public IFolder RootFolder
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

        [SharePointProperty("Folders", Expandable = true)]
        public IFolderCollection Folders
        {
            get
            {
                if (!HasValue(nameof(Folders)))
                {
                    var folders = new FolderCollection(this.PnPContext, this, nameof(Folders));
                    SetValue(folders);
                }
                return GetValue<IFolderCollection>();
            }
        }

        [SharePointProperty("AllProperties", Expandable = true)]
        public IPropertyValues AllProperties
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

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
