namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of localized termset labels
    /// </summary>
    [ConcreteType(typeof(TermLocalizedLabelCollection))]
    public interface ITermLocalizedLabelCollection : IDataModelCollection<ITermLocalizedLabel>, ISupportQuery<ITermLocalizedLabel>, ISupportModules<ITermLocalizedLabelCollection>
    {
    }
}