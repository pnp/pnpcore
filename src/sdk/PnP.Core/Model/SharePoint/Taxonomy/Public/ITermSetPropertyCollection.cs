namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of localized termset labels
    /// </summary>
    [ConcreteType(typeof(TermSetPropertyCollection))]
    public interface ITermSetPropertyCollection : IDataModelCollection<ITermSetProperty>
    {
    }
}
