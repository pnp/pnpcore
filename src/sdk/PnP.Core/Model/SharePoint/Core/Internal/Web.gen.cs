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
        public SearchScopes SearchScope { get => GetValue<SearchScopes>(); set => SetValue(value); }

        // Type mapping needed
        public SearchBoxInNavBar SearchBoxInNavBar { get => GetValue<SearchBoxInNavBar>(); set => SetValue(value); }

        // Not in Web object, requires extra work to load
        public string SearchCenterUrl { get => GetValue<string>(); set => SetValue(value); }

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
                    var webs = new ContentTypeCollection(this.PnPContext, this, nameof(ContentTypes));
                    SetValue(webs);
                }
                return GetValue<IContentTypeCollection>();
            }
        }

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

        // Not in Web object, requires extra work to load
        public IAlternateUICultureCollection AlternateUICultures { get; set; } = new AlternateUICultureCollection();

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
