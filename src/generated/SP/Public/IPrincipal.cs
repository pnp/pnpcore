using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Principal object
    /// </summary>
    [ConcreteType(typeof(Principal))]
    public interface IPrincipal : IDataModel<IPrincipal>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsHiddenInUI { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int PrincipalType { get; set; }

        #endregion

    }
}
