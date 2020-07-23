using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Field objects of SharePoint Online
    /// </summary>
    public interface IFieldCollection : IQueryable<IField>, IDataModelCollection<IField>
    {
        // For FieldType
        // https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-server/ee540543%28v%3doffice.15%29

        #region Extension Methods

        /// <summary>
        /// Adds a new field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="fieldType">The type of the field to add</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddBatchAsync(string title, FieldType fieldType, FieldOptions options = null);

        /// <summary>
        /// Adds a new field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="fieldType">The type of the field to add</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddBatch(string title, FieldType fieldType, FieldOptions options = null);

        /// <summary>
        /// Adds a new Text field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddTextBatchAsync(string title, FieldTextOptions options = null);

        /// <summary>
        /// Adds a new Text field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddTextBatch(string title, FieldTextOptions options = null);

        /// <summary>
        /// Adds a new multiline Text field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddMultilineTextBatchAsync(string title, FieldMultilineTextOptions options = null);

        /// <summary>
        /// Adds a new multiline Text field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddMultilineTextBatch(string title, FieldMultilineTextOptions options = null);

        /// <summary>
        /// Adds a new URL field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUrlBatchAsync(string title, FieldUrlOptions options = null);

        /// <summary>
        /// Adds a new URL field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUrlBatch(string title, FieldUrlOptions options = null);

        /// <summary>
        /// Adds a new Number field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddNumberBatchAsync(string title, FieldNumberOptions options = null);

        /// <summary>
        /// Adds a new Number field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddNumberBatch(string title, FieldNumberOptions options = null);

        /// <summary>
        /// Adds a new DateTime field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddDateTimeBatchAsync(string title, FieldDateTimeOptions options = null);

        /// <summary>
        /// Adds a new DateTime field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddDateTimeBatch(string title, FieldDateTimeOptions options = null);

        /// <summary>
        /// Adds a new Calculated field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddCalculatedBatchAsync(string title, FieldCalculatedOptions options = null);

        /// <summary>
        /// Adds a new Calculated field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddCalculatedBatch(string title, FieldCalculatedOptions options = null);

        /// <summary>
        /// Adds a new Currency field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddCurrencyBatchAsync(string title, FieldCurrencyOptions options = null);

        /// <summary>
        /// Adds a new Currency field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddCurrencyBatch(string title, FieldCurrencyOptions options = null);

        /// <summary>
        /// Adds a new MultiChoice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddMultiChoiceBatchAsync(string title, FieldMultiChoiceOptions options = null);

        /// <summary>
        /// Adds a new MultiChoice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddMultiChoiceBatch(string title, FieldMultiChoiceOptions options = null);

        /// <summary>
        /// Adds a new Choice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddChoiceBatchAsync(string title, FieldChoiceOptions options = null);

        /// <summary>
        /// Adds a new Choice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddChoiceBatch(string title, FieldChoiceOptions options = null);

        /// <summary>
        /// Adds a new Lookup field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddLookupBatchAsync(string title, FieldLookupOptions options = null);

        /// <summary>
        /// Adds a new Lookup field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddLookupBatch(string title, FieldLookupOptions options = null);

        /// <summary>
        /// Adds a new User field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUserBatchAsync(string title, FieldUserOptions options = null);

        /// <summary>
        /// Adds a new User field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUserBatch(string title, FieldUserOptions options = null);

        /// <summary>
        /// Adds a new field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="fieldType">The type of the field to add</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddBatchAsync(Batch batch, string title, FieldType fieldType, FieldOptions options = null);

        /// <summary>
        /// Adds a new field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="fieldType">The type of the field to add</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddBatch(Batch batch, string title, FieldType fieldType, FieldOptions options = null);

        /// <summary>
        /// Adds a new Text field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddTextBatchAsync(Batch batch, string title, FieldTextOptions options = null);

        /// <summary>
        /// Adds a new Text field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddTextBatch(Batch batch, string title, FieldTextOptions options = null);

        /// <summary>
        /// Adds a new multiline Text field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddMultilineTextBatchAsync(Batch batch, string title, FieldMultilineTextOptions options = null);

        /// <summary>
        /// Adds a new multiline Text field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddMultilineTextBatch(Batch batch, string title, FieldMultilineTextOptions options = null);

        /// <summary>
        /// Adds a new URL field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUrlBatchAsync(Batch batch, string title, FieldUrlOptions options = null);

        /// <summary>
        /// Adds a new URL field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUrlBatch(Batch batch, string title, FieldUrlOptions options = null);

        /// <summary>
        /// Adds a new Number field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddNumberBatchAsync(Batch batch, string title, FieldNumberOptions options = null);

        /// <summary>
        /// Adds a new Number field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddNumberBatch(Batch batch, string title, FieldNumberOptions options = null);

        /// <summary>
        /// Adds a new DateTime field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddDateTimeBatchAsync(Batch batch, string title, FieldDateTimeOptions options = null);

        /// <summary>
        /// Adds a new DateTime field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddDateTimeBatch(Batch batch, string title, FieldDateTimeOptions options = null);

        /// <summary>
        /// Adds a new Calculated field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddCalculatedBatchAsync(Batch batch, string title, FieldCalculatedOptions options = null);

        /// <summary>
        /// Adds a new Calculated field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddCalculatedBatch(Batch batch, string title, FieldCalculatedOptions options = null);

        /// <summary>
        /// Adds a new Currency field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddCurrencyBatchAsync(Batch batch, string title, FieldCurrencyOptions options = null);

        /// <summary>
        /// Adds a new Currency field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddCurrencyBatch(Batch batch, string title, FieldCurrencyOptions options = null);

        /// <summary>
        /// Adds a new MultiChoice field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddMultiChoiceBatchAsync(Batch batch, string title, FieldMultiChoiceOptions options = null);

        /// <summary>
        /// Adds a new MultiChoice field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddMultiChoiceBatch(Batch batch, string title, FieldMultiChoiceOptions options = null);

        /// <summary>
        /// Adds a new Choice field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddChoiceBatchAsync(Batch batch, string title, FieldChoiceOptions options = null);

        /// <summary>
        /// Adds a new Choice field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddChoiceBatch(Batch batch, string title, FieldChoiceOptions options = null);

        /// <summary>
        /// Adds a new Lookup field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddLookupBatchAsync(Batch batch, string title, FieldLookupOptions options = null);

        /// <summary>
        /// Adds a new Lookup field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddLookupBatch(Batch batch, string title, FieldLookupOptions options = null);

        /// <summary>
        /// Adds a new User field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUserBatchAsync(Batch batch, string title, FieldUserOptions options = null);

        /// <summary>
        /// Adds a new User field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUserBatch(Batch batch, string title, FieldUserOptions options = null);

        /// <summary>
        /// Adds a new field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="fieldType">The type of the field to add</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddAsync(string title, FieldType fieldType, FieldOptions options = null);

        /// <summary>
        /// Adds a new field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="fieldType">The type of the field to add</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField Add(string title, FieldType fieldType, FieldOptions options = null);

        /// <summary>
        /// Adds a new Text field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddTextAsync(string title, FieldTextOptions options = null);

        /// <summary>
        /// Adds a new Text field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddText(string title, FieldTextOptions options = null);

        /// <summary>
        /// Adds a new multiline Text field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddMultilineTextAsync(string title, FieldMultilineTextOptions options = null);

        /// <summary>
        /// Adds a new multiline Text field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddMultilineText(string title, FieldMultilineTextOptions options = null);

        /// <summary>
        /// Adds a new URL field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUrlAsync(string title, FieldUrlOptions options = null);

        /// <summary>
        /// Adds a new URL field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUrl(string title, FieldUrlOptions options = null);

        /// <summary>
        /// Adds a new Number field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddNumberAsync(string title, FieldNumberOptions options = null);

        /// <summary>
        /// Adds a new Number field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddNumber(string title, FieldNumberOptions options = null);

        /// <summary>
        /// Adds a new DateTime field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddDateTimeAsync(string title, FieldDateTimeOptions options = null);

        /// <summary>
        /// Adds a new DateTime field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddDateTime(string title, FieldDateTimeOptions options = null);

        /// <summary>
        /// Adds a new Calculated field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddCalculatedAsync(string title, FieldCalculatedOptions options = null);

        /// <summary>
        /// Adds a new Currency field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddCurrencyAsync(string title, FieldCurrencyOptions options = null);

        /// <summary>
        /// Adds a new Currency field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddCurrency(string title, FieldCurrencyOptions options = null);

        /// <summary>
        /// Adds a new MultiChoice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddMultiChoiceAsync(string title, FieldMultiChoiceOptions options = null);

        /// <summary>
        /// Adds a new MultiChoice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddMultiChoice(string title, FieldMultiChoiceOptions options = null);

        /// <summary>
        /// Adds a new Choice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddChoiceAsync(string title, FieldChoiceOptions options = null);

        /// <summary>
        /// Adds a new Choice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddChoice(string title, FieldChoiceOptions options = null);

        /// <summary>
        /// Adds a new Lookup field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddLookupAsync(string title, FieldLookupOptions options = null);

        /// <summary>
        /// Adds a new Lookup field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddLookup(string title, FieldLookupOptions options = null);

        /// <summary>
        /// Adds a new User field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUserAsync(string title, FieldUserOptions options = null);

        /// <summary>
        /// Adds a new User field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUser(string title, FieldUserOptions options = null);

        /// <summary>
        /// Adds a new field from its XML schema
        /// </summary>
        /// <param name="schemaXml">
        /// A Collaborative Application Markup Language (CAML) string that contains the schema.
        /// It must not be a null reference(Nothing in Visual Basic). 
        /// It must not be empty.It must be a valid Collaborative Application Markup Language(CAML) string according to the schema specified in [MS-WSSFO2], section 2.2.9.3.3.1.
        /// </param>
        /// <param name="addToDefaultView">Specifies to add the field to the default list view.
        /// <c>true</c> if the field is added to the default list view; otherwise, <c>false</c>.</param>
        /// <param name="options">An AddFieldOptionsFlags value that specifies the field options.</param>
        /// <returns>The added field</returns>
        Task<IField> AddFieldAsXmlBatchAsync(string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue);

        /// <summary>
        /// Adds a new field from its XML schema
        /// </summary>
        /// <param name="schemaXml">
        /// A Collaborative Application Markup Language (CAML) string that contains the schema.
        /// It must not be a null reference(Nothing in Visual Basic). 
        /// It must not be empty.It must be a valid Collaborative Application Markup Language(CAML) string according to the schema specified in [MS-WSSFO2], section 2.2.9.3.3.1.
        /// </param>
        /// <param name="addToDefaultView">Specifies to add the field to the default list view.
        /// <c>true</c> if the field is added to the default list view; otherwise, <c>false</c>.</param>
        /// <param name="options">An AddFieldOptionsFlags value that specifies the field options.</param>
        /// <returns>The added field</returns>
        IField AddFieldAsXmlBatch(string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue);

        /// <summary>
        /// Adds a new field from its XML schema
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="schemaXml">
        /// A Collaborative Application Markup Language (CAML) string that contains the schema.
        /// It must not be a null reference(Nothing in Visual Basic). 
        /// It must not be empty.It must be a valid Collaborative Application Markup Language(CAML) string according to the schema specified in [MS-WSSFO2], section 2.2.9.3.3.1.
        /// </param>
        /// <param name="addToDefaultView">Specifies to add the field to the default list view.
        /// <c>true</c> if the field is added to the default list view; otherwise, <c>false</c>.</param>
        /// <param name="options">An AddFieldOptionsFlags value that specifies the field options.</param>
        /// <returns>The added field</returns>
        Task<IField> AddFieldAsXmlBatchAsync(Batch batch, string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue);

        /// <summary>
        /// Adds a new field from its XML schema
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="schemaXml">
        /// A Collaborative Application Markup Language (CAML) string that contains the schema.
        /// It must not be a null reference(Nothing in Visual Basic). 
        /// It must not be empty.It must be a valid Collaborative Application Markup Language(CAML) string according to the schema specified in [MS-WSSFO2], section 2.2.9.3.3.1.
        /// </param>
        /// <param name="addToDefaultView">Specifies to add the field to the default list view.
        /// <c>true</c> if the field is added to the default list view; otherwise, <c>false</c>.</param>
        /// <param name="options">An AddFieldOptionsFlags value that specifies the field options.</param>
        /// <returns>The added field</returns>
        IField AddFieldAsXmlBatch(Batch batch, string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue);

        /// <summary>
        /// Adds a new field from its XML schema
        /// </summary>
        /// <param name="schemaXml">
        /// A Collaborative Application Markup Language (CAML) string that contains the schema.
        /// It must not be a null reference(Nothing in Visual Basic). 
        /// It must not be empty.It must be a valid Collaborative Application Markup Language(CAML) string according to the schema specified in [MS-WSSFO2], section 2.2.9.3.3.1.
        /// </param>
        /// <param name="addToDefaultView">Specifies to add the field to the default list view.
        /// <c>true</c> if the field is added to the default list view; otherwise, <c>false</c>.</param>
        /// <param name="options">An AddFieldOptionsFlags value that specifies the field options.</param>
        /// <returns>The added field</returns>
        Task<IField> AddFieldAsXmlAsync(string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue);

        /// <summary>
        /// Adds a new field from its XML schema
        /// </summary>
        /// <param name="schemaXml">
        /// A Collaborative Application Markup Language (CAML) string that contains the schema.
        /// It must not be a null reference(Nothing in Visual Basic). 
        /// It must not be empty.It must be a valid Collaborative Application Markup Language(CAML) string according to the schema specified in [MS-WSSFO2], section 2.2.9.3.3.1.
        /// </param>
        /// <param name="addToDefaultView">Specifies to add the field to the default list view.
        /// <c>true</c> if the field is added to the default list view; otherwise, <c>false</c>.</param>
        /// <param name="options">An AddFieldOptionsFlags value that specifies the field options.</param>
        /// <returns>The added field</returns>
        IField AddFieldAsXml(string schemaXml, bool addToDefaultView = false, AddFieldOptionsFlags options = AddFieldOptionsFlags.DefaultValue);
        #endregion
    }
}
