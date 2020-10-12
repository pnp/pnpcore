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

        #endregion

    }
}
