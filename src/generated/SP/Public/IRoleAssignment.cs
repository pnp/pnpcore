using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a RoleAssignment object
    /// </summary>
    [ConcreteType(typeof(RoleAssignment))]
    public interface IRoleAssignment : IDataModel<IRoleAssignment>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int PrincipalId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IPrincipal Member { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IRoleDefinitionCollection RoleDefinitionBindings { get; }

        #endregion

    }
}
