using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a userEntity object
    /// </summary>
    [ConcreteType(typeof(userEntity))]
    public interface IuserEntity : IDataModel<IuserEntity>, IDataModelGet<IuserEntity>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; set; }

        #endregion

    }
}
