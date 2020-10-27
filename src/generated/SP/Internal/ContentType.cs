using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ContentType class, write your custom code here
    /// </summary>
    [SharePointType("SP.ContentType", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ContentType : BaseDataModel<IContentType>, IContentType
    {
        #region Construction
        public ContentType()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        public string ClientFormCustomFormatter { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayFormTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string DocumentTemplate { get => GetValue<string>(); set => SetValue(value); }

        public string DocumentTemplateUrl { get => GetValue<string>(); set => SetValue(value); }

        public string EditFormTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public string EditFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Group { get => GetValue<string>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public string JSLink { get => GetValue<string>(); set => SetValue(value); }

        public string MobileDisplayFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string MobileEditFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string MobileNewFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string NewFormTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public string NewFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ReadOnly { get => GetValue<bool>(); set => SetValue(value); }

        public string SchemaXml { get => GetValue<string>(); set => SetValue(value); }

        public string SchemaXmlWithResourceTokens { get => GetValue<string>(); set => SetValue(value); }

        public string Scope { get => GetValue<string>(); set => SetValue(value); }

        public bool Sealed { get => GetValue<bool>(); set => SetValue(value); }

        public string StringId { get => GetValue<string>(); set => SetValue(value); }

        public IFieldLinkCollection FieldLinks { get => GetModelCollectionValue<IFieldLinkCollection>(); }


        #endregion

        #region New properties

        public IUserResource DescriptionResource { get => GetModelValue<IUserResource>(); }


        public IFieldCollection Fields { get => GetModelCollectionValue<IFieldCollection>(); }


        public IUserResource NameResource { get => GetModelValue<IUserResource>(); }


        public IContentType Parent { get => GetModelValue<IContentType>(); }


        public IWorkflowAssociationCollection WorkflowAssociations { get => GetModelCollectionValue<IWorkflowAssociationCollection>(); }


        #endregion

        [KeyProperty(nameof(StringId))]
        public override object Key { get => StringId; set => StringId = value.ToString(); }


        #endregion

        #region Extension methods
        #endregion
    }
}
