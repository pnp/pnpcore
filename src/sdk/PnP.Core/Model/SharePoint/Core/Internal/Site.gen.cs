using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a Site object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class Site : BaseDataModel<ISite>, ISite
    {
        private bool rootWebInstantiated = false;

        [SharePointProperty("Id", IsKey = true)]
        [GraphProperty("sharepointIds", JsonPath = "siteId", IsKey = true)]
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid GroupId { get => GetValue<Guid>(); set => SetValue(value); }

        public Uri Url { get => GetValue<Uri>(); set => SetValue(value); }

        public string Classification { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("RootWeb", Expandable = true)]
        public IWeb RootWeb
        {
            get
            {
                if (!rootWebInstantiated)
                {
                    var rootWeb = new Web
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(rootWeb);
                    rootWebInstantiated = true;
                }
                return GetValue<IWeb>();
            }
            set
            {
                rootWebInstantiated = true;
                SetValue(value);                
            }
        }

        private IWebCollection allWebs;

        public IWebCollection AllWebs
        {
            // PAOLO: Do we need to use TransientObject GetValue/SetValue here?
            // PAOLO: Here, and almost everywhere, we don't have code to guarantee
            // thread safety for instance members, which is the OOB behavior of Microsoft
            // libraries, too ... but just to keep it in mind ...
            get
            {
                if (allWebs == null)
                {
                    allWebs = new WebCollection(this.PnPContext, this);
                }

                return allWebs;
            }
            set
            {
                allWebs = value;
            }
        }

        public bool SocialBarOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public SearchBoxInNavBar SearchBoxInNavBar { get => GetValue<SearchBoxInNavBar>(); set => SetValue(value); }

        /* Not directly a field in the Site object*/
        public string SearchCenterUrl { get => GetValue<string>(); set => SetValue(value); }

        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
