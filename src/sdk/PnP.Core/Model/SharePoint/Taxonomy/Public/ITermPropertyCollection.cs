namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of localized termset labels
    /// </summary>
    [ConcreteType(typeof(TermPropertyCollection))]
    public interface ITermPropertyCollection : IDataModelCollection<ITermProperty>, IDataModelCollectionLoad<ITermProperty>, ISupportQuery<ITermProperty>
    {
    }
}