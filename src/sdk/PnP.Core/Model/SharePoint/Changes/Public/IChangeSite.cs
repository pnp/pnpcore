namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates a change to a <seealso cref="ISite"/> object.
    /// </summary>
    /// <seealso cref="PnP.Core.Model.SharePoint.IChange" />
    [ConcreteType(typeof(ChangeSite))]
    public interface IChangeSite : IChange
    {
        // No additional properties
    }
}