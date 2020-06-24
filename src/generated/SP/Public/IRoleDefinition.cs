using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a RoleDefinition object
    /// </summary>
    [ConcreteType(typeof(RoleDefinition))]
    public interface IRoleDefinition : IDataModel<IRoleDefinition>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int RoleTypeKind { get; set; }

        #endregion

    }
}
