using PnP.Core.Model.SharePoint;

namespace PnP.Core.Model.Security
{


    /// <summary>
    /// Defines a single role definition, including a name, description, and set of rights.
    /// </summary>
    [ConcreteType(typeof(RoleAssignment))]
    public interface IRoleAssignment : IDataModel<IRoleAssignment>, IDataModelGet<IRoleAssignment>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {
        /// <summary>
        /// Gets or sets a value that specifies the base permissions for the role definition.
        /// </summary>
        public int PrincipalId { get; set; }

        /// <summary>
        /// Role definitions for this assignment
        /// </summary>
        public IRoleDefinitionCollection RoleDefinitions { get; }
    }
}