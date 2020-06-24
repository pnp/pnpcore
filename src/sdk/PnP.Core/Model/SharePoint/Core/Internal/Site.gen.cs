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
        [GraphProperty("sharepointIds", JsonPath = "siteId")]
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid GroupId { get => GetValue<Guid>(); set => SetValue(value); }

        public Uri Url { get => GetValue<Uri>(); set => SetValue(value); }

        public string Classification { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("RootWeb", Expandable = true)]
        public IWeb RootWeb
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var rootWeb = new Web
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(rootWeb);
                    InstantiateNavigationProperty();
                }
                return GetValue<IWeb>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }

        private IWebCollection allWebs;

        public IWebCollection AllWebs
        {
            get
            {
                if (allWebs == null)
                {
                    allWebs = new WebCollection(this.PnPContext, this, "AllWebs");
                }

                return allWebs;
            }
            set
            {
                allWebs = value;
            }
        }

        public bool SocialBarOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

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

        public SearchBoxInNavBar SearchBoxInNavBar { get => GetValue<SearchBoxInNavBar>(); set => SetValue(value); }

        /* Not directly a field in the Site object*/
        public string SearchCenterUrl { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
