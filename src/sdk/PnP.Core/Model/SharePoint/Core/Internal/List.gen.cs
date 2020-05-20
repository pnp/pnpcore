using System;
using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a List object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>    
    internal partial class List : BaseDataModel<IList>, IList
    {
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        [GraphProperty("displayName")]
        public string Title { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("description")]
        public string Description { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("DocumentTemplateUrl")]
        public string DocumentTemplate { get => GetValue<string>(); set => SetValue(value); }

        public bool OnQuickLaunch { get => GetValue<bool>(); set => SetValue(value); }

        [SharePointProperty("BaseTemplate")]
        public ListTemplateType TemplateType { get => GetValue<ListTemplateType>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public bool EnableVersioning { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableMinorVersions { get => GetValue<bool>(); set => SetValue(value); }

        public int DraftVersionVisibility { get => GetValue<int>(); set => SetValue(value); }

        public bool EnableModeration { get => GetValue<bool>(); set => SetValue(value); }

        [SharePointProperty("MajorWithMinorVersionsLimit")]
        public int MinorVersionLimit { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("MajorVersionLimit")]
        public int MaxVersionLimit { get => GetValue<int>(); set => SetValue(value); }

        [GraphProperty("list", JsonPath = "contentTypesEnabled")]
        public bool ContentTypesEnabled { get => GetValue<bool>(); set => SetValue(value); }

        [GraphProperty("list", JsonPath = "hidden")]
        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public bool ForceCheckout { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableAttachments { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableFolderCreation { get => GetValue<bool>(); set => SetValue(value); }

        public Guid TemplateFeatureId { get => GetValue<Guid>(); set => SetValue(value); }

        public Dictionary<string, string> FieldDefaults { get => GetValue<Dictionary<string, string>>(); set => SetValue(value); }

        public bool NoCrawl { get => GetValue<bool>(); set => SetValue(value); }

        [SharePointProperty("ListExperienceOptions")]
        public ListExperience ListExperience { get => GetValue<ListExperience>(); set => SetValue(value); }

        public string DefaultDisplayFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string DefaultEditFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string DefaultNewFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public ListReadingDirection Direction { get => GetValue<ListReadingDirection>(); set => SetValue(value); }

        public string ImageUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool IrmExpire { get => GetValue<bool>(); set => SetValue(value); }

        public bool IrmReject { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsApplicationList { get => GetValue<bool>(); set => SetValue(value); }

        public int ReadSecurity { get => GetValue<int>(); set => SetValue(value); }

        public int WriteSecurity { get => GetValue<int>(); set => SetValue(value); }

        public string ValidationFormula { get => GetValue<string>(); set => SetValue(value); }

        public string ValidationMessage { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Items", Expandable = true)]
        [GraphProperty("items", Get = "/sites/{Web.GraphId}/lists/{GraphId}/items?expand=fields")]
        public IListItemCollection Items
        {
            get
            {
                if (!HasValue(nameof(Items)))
                {
                    var items = new ListItemCollection(this.PnPContext, this, nameof(Items));
                    SetValue(items);
                }
                return GetValue<IListItemCollection>();
            }
        }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
