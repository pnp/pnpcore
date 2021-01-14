using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of FieldLink objects of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(FieldLinkCollection))]
    public interface IFieldLinkCollection : IQueryable<IFieldLink>, IDataModelCollection<IFieldLink>, IDataModelCollectionDeleteByGuidId
    {
        /// <summary>
        /// Adds a field link via a batch
        /// </summary>
        /// <param name="fieldInternalName">Internal name of the field</param>
        /// <param name="displayName">Display name of the field</param>
        /// <param name="hidden">Field is hidden</param>
        /// <param name="required">Field is required</param>
        /// <param name="readOnly">Field is read only</param>
        /// <param name="showInDisplayForm">Show the field in the display form</param>
        /// <returns>Added field link</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        Task<IFieldLink> AddBatchAsync(string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true);

        /// <summary>
        /// Adds a field link via a batch
        /// </summary>
        /// <param name="fieldInternalName">Internal name of the field</param>
        /// <param name="displayName">Display name of the field</param>
        /// <param name="hidden">Field is hidden</param>
        /// <param name="required">Field is required</param>
        /// <param name="readOnly">Field is read only</param>
        /// <param name="showInDisplayForm">Show the field in the display form</param>
        /// <returns>Added field link</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        IFieldLink AddBatch(string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true);

        /// <summary>
        /// Adds a field link via a batch
        /// </summary>
        /// <param name="batch">Batcht to add this request to</param>
        /// <param name="fieldInternalName">Internal name of the field</param>
        /// <param name="displayName">Display name of the field</param>
        /// <param name="hidden">Field is hidden</param>
        /// <param name="required">Field is required</param>
        /// <param name="readOnly">Field is read only</param>
        /// <param name="showInDisplayForm">Show the field in the display form</param>
        /// <returns>Added field link</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        Task<IFieldLink> AddBatchAsync(Batch batch, string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true);

        /// <summary>
        /// Adds a field link via a batch
        /// </summary>
        /// <param name="batch">Batcht to add this request to</param>
        /// <param name="fieldInternalName">Internal name of the field</param>
        /// <param name="displayName">Display name of the field</param>
        /// <param name="hidden">Field is hidden</param>
        /// <param name="required">Field is required</param>
        /// <param name="readOnly">Field is read only</param>
        /// <param name="showInDisplayForm">Show the field in the display form</param>
        /// <returns>Added field link</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        IFieldLink AddBatch(Batch batch, string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true);

        /// <summary>
        /// Adds a field link 
        /// </summary>
        /// <param name="fieldInternalName">Internal name of the field</param>
        /// <param name="displayName">Display name of the field</param>
        /// <param name="hidden">Field is hidden</param>
        /// <param name="required">Field is required</param>
        /// <param name="readOnly">Field is read only</param>
        /// <param name="showInDisplayForm">Show the field in the display form</param>
        /// <returns>Added field link</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        Task<IFieldLink> AddAsync(string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true);


        /// <summary>
        /// Adds a field link 
        /// </summary>
        /// <param name="fieldInternalName">Internal name of the field</param>
        /// <param name="displayName">Display name of the field</param>
        /// <param name="hidden">Field is hidden</param>
        /// <param name="required">Field is required</param>
        /// <param name="readOnly">Field is read only</param>
        /// <param name="showInDisplayForm">Show the field in the display form</param>
        /// <returns>Added field link</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        IFieldLink Add(string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true);
    }
}
