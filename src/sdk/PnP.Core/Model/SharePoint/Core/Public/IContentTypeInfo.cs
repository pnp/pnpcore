namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// The contentTypeInfo resource indicates the SharePoint content type of an item.
    /// </summary>
    [ConcreteType(typeof(ContentTypeInfo))]
    public interface IContentTypeInfo : IDataModel<IContentTypeInfo>
    {
        /// <summary>
        /// The id of the content type.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the content type.
        /// </summary>
        public string Name { get; set; }
    }
}
