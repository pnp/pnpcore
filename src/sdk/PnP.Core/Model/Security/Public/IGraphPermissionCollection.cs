namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Collection of the sharing permissions granted on a driveItem resource
    /// </summary>
    [ConcreteType(typeof(GraphPermissionCollection))]
    public interface IGraphPermissionCollection : IDataModelCollection<IGraphPermission>
    {
    }
}
