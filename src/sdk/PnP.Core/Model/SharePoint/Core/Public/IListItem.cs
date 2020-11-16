using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SharePoint Online list item
    /// </summary>
    [ConcreteType(typeof(ListItem))]
    public interface IListItem : IDataModel<IListItem>, IDataModelGet<IListItem>, IListItemBase, IDataModelUpdate, IDataModelDelete, IExpandoDataModel, IQueryableDataModel
    {
        /// <summary>
        /// Creates a new <see cref="IFieldUrlValue"/> object
        /// </summary>
        /// <param name="fieldToUpdate"><see cref="IField"/> representing the field to set</param>
        /// <param name="url">Url value</param>
        /// <param name="description">Optional description value</param>
        /// <returns>Configured <see cref="IFieldUrlValue"/> object</returns>
        public IFieldUrlValue NewFieldUrlValue(IField fieldToUpdate, string url, string description = null);

        /// <summary>
        /// Creates a new <see cref="IFieldLookupValue"/> object
        /// </summary>
        /// <param name="fieldToUpdate"><see cref="IField"/> representing the field to set</param>
        /// <param name="lookupId">Id of the lookup value</param>
        /// <returns>Configured <see cref="IFieldLookupValue"/> object</returns>
        public IFieldLookupValue NewFieldLookupValue(IField fieldToUpdate, int lookupId);

        /// <summary>
        /// Creates a new <see cref="IFieldUserValue"/> object
        /// </summary>
        /// <param name="fieldToUpdate"><see cref="IField"/> representing the field to set</param>
        /// <param name="userId">Id of the user</param>
        /// <returns>Configured <see cref="IFieldUserValue"/> object</returns>
        public IFieldUserValue NewFieldUserValue(IField fieldToUpdate, int userId);

        /// <summary>
        /// Creates a new <see cref="IFieldTaxonomyValue"/> object
        /// </summary>
        /// <param name="fieldToUpdate"><see cref="IField"/> representing the field to set</param>
        /// <param name="termId">Name of the term to set</param>
        /// <param name="label">Label of the term to set</param>
        /// <param name="wssId">Optionally provide the wssId value</param>
        /// <returns>Configured <see cref="IFieldTaxonomyValue"/> object</returns>
        public IFieldTaxonomyValue NewFieldTaxonomyValue(IField fieldToUpdate, Guid termId, string label, int wssId = -1);

        /// <summary>
        /// Creates a new collection to hold <see cref="IFieldValue"/> objects
        /// </summary>
        /// <param name="fieldToUpdate"><see cref="IField"/> representing the field to set</param>
        /// <param name="parent">List item values collection that's being updated by this collection</param>
        /// <returns></returns>
        public IFieldValueCollection NewFieldValueCollection(IField fieldToUpdate, TransientDictionary parent);

    }
}
