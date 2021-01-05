using PnP.Core.Model.SharePoint;

namespace PnP.Core.Model.Security
{


    /// <summary>
    /// Defines a single role definition, including a name, description, and set of rights.
    /// </summary>
    [ConcreteType(typeof(RoleDefinition))]
    public interface IRoleDefinition : IDataModel<IRoleDefinition>, IDataModelGet<IRoleDefinition>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {
        /// <summary>
        /// Gets or sets a value that specifies the base permissions for the role definition.
        /// </summary>
        public IBasePermissions BasePermissions { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the description of the role definition.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets a value that specifies whether the role definition is displayed.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets a value that specifies the Id of the role definition.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets or sets a value that specifies the role definition name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the order position of the object in the site collection Permission Levels page.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets a value that specifies the type of the role definition.
        /// </summary>
        public RoleType RoleTypeKind { get; set; }
    }
}