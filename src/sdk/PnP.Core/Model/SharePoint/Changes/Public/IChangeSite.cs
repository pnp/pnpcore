namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates a change to a <seealso cref="ISite"/> object.
    /// </summary>
    /// <seealso cref="IChange" />
    [ConcreteType(typeof(ChangeSite))]
    public interface IChangeSite : IChange
    {
        // No additional properties
    }
}