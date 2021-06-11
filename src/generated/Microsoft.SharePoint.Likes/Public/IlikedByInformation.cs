using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a likedByInformation object
    /// </summary>
    [ConcreteType(typeof(likedByInformation))]
    public interface IlikedByInformation : IDataModel<IlikedByInformation>, IDataModelGet<IlikedByInformation>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsLikedByUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IuserEntityCollection LikedBy { get; }

        #endregion

    }
}
