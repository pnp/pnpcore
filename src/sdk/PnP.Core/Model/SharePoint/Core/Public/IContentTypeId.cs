namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Content Type Id object
    /// </summary>
    [ConcreteType(typeof(ContentTypeId))]
    public interface IContentTypeId : IDataModel<IContentTypeId>
    {
        /// <summary>
        /// The unique ID of the Content Type as string
        /// </summary>
        public string StringValue { get; }

    }
}
