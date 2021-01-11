namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines the granted permission mask
    /// </summary>
    [ConcreteType(typeof(BasePermissions))]
    public interface IBasePermissions : IDataModel<IBasePermissions>
    {
        /// <summary>
        /// Minimal granted permission mask
        /// </summary>
        public long Low { get; set; }

        /// <summary>
        /// Maximum granted permission mask
        /// </summary>
        public long High { get; set; }


        #region methods

        /// <summary>
        /// Clears the specified permission for the current instance.
        /// </summary>
        void Clear(PermissionKind permissionKind);
        
        /// <summary>
        /// Clears all permissions for the current instance.
        /// </summary>
        void ClearAll();

        /// <summary>
        /// Is this requested <see cref="PermissionKind"/> include?
        /// </summary>
        /// <param name="perm"><see cref="PermissionKind"/> permisson</param>
        /// <returns>True if included, false otherwise</returns>
        public bool Has(PermissionKind perm);

        /// <summary>
        /// Are the requested permission masks included?
        /// </summary>
        /// <param name="high">High end mask</param>
        /// <param name="low">Low end mask</param>
        /// <returns>True if included, false otherwise</returns>
        public bool HasPermissions(uint high, uint low);

        /// <summary>
        /// Sets the specified permission for the current instance.
        /// </summary>
        /// <param name="permissionKind"></param>
#pragma warning disable CA1716 // override reserved keywork Set
        void Set(PermissionKind permissionKind);
#pragma warning restore CA1716

        #endregion


    }
}
