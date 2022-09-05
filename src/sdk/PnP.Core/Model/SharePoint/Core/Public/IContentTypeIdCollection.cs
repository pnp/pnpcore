namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ContentType objects of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(ContentTypeIdCollection))]
    public interface IContentTypeIdCollection : IDataModelCollection<IContentTypeId>, ISupportModules<IContentTypeIdCollection>
    {
    }
}
