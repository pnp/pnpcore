namespace PnP.Core.Model.SharePoint
{
    [ConcreteType(typeof(ContentTypeId))]
    public interface IContentTypeId : IComplexType
    {
        /// <summary>
        /// The String Value of the Id of the content type
        /// </summary>
        public string StringValue { get; set; }
    }
}
