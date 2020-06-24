using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SecurableObject object
    /// </summary>
    [ConcreteType(typeof(SecurableObject))]
    public interface ISecurableObject : IDataModel<ISecurableObject>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool HasUniqueRoleAssignments { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public ISecurableObject FirstUniqueAncestorSecurableObject { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IRoleAssignmentCollection RoleAssignments { get; }

        #endregion

    }
}
