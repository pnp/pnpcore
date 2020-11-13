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
        /// <param name="propertyName">Name of the property that will be set by this object</param>
        /// <param name="url">Url value</param>
        /// <param name="description">Optional description value</param>
        /// <returns>Configured <see cref="IFieldUrlValue"/> object</returns>
        public IFieldUrlValue NewFieldUrlValue(string propertyName, string url, string description = null);

        /// <summary>
        /// Creates a new <see cref="IFieldLookupValue"/> object
        /// </summary>
        /// <param name="propertyName">Name of the property that will be set by this object</param>
        /// <param name="lookupId">Id of the lookup value</param>
        /// <returns>Configured <see cref="IFieldLookupValue"/> object</returns>
        public IFieldLookupValue NewFieldLookupValue(string propertyName, int lookupId);

        /// <summary>
        /// Creates a new <see cref="IFieldUserValue"/> object
        /// </summary>
        /// <param name="propertyName">Name of the property that will be set by this object</param>
        /// <param name="userId">Id of the user</param>
        /// <returns>Configured <see cref="IFieldUserValue"/> object</returns>
        public IFieldUserValue NewFieldUserValue(string propertyName, int userId);

    }
}
