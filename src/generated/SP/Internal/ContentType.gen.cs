using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ContentType object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ContentType : BaseDataModel<IContentType>, IContentType
    {

        #region Existing properties

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public string Group { get => GetValue<string>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public string JSLink { get => GetValue<string>(); set => SetValue(value); }

        public string SchemaXml { get => GetValue<string>(); set => SetValue(value); }

        public string Scope { get => GetValue<string>(); set => SetValue(value); }

        public bool Sealed { get => GetValue<bool>(); set => SetValue(value); }

        #endregion

        #region New properties

        public string ClientFormCustomFormatter { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayFormTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string DocumentTemplate { get => GetValue<string>(); set => SetValue(value); }

        public string DocumentTemplateUrl { get => GetValue<string>(); set => SetValue(value); }

        public string EditFormTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public string EditFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string MobileDisplayFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string MobileEditFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string MobileNewFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string NewFormTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public string NewFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ReadOnly { get => GetValue<bool>(); set => SetValue(value); }

        public string SchemaXmlWithResourceTokens { get => GetValue<string>(); set => SetValue(value); }

        public string StringId { get => GetValue<string>(); set => SetValue(value); }

        public IUserResource DescriptionResource
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new UserResource
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IUserResource>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("FieldLinks", Expandable = true)]
        public IFieldLinkCollection FieldLinks
        {
            get
            {
                if (!HasValue(nameof(FieldLinks)))
                {
                    var collection = new FieldLinkCollection(this.PnPContext, this, nameof(FieldLinks));
                    SetValue(collection);
                }
                return GetValue<IFieldLinkCollection>();
            }
        }

        [SharePointProperty("Fields", Expandable = true)]
        public IFieldCollection Fields
        {
            get
            {
                if (!HasValue(nameof(Fields)))
                {
                    var collection = new FieldCollection(this.PnPContext, this, nameof(Fields));
                    SetValue(collection);
                }
                return GetValue<IFieldCollection>();
            }
        }

        public IUserResource NameResource
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new UserResource
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IUserResource>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IContentType Parent
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


        [SharePointProperty("WorkflowAssociations", Expandable = true)]
        public IWorkflowAssociationCollection WorkflowAssociations
        {
            get
            {
                if (!HasValue(nameof(WorkflowAssociations)))
                {
                    var collection = new WorkflowAssociationCollection(this.PnPContext, this, nameof(WorkflowAssociations));
                    SetValue(collection);
                }
                return GetValue<IWorkflowAssociationCollection>();
            }
        }

        #endregion

        [KeyProperty("StringId")]
        public override object Key { get => this.StringId; set => this.StringId = value.ToString(); }


    }
}
