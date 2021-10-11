namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of file version events
    /// </summary>
    [ConcreteType(typeof(FileVersionEventCollection))]
    public interface IFileVersionEventCollection : IDataModelCollection<IFileVersionEvent>, IDataModelCollectionLoad<IFileVersionEvent>, ISupportModules<IFileVersionEventCollection>
    {

    }
}
