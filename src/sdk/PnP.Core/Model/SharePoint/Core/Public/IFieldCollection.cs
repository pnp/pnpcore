using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Field objects of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(FieldCollection))]
    public interface IFieldCollection : IQueryable<IField>, IDataModelCollection<IField>
    {
        // For FieldType
        // https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-server/ee540543%28v%3doffice.15%29

        #region Extension Methods

        #region Text fields
        /// <summary>
        /// Adds a new Text field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddTextBatchAsync(string title, FieldTextOptions options = null);

        /// <summary>
        /// Adds a new Text field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddTextBatch(string title, FieldTextOptions options = null);

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
        Task<IField> AddMultilineTextBatchAsync(string title, FieldMultilineTextOptions options);

        /// <summary>
        /// Adds a new multiline Text field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddMultilineTextBatch(string title, FieldMultilineTextOptions options);

        /// <summary>
        /// Adds a new multiline Text field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddMultilineTextBatchAsync(Batch batch, string title, FieldMultilineTextOptions options);

        /// <summary>
        /// Adds a new multiline Text field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddMultilineTextBatch(Batch batch, string title, FieldMultilineTextOptions options);

        /// <summary>
        /// Adds a new multiline Text field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddMultilineTextAsync(string title, FieldMultilineTextOptions options);

        /// <summary>
        /// Adds a new multiline Text field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddMultilineText(string title, FieldMultilineTextOptions options);
        #endregion

        #region Number fields
        /// <summary>
        /// Adds a new Number field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddNumberBatchAsync(string title, FieldNumberOptions options);

        /// <summary>
        /// Adds a new Number field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddNumberBatch(string title, FieldNumberOptions options);

        /// <summary>
        /// Adds a new Number field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddNumberBatchAsync(Batch batch, string title, FieldNumberOptions options);

        /// <summary>
        /// Adds a new Number field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddNumberBatch(Batch batch, string title, FieldNumberOptions options);

        /// <summary>
        /// Adds a new Number field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddNumberAsync(string title, FieldNumberOptions options);

        /// <summary>
        /// Adds a new Number field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddNumber(string title, FieldNumberOptions options);
        #endregion

        #region Boolean fields
        /// <summary>
        /// Adds a new Boolean field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddBooleanBatchAsync(string title, FieldBooleanOptions options);

        /// <summary>
        /// Adds a new Boolean field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddBooleanBatch(string title, FieldBooleanOptions options);

        /// <summary>
        /// Adds a new Boolean field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddBooleanBatchAsync(Batch batch, string title, FieldBooleanOptions options);

        /// <summary>
        /// Adds a new Boolean field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddBooleanBatch(Batch batch, string title, FieldBooleanOptions options);

        /// <summary>
        /// Adds a new Boolean field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddBooleanAsync(string title, FieldBooleanOptions options);

        /// <summary>
        /// Adds a new Boolean field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddBoolean(string title, FieldBooleanOptions options);
        #endregion

        #region DateTime fields
        /// <summary>
        /// Adds a new DateTime field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddDateTimeBatchAsync(string title, FieldDateTimeOptions options);

        /// <summary>
        /// Adds a new DateTime field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddDateTimeBatch(string title, FieldDateTimeOptions options);

        /// <summary>
        /// Adds a new DateTime field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddDateTimeBatchAsync(Batch batch, string title, FieldDateTimeOptions options);

        /// <summary>
        /// Adds a new DateTime field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddDateTimeBatch(Batch batch, string title, FieldDateTimeOptions options);

        /// <summary>
        /// Adds a new DateTime field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddDateTimeAsync(string title, FieldDateTimeOptions options);

        /// <summary>
        /// Adds a new DateTime field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddDateTime(string title, FieldDateTimeOptions options);

        #endregion

        #region Currency fields

        /// <summary>
        /// Adds a new Currency field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddCurrencyBatchAsync(string title, FieldCurrencyOptions options);

        /// <summary>
        /// Adds a new Currency field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddCurrencyBatch(string title, FieldCurrencyOptions options);

        /// <summary>
        /// Adds a new Currency field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddCurrencyBatchAsync(Batch batch, string title, FieldCurrencyOptions options);

        /// <summary>
        /// Adds a new Currency field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddCurrencyBatch(Batch batch, string title, FieldCurrencyOptions options);

        /// <summary>
        /// Adds a new Currency field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddCurrencyAsync(string title, FieldCurrencyOptions options);

        /// <summary>
        /// Adds a new Currency field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddCurrency(string title, FieldCurrencyOptions options);
        #endregion

        #region Calculated fields
        /// <summary>
        /// Adds a new Calculated field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddCalculatedBatchAsync(string title, FieldCalculatedOptions options);

        /// <summary>
        /// Adds a new Calculated field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddCalculatedBatch(string title, FieldCalculatedOptions options);

        /// <summary>
        /// Adds a new Calculated field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddCalculatedBatchAsync(Batch batch, string title, FieldCalculatedOptions options);

        /// <summary>
        /// Adds a new Calculated field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddCalculatedBatch(Batch batch, string title, FieldCalculatedOptions options);

        /// <summary>
        /// Adds a new Calculated field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddCalculatedAsync(string title, FieldCalculatedOptions options);

        /// <summary>
        /// Adds a new Calculated field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddCalculated(string title, FieldCalculatedOptions options);
        #endregion

        #region Choice fields
        /// <summary>
        /// Adds a new MultiChoice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddChoiceMultiBatchAsync(string title, FieldChoiceMultiOptions options);

        /// <summary>
        /// Adds a new MultiChoice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddChoiceMultiBatch(string title, FieldChoiceMultiOptions options);

        /// <summary>
        /// Adds a new MultiChoice field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddChoiceMultiBatchAsync(Batch batch, string title, FieldChoiceMultiOptions options);

        /// <summary>
        /// Adds a new MultiChoice field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddChoiceMultiBatch(Batch batch, string title, FieldChoiceMultiOptions options);

        /// <summary>
        /// Adds a new MultiChoice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddChoiceMultiAsync(string title, FieldChoiceMultiOptions options);

        /// <summary>
        /// Adds a new MultiChoice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddChoiceMulti(string title, FieldChoiceMultiOptions options);

        /// <summary>
        /// Adds a new Choice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddChoiceBatchAsync(string title, FieldChoiceOptions options);

        /// <summary>
        /// Adds a new Choice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddChoiceBatch(string title, FieldChoiceOptions options);

        /// <summary>
        /// Adds a new Choice field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddChoiceBatchAsync(Batch batch, string title, FieldChoiceOptions options);

        /// <summary>
        /// Adds a new Choice field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddChoiceBatch(Batch batch, string title, FieldChoiceOptions options);

        /// <summary>
        /// Adds a new Choice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddChoiceAsync(string title, FieldChoiceOptions options);

        /// <summary>
        /// Adds a new Choice field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddChoice(string title, FieldChoiceOptions options);

        #endregion

        #region Taxonomy fields
        /// <summary>
        /// Adds a new Taxonomy field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddTaxonomy(string title, FieldTaxonomyOptions options);

        /// <summary>
        /// Adds a new Taxonomy field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddTaxonomyAsync(string title, FieldTaxonomyOptions options);

        /// <summary>
        /// Adds a new Taxonomy Multi field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddTaxonomyMulti(string title, FieldTaxonomyOptions options);

        /// <summary>
        /// Adds a new Taxonomy Multi field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddTaxonomyMultiAsync(string title, FieldTaxonomyOptions options);
        #endregion

        #region Lookup fields
        /// <summary>
        /// Adds a new Lookup field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddLookupBatchAsync(string title, FieldLookupOptions options);

        /// <summary>
        /// Adds a new Lookup field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddLookupBatch(string title, FieldLookupOptions options);

        /// <summary>
        /// Adds a new Lookup field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddLookupBatchAsync(Batch batch, string title, FieldLookupOptions options);

        /// <summary>
        /// Adds a new Lookup field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddLookupBatch(Batch batch, string title, FieldLookupOptions options);

        /// <summary>
        /// Adds a new Lookup field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddLookupAsync(string title, FieldLookupOptions options);

        /// <summary>
        /// Adds a new Lookup field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddLookup(string title, FieldLookupOptions options);

        /// <summary>
        /// Adds a new Lookup Multi field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddLookupMultiBatchAsync(string title, FieldLookupOptions options);

        /// <summary>
        /// Adds a new Lookup Multi field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddLookupMultiBatch(string title, FieldLookupOptions options);

        /// <summary>
        /// Adds a new Lookup Multi field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddLookupMultiBatchAsync(Batch batch, string title, FieldLookupOptions options);

        /// <summary>
        /// Adds a new Lookup Multi field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddLookupMultiBatch(Batch batch, string title, FieldLookupOptions options);

        /// <summary>
        /// Adds a new Lookup Multi field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddLookupMultiAsync(string title, FieldLookupOptions options);

        /// <summary>
        /// Adds a new Lookup Multi field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddLookupMulti(string title, FieldLookupOptions options);
        #endregion

        #region User fields
        /// <summary>
        /// Adds a new User field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUserBatchAsync(string title, FieldUserOptions options);

        /// <summary>
        /// Adds a new User field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUserBatch(string title, FieldUserOptions options);

        /// <summary>
        /// Adds a new User Multi field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUserMultiBatchAsync(string title, FieldUserOptions options);

        /// <summary>
        /// Adds a new User Multi field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUserMultiBatch(string title, FieldUserOptions options);

        /// <summary>
        /// Adds a new User field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUserBatchAsync(Batch batch, string title, FieldUserOptions options);

        /// <summary>
        /// Adds a new User field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUserBatch(Batch batch, string title, FieldUserOptions options);

        /// <summary>
        /// Adds a new User Multi field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUserMultiBatchAsync(Batch batch, string title, FieldUserOptions options);

        /// <summary>
        /// Adds a new User Multi field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUserMultiBatch(Batch batch, string title, FieldUserOptions options);

        /// <summary>
        /// Adds a new User field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUserAsync(string title, FieldUserOptions options);

        /// <summary>
        /// Adds a new User field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUser(string title, FieldUserOptions options);

        /// <summary>
        /// Adds a new Multi User field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUserMulti(string title, FieldUserOptions options);

        /// <summary>
        /// Adds a new Multi User field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUserMultiAsync(string title, FieldUserOptions options);
        #endregion

        #region Url fields
        /// <summary>
        /// Adds a new URL field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUrlBatchAsync(Batch batch, string title, FieldUrlOptions options);

        /// <summary>
        /// Adds a new URL field to the collection
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUrlBatch(Batch batch, string title, FieldUrlOptions options);

        /// <summary>
        /// Adds a new URL field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUrlBatchAsync(string title, FieldUrlOptions options);

        /// <summary>
        /// Adds a new URL field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUrlBatch(string title, FieldUrlOptions options);

        /// <summary>
        /// Adds a new URL field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        Task<IField> AddUrlAsync(string title, FieldUrlOptions options);

        /// <summary>
        /// Adds a new URL field to the collection
        /// </summary>
        /// <param name="title">The title of the field</param>
        /// <param name="options">The specific options for field creation</param>
        /// <returns>The added field</returns>
        IField AddUrl(string title, FieldUrlOptions options);
        #endregion

        #region Add field as xml
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

        #region Generic field creation
        /// <summary>
        /// Generic batch field add option based upon information specified in a <see cref="FieldCreationOptions"/> object
        /// </summary>
        /// <param name="fieldCreationOptions">The <see cref="FieldCreationOptions"/> object containing information used to add the field</param>
        /// <returns>The added field</returns>
        IField AddFieldBatch(FieldCreationOptions fieldCreationOptions);

        /// <summary>
        /// Generic batch field add option based upon information specified in a <see cref="FieldCreationOptions"/> object
        /// </summary>
        /// <param name="fieldCreationOptions">The <see cref="FieldCreationOptions"/> object containing information used to add the field</param>
        /// <returns>The added field</returns>
        Task<IField> AddFieldBatchAsync(FieldCreationOptions fieldCreationOptions);

        /// <summary>
        /// Generic batch field add option based upon information specified in a <see cref="FieldCreationOptions"/> object
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="fieldCreationOptions">The <see cref="FieldCreationOptions"/> object containing information used to add the field</param>
        /// <returns>The added field</returns>
        IField AddFieldBatch(Batch batch, FieldCreationOptions fieldCreationOptions);

        /// <summary>
        /// Generic batch field add option based upon information specified in a <see cref="FieldCreationOptions"/> object
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="fieldCreationOptions">The <see cref="FieldCreationOptions"/> object containing information used to add the field</param>
        /// <returns>The added field</returns>
        Task<IField> AddFieldBatchAsync(Batch batch, FieldCreationOptions fieldCreationOptions);

        /// <summary>
        /// Generic field add option based upon information specified in a <see cref="FieldCreationOptions"/> object
        /// </summary>
        /// <param name="fieldCreationOptions">The <see cref="FieldCreationOptions"/> object containing information used to add the field</param>
        /// <returns>The added field</returns>
        IField AddField(FieldCreationOptions fieldCreationOptions);

        /// <summary>
        /// Generic field add option based upon information specified in a <see cref="FieldCreationOptions"/> object
        /// </summary>
        /// <param name="fieldCreationOptions">The <see cref="FieldCreationOptions"/> object containing information used to add the field</param>
        /// <returns>The added field</returns>
        Task<IField> AddFieldAsync(FieldCreationOptions fieldCreationOptions);
        #endregion

        #endregion
    }
}
