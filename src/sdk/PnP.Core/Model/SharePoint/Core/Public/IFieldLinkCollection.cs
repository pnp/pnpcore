using PnP.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of FieldLink objects of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(FieldLinkCollection))]
    public interface IFieldLinkCollection : IQueryable<IFieldLink>, IAsyncEnumerable<IFieldLink>, IDataModelCollection<IFieldLink>, IDataModelCollectionLoad<IFieldLink>, IDataModelCollectionDeleteByGuidId, ISupportModules<IFieldLinkCollection>
    {
        /// <summary>
        /// Adds a field link via a batch
        /// </summary>
        /// <param name="field">Field to add as field link</param>
        Task AddBatchAsync(IField field);

        /// <summary>
        /// Adds a field link via a batch
        /// </summary>
        /// <param name="field">Field to add as field link</param>
        void AddBatch(IField field);

        /// <summary>
        /// Adds a field link via a batch
        /// </summary>
        /// <param name="batch">Batcht to add this request to</param>
        /// <param name="field">Field to add as field link</param>
        Task AddBatchAsync(Batch batch, IField field);

        /// <summary>
        /// Adds a field link via a batch
        /// </summary>
        /// <param name="batch">Batcht to add this request to</param>
        /// <param name="field">Field to add as field link</param>
        void AddBatch(Batch batch, IField field);

        /// <summary>
        /// Adds a field link for the given field
        /// </summary>
        /// <param name="field">Field to add as field link</param>
        /// <param name="displayName">Display name of the field</param>
        /// <param name="hidden">Field is hidden</param>
        /// <param name="required">Field is required</param>
        /// <param name="readOnly">Field is read only</param>
        /// <param name="showInDisplayForm">Show the field in the display form</param>
        /// <returns>added FieldLink</returns>
        Task<IFieldLink> AddAsync(IField field, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true);

        /// <summary>
        /// Adds a field link for the given field
        /// </summary>
        /// <param name="field">Field to add as field link</param>
        /// <param name="displayName">Display name of the field</param>
        /// <param name="hidden">Field is hidden</param>
        /// <param name="required">Field is required</param>
        /// <param name="readOnly">Field is read only</param>
        /// <param name="showInDisplayForm">Show the field in the display form</param>
        /// <returns>added FieldLink</returns>
        IFieldLink Add(IField field, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true);

    }
}
