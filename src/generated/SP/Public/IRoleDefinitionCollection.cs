using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of RoleDefinition objects
    /// </summary>
    public interface IRoleDefinitionCollection : IQueryable<IRoleDefinition>, IDataModelCollection<IRoleDefinition>
    {
    }
}