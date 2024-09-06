using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.Requests.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class FieldCollection : QueryableDataModelCollection<IField>, IFieldCollection
    {
        internal const string FIELD_XML_FORMAT = @"<Field Type=""{0}"" Name=""{1}"" DisplayName=""{2}"" ID=""{3}"" Group=""{4}"" Required=""{5}"" {6}/>";
        internal const string FIELD_XML_FORMAT_WITH_CHILD_NODES = @"<Field Type=""{0}"" Name=""{1}"" DisplayName=""{2}"" ID=""{3}"" Group=""{4}"" Required=""{5}"" {6}>{7}</Field>";
        internal const string FIELD_XML_PARAMETER_FORMAT = @"{0}=""{1}""";
        internal const string FIELD_XML_CHILD_NODE = @"<{0}>{1}</{0}>";

        public FieldCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Text fields
        public async Task<IField> AddTextBatchAsync(string title, FieldTextOptions options = null)
        {
            return await AddTextBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddTextBatch(string title, FieldTextOptions options = null)
        {
            return AddTextBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddTextBatchAsync(Batch batch, string title, FieldTextOptions options = null)
        {
            return await AddFieldBatchAsync(batch, FieldTextOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddTextBatch(Batch batch, string title, FieldTextOptions options = null)
        {
            return AddTextBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddTextAsync(string title, FieldTextOptions options = null)
        {
            return await AddFieldAsync(FieldTextOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddText(string title, FieldTextOptions options = null)
        {
            return AddTextAsync(title, options).GetAwaiter().GetResult();
        }

        private static FieldCreationOptions FieldTextOptionsToCreation(string title, FieldTextOptions options)
        {
            FieldCreationOptions creationOptions = new FieldCreationOptions(FieldType.Text);
            creationOptions.ImportFromCommonFieldOptions(title, options);

            if (options != null && options.MaxLength.HasValue)
            {
                creationOptions.SetAttribute("MaxLength", options.MaxLength.ToString());
            }

            return creationOptions;
        }

        public async Task<IField> AddMultilineTextBatchAsync(string title, FieldMultilineTextOptions options)
        {
            return await AddMultilineTextBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddMultilineTextBatch(string title, FieldMultilineTextOptions options)
        {
            return AddMultilineTextBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddMultilineTextBatchAsync(Batch batch, string title, FieldMultilineTextOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldBatchAsync(batch, FieldMultilineTextOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddMultilineTextBatch(Batch batch, string title, FieldMultilineTextOptions options)
        {
            return AddMultilineTextBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddMultilineTextAsync(string title, FieldMultilineTextOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldAsync(FieldMultilineTextOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddMultilineText(string title, FieldMultilineTextOptions options)
        {
            return AddMultilineTextAsync(title, options).GetAwaiter().GetResult();
        }

        private static FieldCreationOptions FieldMultilineTextOptionsToCreation(string title, FieldMultilineTextOptions options)
        {
            FieldCreationOptions creationOptions = new FieldCreationOptions(FieldType.Note);
            creationOptions.ImportFromCommonFieldOptions(title, options);

            if (options.NumberOfLines.HasValue)
            {
                creationOptions.SetAttribute("NumLines", options.NumberOfLines.ToString());
            }

            if (options.UnlimitedLengthInDocumentLibrary.HasValue)
            {
                creationOptions.SetAttribute("UnlimitedLengthInDocumentLibrary", options.UnlimitedLengthInDocumentLibrary.Value.ToString().ToUpper());
            }

            if (options.RichText.HasValue)
            {
                creationOptions.SetAttribute("RichText", options.RichText.Value.ToString().ToUpper());
            }

            if (options.AppendOnly.HasValue)
            {
                creationOptions.SetAttribute("AppendOnly", options.AppendOnly.Value.ToString().ToUpper());
            }

            if (options.AllowHyperlink.HasValue)
            {
                creationOptions.SetAttribute("AllowHyperlink", options.AllowHyperlink.Value.ToString().ToUpper());
            }

            return creationOptions;
        }
        #endregion

        #region Number fields
        public async Task<IField> AddNumberBatchAsync(string title, FieldNumberOptions options)
        {
            return await AddNumberBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddNumberBatch(string title, FieldNumberOptions options)
        {
            return AddNumberBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddNumberBatchAsync(Batch batch, string title, FieldNumberOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldBatchAsync(batch, FieldNumberOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddNumberBatch(Batch batch, string title, FieldNumberOptions options)
        {
            return AddNumberBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddNumberAsync(string title, FieldNumberOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldAsync(FieldNumberOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddNumber(string title, FieldNumberOptions options)
        {
            return AddNumberAsync(title, options).GetAwaiter().GetResult();
        }

        private static FieldCreationOptions FieldNumberOptionsToCreation(string title, FieldNumberOptions options)
        {
            FieldCreationOptions creationOptions = new FieldCreationOptions(FieldType.Number);
            creationOptions.ImportFromCommonFieldOptions(title, options);

            if (options.DefaultValue.HasValue)
            {
                creationOptions.SetChildXmlNode("DefaultValue", $"<Default>{CsomHelper.XmlString(options.DefaultValue.Value.ToString())}</Default>");
            }

            if (options.Decimals.HasValue)
            {
                creationOptions.SetAttribute("Decimals", options.Decimals.ToString());
            }

            if (options.MinimumValue.HasValue)
            {
                creationOptions.SetAttribute("Min", options.MinimumValue.ToString());
            }

            if (options.MaximumValue.HasValue)
            {
                creationOptions.SetAttribute("Max", options.MaximumValue.ToString());
            }

            if (options.ShowAsPercentage.HasValue)
            {
                creationOptions.SetAttribute("Percentage", options.ShowAsPercentage.ToString().ToUpper());
            }

            return creationOptions;
        }

        #endregion

        #region Boolean fields
        public async Task<IField> AddBooleanBatchAsync(string title, FieldBooleanOptions options)
        {
            return await AddBooleanBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddBooleanBatch(string title, FieldBooleanOptions options)
        {
            return AddBooleanBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddBooleanBatchAsync(Batch batch, string title, FieldBooleanOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldBatchAsync(batch, FieldBooleanOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddBooleanBatch(Batch batch, string title, FieldBooleanOptions options)
        {
            return AddBooleanBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddBooleanAsync(string title, FieldBooleanOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldAsync(FieldBooleanOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddBoolean(string title, FieldBooleanOptions options)
        {
            return AddBooleanAsync(title, options).GetAwaiter().GetResult();
        }

        private static FieldCreationOptions FieldBooleanOptionsToCreation(string title, FieldBooleanOptions options)
        {
            FieldCreationOptions creationOptions = new FieldCreationOptions(FieldType.Boolean);
            creationOptions.ImportFromCommonFieldOptions(title, options);
            return creationOptions;
        }
        #endregion

        #region DateTime fields
        public async Task<IField> AddDateTimeBatchAsync(string title, FieldDateTimeOptions options)
        {
            return await AddDateTimeBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddDateTimeBatch(string title, FieldDateTimeOptions options)
        {
            return AddDateTimeBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddDateTimeBatchAsync(Batch batch, string title, FieldDateTimeOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldBatchAsync(batch, FieldDateTimeOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddDateTimeBatch(Batch batch, string title, FieldDateTimeOptions options)
        {
            return AddDateTimeBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddDateTimeAsync(string title, FieldDateTimeOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldAsync(FieldDateTimeOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddDateTime(string title, FieldDateTimeOptions options)
        {
            return AddDateTimeAsync(title, options).GetAwaiter().GetResult();
        }

        private static FieldCreationOptions FieldDateTimeOptionsToCreation(string title, FieldDateTimeOptions options)
        {
            FieldCreationOptions creationOptions = new FieldCreationOptions(FieldType.DateTime);
            creationOptions.ImportFromCommonFieldOptions(title, options);
            creationOptions.SetAttribute("Format", options.DisplayFormat.ToString());
            creationOptions.SetAttribute("FriendlyDisplayFormat", options.FriendlyDisplayFormat.ToString());
            creationOptions.SetAttribute("CalType", ((int)options.DateTimeCalendarType).ToString());
            return creationOptions;
        }
        #endregion

        #region Currency fields
        public async Task<IField> AddCurrencyBatchAsync(string title, FieldCurrencyOptions options)
        {
            return await AddCurrencyBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddCurrencyBatch(string title, FieldCurrencyOptions options)
        {
            return AddCurrencyBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddCurrencyBatchAsync(Batch batch, string title, FieldCurrencyOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldBatchAsync(batch, FieldCurrencyOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddCurrencyBatch(Batch batch, string title, FieldCurrencyOptions options)
        {
            return AddCurrencyBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddCurrencyAsync(string title, FieldCurrencyOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldAsync(FieldCurrencyOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddCurrency(string title, FieldCurrencyOptions options)
        {
            return AddCurrencyAsync(title, options).GetAwaiter().GetResult();
        }

        private static FieldCreationOptions FieldCurrencyOptionsToCreation(string title, FieldCurrencyOptions options)
        {
            FieldCreationOptions creationOptions = new FieldCreationOptions(FieldType.Currency);
            creationOptions.ImportFromCommonFieldOptions(title, options);
            if (options.Decimals.HasValue)
            {
                creationOptions.SetAttribute("Decimals", options.Decimals.ToString().ToUpper());
            }
            if (options.CurrencyLocaleId.HasValue)
            {
                creationOptions.SetAttribute("LCID", options.CurrencyLocaleId.ToString().ToUpper());
            }
            return creationOptions;
        }
        #endregion

        #region Calculated fields
        public async Task<IField> AddCalculatedBatchAsync(string title, FieldCalculatedOptions options)
        {
            return await AddCalculatedBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddCalculatedBatch(string title, FieldCalculatedOptions options)
        {
            return AddCalculatedBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddCalculatedBatchAsync(Batch batch, string title, FieldCalculatedOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldBatchAsync(batch, FieldCalculatedOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddCalculatedBatch(Batch batch, string title, FieldCalculatedOptions options)
        {
            return AddCalculatedBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddCalculatedAsync(string title, FieldCalculatedOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldAsync(FieldCalculatedOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddCalculated(string title, FieldCalculatedOptions options)
        {
            return AddCalculatedAsync(title, options).GetAwaiter().GetResult();
        }

        private static FieldCreationOptions FieldCalculatedOptionsToCreation(string title, FieldCalculatedOptions options)
        {
            FieldCreationOptions creationOptions = new FieldCreationOptions(FieldType.Calculated);
            creationOptions.ImportFromCommonFieldOptions(title, options);
            creationOptions.SetChildXmlNode("Formula", $"<Formula>{CsomHelper.XmlString(options.Formula)}</Formula>");
            if (options.DateFormat.HasValue)
            {
                creationOptions.SetAttribute("Format", options.DateFormat.ToString());
            }
            creationOptions.SetAttribute("ResultType", options.OutputType.ToString());
            if (options.ShowAsPercentage.HasValue)
            {
                creationOptions.SetAttribute("Percentage", options.ShowAsPercentage.ToString().ToUpper());
            }
            return creationOptions;
        }
        #endregion

        #region Choice fields

        public async Task<IField> AddChoiceBatchAsync(string title, FieldChoiceOptions options)
        {
            return await AddChoiceBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddChoiceBatch(string title, FieldChoiceOptions options)
        {
            return AddChoiceBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddChoiceBatchAsync(Batch batch, string title, FieldChoiceOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldBatchAsync(batch, FieldChoiceOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddChoiceBatch(Batch batch, string title, FieldChoiceOptions options)
        {
            return AddChoiceBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddChoiceAsync(string title, FieldChoiceOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldAsync(FieldChoiceOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddChoice(string title, FieldChoiceOptions options)
        {
            return AddChoiceAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddChoiceMultiBatchAsync(string title, FieldChoiceMultiOptions options)
        {
            return await AddChoiceMultiBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddChoiceMultiBatch(string title, FieldChoiceMultiOptions options)
        {
            return AddChoiceMultiBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddChoiceMultiBatchAsync(Batch batch, string title, FieldChoiceMultiOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldBatchAsync(batch, FieldChoiceMultiOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddChoiceMultiBatch(Batch batch, string title, FieldChoiceMultiOptions options)
        {
            return AddChoiceMultiBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddChoiceMultiAsync(string title, FieldChoiceMultiOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldAsync(FieldChoiceMultiOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddChoiceMulti(string title, FieldChoiceMultiOptions options)
        {
            return AddChoiceMultiAsync(title, options).GetAwaiter().GetResult();
        }

        private static FieldCreationOptions FieldChoiceMultiOptionsToCreation(string title, FieldChoiceMultiOptions options)
        {
            FieldCreationOptions creationOptions = new FieldCreationOptions(FieldType.MultiChoice);
            creationOptions.ImportFromCommonFieldOptions(title, options);
            if (options.FillInChoice.HasValue)
            {
                creationOptions.SetAttribute("FillInChoice", options.FillInChoice.Value.ToString().ToUpper());
            }

            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(options.DefaultChoice))
            {
                sb.AppendLine($"<Default>{CsomHelper.XmlString(options.DefaultChoice)}</Default>");
            }

            sb.AppendLine("<CHOICES>");

            foreach (var choice in options.Choices)
            {
                sb.AppendLine($"<CHOICE>{CsomHelper.XmlString(choice)}</CHOICE>");
            }
            sb.AppendLine("</CHOICES>");

            creationOptions.SetChildXmlNode("ChoiceXml", sb.ToString());

            return creationOptions;
        }

        private static FieldCreationOptions FieldChoiceOptionsToCreation(string title, FieldChoiceOptions options)
        {
            var creationOptions = FieldChoiceMultiOptionsToCreation(title, options);
            creationOptions.FieldType = FieldType.Choice.ToString();
            creationOptions.ImportFromCommonFieldOptions(title, options);
            creationOptions.SetAttribute("Format", options.EditFormat.ToString());

            return creationOptions;
        }
        #endregion

        #region Taxonomy fields

        private async Task WireUpTaxonomyFieldAsync(Field field, TaxonomyFieldCreationOptions options)
        {
            string parentId = (Parent is IList) ? (Parent as IList).Id.ToString() : "";

            Guid siteId = PnPContext.Site.Id;
            Guid webId = PnPContext.Web.Id;
            if (field.Parent != null && field.Parent.Parent is ContentTypeHub hub)
            {
                var contentTypeHubSiteId = await hub.GetSiteIdAsync().ConfigureAwait(false);
                GetSiteAndWebId(contentTypeHubSiteId, out siteId, out webId);
            }

            ProvisionTaxonomyFieldRequest request = new ProvisionTaxonomyFieldRequest(siteId.ToString(),
                webId.ToString(),
                field.Id.ToString(),
                parentId,
                options.TermStoreId,
                options.TermSetId,
                options.Open);

            ApiCall updateCall = new ApiCall(new List<Services.Core.CSOM.Requests.IRequest<object>>() { request })
            {
                Commit = true,
            };

            await field.RawRequestAsync(updateCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private static void GetSiteAndWebId(string graphId, out Guid siteId, out Guid webId)
        {
            string[] split = graphId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            siteId = Guid.Parse(split[1]);
            webId = Guid.Parse(split[2]);
        }

        public async Task<IField> AddTaxonomyAsync(string title, FieldTaxonomyOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Prep generic creation structure
            TaxonomyFieldCreationOptions creationOptions = new TaxonomyFieldCreationOptions
            {
                MultiValue = false,
                TermStoreId = options.TermStoreId,
                TermSetId = options.TermSetId,
                DefaultValue = options.DefaultValue,
                Open = options.OpenTermSet
            };
            creationOptions.ImportFromCommonFieldOptions(title, options);

            // Step 1: create the field
            var newField = await AddFieldAsync(creationOptions).ConfigureAwait(false);

            // Step 2: make it a taxonomy field (depends on CSOM)
            await WireUpTaxonomyFieldAsync(newField as Field, creationOptions).ConfigureAwait(false);

            return newField;
        }

        public IField AddTaxonomy(string title, FieldTaxonomyOptions options)
        {
            return AddTaxonomyAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddTaxonomyMultiAsync(string title, FieldTaxonomyOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Prep generic creation structure
            TaxonomyFieldCreationOptions creationOptions = new TaxonomyFieldCreationOptions
            {
                MultiValue = true,
                TermStoreId = options.TermStoreId,
                TermSetId = options.TermSetId,
                DefaultValues = options.DefaultValues,
                Open = options.OpenTermSet
            };
            creationOptions.ImportFromCommonFieldOptions(title, options);

            // Step 1: create the field
            var newField = await AddFieldAsync(creationOptions).ConfigureAwait(false);

            // Step 2: make it a taxonomy field (depends on CSOM)
            await WireUpTaxonomyFieldAsync(newField as Field, creationOptions).ConfigureAwait(false);

            return newField;
        }

        public IField AddTaxonomyMulti(string title, FieldTaxonomyOptions options)
        {
            return AddTaxonomyMultiAsync(title, options).GetAwaiter().GetResult();
        }
        #endregion

        #region Lookup fields
        public async Task<IField> AddLookupBatchAsync(string title, FieldLookupOptions options)
        {
            return await AddLookupBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddLookupBatch(string title, FieldLookupOptions options)
        {
            return AddLookupBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddLookupBatchAsync(Batch batch, string title, FieldLookupOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldBatchAsync(batch, FieldLookupOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddLookupBatch(Batch batch, string title, FieldLookupOptions options)
        {
            return AddLookupBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddLookupAsync(string title, FieldLookupOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldAsync(FieldLookupOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddLookup(string title, FieldLookupOptions options)
        {
            return AddLookupAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddLookupMultiBatchAsync(string title, FieldLookupOptions options)
        {
            return await AddLookupMultiBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddLookupMultiBatch(string title, FieldLookupOptions options)
        {
            return AddLookupMultiBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddLookupMultiBatchAsync(Batch batch, string title, FieldLookupOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldBatchAsync(batch, FieldLookupMultiOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddLookupMultiBatch(Batch batch, string title, FieldLookupOptions options)
        {
            return AddLookupMultiBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddLookupMultiAsync(string title, FieldLookupOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldAsync(FieldLookupMultiOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddLookupMulti(string title, FieldLookupOptions options)
        {
            return AddLookupMultiAsync(title, options).GetAwaiter().GetResult();
        }

        private static FieldCreationOptions FieldLookupMultiOptionsToCreation(string title, FieldLookupOptions options)
        {
            var creationOptions = FieldLookupOptionsToCreation(title, options);
            creationOptions.FieldType = "LookupMulti";
            creationOptions.SetAttribute("Mult", "TRUE");
            return creationOptions;
        }

        private static FieldCreationOptions FieldLookupOptionsToCreation(string title, FieldLookupOptions options)
        {
            FieldCreationOptions creationOptions = new FieldCreationOptions(FieldType.Lookup);
            creationOptions.ImportFromCommonFieldOptions(title, options);
            creationOptions.SetAttribute("ShowField", options.LookupFieldName);
            creationOptions.SetAttribute("List", options.LookupListId.ToString());
            return creationOptions;
        }
        #endregion

        #region User fields
        public async Task<IField> AddUserBatchAsync(string title, FieldUserOptions options)
        {
            return await AddUserBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddUserBatch(string title, FieldUserOptions options)
        {
            return AddUserBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddUserBatchAsync(Batch batch, string title, FieldUserOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldBatchAsync(batch, FieldUserOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddUserMultiBatch(Batch batch, string title, FieldUserOptions options)
        {
            return AddUserMultiBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddUserMultiBatchAsync(string title, FieldUserOptions options)
        {
            return await AddUserMultiBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddUserMultiBatch(string title, FieldUserOptions options)
        {
            return AddUserMultiBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddUserMultiBatchAsync(Batch batch, string title, FieldUserOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldBatchAsync(FieldUserMultiOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddUserBatch(Batch batch, string title, FieldUserOptions options)
        {
            return AddUserBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddUserAsync(string title, FieldUserOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldAsync(FieldUserOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddUser(string title, FieldUserOptions options)
        {
            return AddUserAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddUserMultiAsync(string title, FieldUserOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldAsync(FieldUserMultiOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddUserMulti(string title, FieldUserOptions options)
        {
            return AddUserMultiAsync(title, options).GetAwaiter().GetResult();
        }

        private static FieldCreationOptions FieldUserMultiOptionsToCreation(string title, FieldUserOptions options)
        {
            var creationOptions = FieldUserOptionsToCreation(title, options);
            creationOptions.FieldType = "UserMulti";
            creationOptions.SetAttribute("Mult", "TRUE");
            return creationOptions;
        }

        private static FieldCreationOptions FieldUserOptionsToCreation(string title, FieldUserOptions options)
        {
            FieldCreationOptions creationOptions = new FieldCreationOptions(FieldType.User);
            creationOptions.ImportFromCommonFieldOptions(title, options);
            creationOptions.SetAttribute("UserSelectionMode", ((int)options.SelectionMode).ToString());
            creationOptions.SetAttribute("UserSelectionScope", "0");
            //creationOptions.SetAttribute("IsModern", "TRUE");
            //creationOptions.SetAttribute("Dropdown", "TRUE");
            return creationOptions;
        }
        #endregion

        #region Url fields
        public async Task<IField> AddUrlBatchAsync(string title, FieldUrlOptions options)
        {
            return await AddUrlBatchAsync(PnPContext.CurrentBatch, title, options).ConfigureAwait(false);
        }

        public IField AddUrlBatch(string title, FieldUrlOptions options)
        {
            return AddUrlBatchAsync(title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddUrlBatchAsync(Batch batch, string title, FieldUrlOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldBatchAsync(batch, FieldUrlOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddUrlBatch(Batch batch, string title, FieldUrlOptions options)
        {
            return AddUrlBatchAsync(batch, title, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddUrlAsync(string title, FieldUrlOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await AddFieldAsync(FieldUrlOptionsToCreation(title, options)).ConfigureAwait(false);
        }

        public IField AddUrl(string title, FieldUrlOptions options)
        {
            return AddUrlAsync(title, options).GetAwaiter().GetResult();
        }

        private static FieldCreationOptions FieldUrlOptionsToCreation(string title, FieldUrlOptions options)
        {
            FieldCreationOptions creationOptions = new FieldCreationOptions(FieldType.URL);
            creationOptions.ImportFromCommonFieldOptions(title, options);
            creationOptions.SetAttribute("Format", options.DisplayFormat.ToString());
            return creationOptions;
        }
        #endregion

        #region Add field as xml
        public async Task<IField> AddFieldAsXmlBatchAsync(string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue)
        {
            return await AddFieldAsXmlBatchAsync(PnPContext.CurrentBatch, schemaXml, addToDefaultView, options).ConfigureAwait(false);
        }

        public IField AddFieldAsXmlBatch(string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue)
        {
            return AddFieldAsXmlBatchAsync(schemaXml, addToDefaultView, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddFieldAsXmlBatchAsync(Batch batch, string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue)
        {
            if (addToDefaultView)
            {
                options |= AddFieldOptionsFlags.AddFieldToDefaultView;
            }

            var newField = CreateNewAndAdd() as Field;
            await newField.AddAsXmlBatchAsync(batch, schemaXml, options).ConfigureAwait(false);
            return newField;
        }

        public IField AddFieldAsXmlBatch(Batch batch, string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue)
        {
            return AddFieldAsXmlBatchAsync(batch, schemaXml, addToDefaultView, options).GetAwaiter().GetResult();
        }

        public async Task<IField> AddFieldAsXmlAsync(string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue)
        {
            // Ensure the AddFieldToDefaultView is in our set of field add flags
            if (addToDefaultView)
            {
                options |= AddFieldOptionsFlags.AddFieldToDefaultView;
            }

            var newField = CreateNewAndAdd() as Field;
            await newField.AddAsXmlAsync(schemaXml, options).ConfigureAwait(false);
            return newField;
        }

        public IField AddFieldAsXml(string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue)
        {
            return AddFieldAsXmlAsync(schemaXml, addToDefaultView, options).GetAwaiter().GetResult();
        }
        #endregion

        #region Generic Field creation 
        public IField AddFieldBatch(FieldCreationOptions fieldCreationOptions)
        {
            return AddFieldBatchAsync(fieldCreationOptions).GetAwaiter().GetResult();
        }

        public async Task<IField> AddFieldBatchAsync(FieldCreationOptions fieldCreationOptions)
        {
            return await AddFieldBatchAsync(PnPContext.CurrentBatch, fieldCreationOptions).ConfigureAwait(false);
        }

        public IField AddFieldBatch(Batch batch, FieldCreationOptions fieldCreationOptions)
        {
            return AddFieldBatchAsync(batch, fieldCreationOptions).GetAwaiter().GetResult();
        }

        public async Task<IField> AddFieldBatchAsync(Batch batch, FieldCreationOptions fieldCreationOptions)
        {
            if (fieldCreationOptions == null)
            {
                throw new ArgumentNullException(nameof(fieldCreationOptions));
            }

            // Translate into field creation caml
            var newFieldCAML = FormatFieldXml(fieldCreationOptions);

            // Create the field using the XML approach
            return await AddFieldAsXmlBatchAsync(batch, newFieldCAML, fieldCreationOptions.AddToDefaultView, fieldCreationOptions.Options).ConfigureAwait(false);
        }

        public IField AddField(FieldCreationOptions fieldCreationOptions)
        {
            if (fieldCreationOptions == null)
            {
                throw new ArgumentNullException(nameof(fieldCreationOptions));
            }

            return AddFieldAsync(fieldCreationOptions).GetAwaiter().GetResult();
        }

        public async Task<IField> AddFieldAsync(FieldCreationOptions fieldCreationOptions)
        {
            if (fieldCreationOptions == null)
            {
                throw new ArgumentNullException(nameof(fieldCreationOptions));
            }

            // Translate into field creation caml
            var newFieldCAML = FormatFieldXml(fieldCreationOptions);

            // Create the field using the XML approach
            return await AddFieldAsXmlAsync(newFieldCAML, fieldCreationOptions.AddToDefaultView, fieldCreationOptions.Options).ConfigureAwait(false);
        }

        /// <summary>
        /// Formats a FieldCreationOptions object into Field CAML xml.
        /// </summary>
        /// <param name="fieldCreationOptions">Field Creation Information object</param>
        /// <returns>Field creation CAML</returns>
        private static string FormatFieldXml(FieldCreationOptions fieldCreationOptions)
        {
            List<string> additionalAttributesList = new List<string>();

            if (fieldCreationOptions.AdditionalAttributes != null)
            {
                foreach (var keyvaluepair in fieldCreationOptions.AdditionalAttributes)
                {
                    additionalAttributesList.Add(string.Format(FIELD_XML_PARAMETER_FORMAT, keyvaluepair.Key, CsomHelper.XmlString(keyvaluepair.Value)));
                }
            }

            List<string> additionalChildNodesList = new List<string>();

            if (fieldCreationOptions.AdditionalChildNodes != null)
            {
                foreach (var keyvaluepair in fieldCreationOptions.AdditionalChildNodes)
                {
                    if (keyvaluepair.Key.StartsWith("Xml:"))
                    {
                        additionalChildNodesList.Add(keyvaluepair.Value);
                    }
                    else
                    {
                        additionalChildNodesList.Add(string.Format(FIELD_XML_CHILD_NODE, keyvaluepair.Key, CsomHelper.XmlString(keyvaluepair.Value)));
                    }
                }
            }

            if (!additionalAttributesList.Contains("ClientSideComponentId"))
            {
                if (fieldCreationOptions.ClientSideComponentId != Guid.Empty)
                {
                    additionalAttributesList.Add(string.Format(FIELD_XML_PARAMETER_FORMAT, "ClientSideComponentId", fieldCreationOptions.ClientSideComponentId.ToString("D")));
                }
            }
            if (!additionalAttributesList.Contains("ClientSideComponentProperties"))
            {
                if (fieldCreationOptions.ClientSideComponentProperties != null)
                {
                    additionalAttributesList.Add(string.Format(FIELD_XML_PARAMETER_FORMAT, "ClientSideComponentProperties", fieldCreationOptions.ClientSideComponentProperties));
                }
            }

            string newFieldCAML;
            if (additionalChildNodesList.Count > 0)
            {
                // Calculated fields require a Formula child node
                newFieldCAML = string.Format(FIELD_XML_FORMAT_WITH_CHILD_NODES,
                    fieldCreationOptions.FieldType,
                    fieldCreationOptions.InternalName,
                    fieldCreationOptions.DisplayName,
                    fieldCreationOptions.Id,
                    fieldCreationOptions.Group,
                    fieldCreationOptions.Required ? "TRUE" : "FALSE",
                    additionalAttributesList.Any() ? string.Join(" ", additionalAttributesList) : "",
                    string.Join("", additionalChildNodesList));
            }
            else
            {
                newFieldCAML = string.Format(FIELD_XML_FORMAT,
                    fieldCreationOptions.FieldType,
                    fieldCreationOptions.InternalName,
                    fieldCreationOptions.DisplayName,
                    fieldCreationOptions.Id,
                    fieldCreationOptions.Group,
                    fieldCreationOptions.Required ? "TRUE" : "FALSE",
                    additionalAttributesList.Any() ? string.Join(" ", additionalAttributesList) : "");
            }

            return newFieldCAML;
        }
        #endregion
    }
}
