using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a RequestUserContext object
    /// </summary>
    [ConcreteType(typeof(RequestUserContext))]
    public interface IRequestUserContext : IDataModel<IRequestUserContext>, IDataModelGet<IRequestUserContext>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Id4a81de82eeb94d6080ea5bf63e27023a { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IRequestUserContext Current { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser User { get; }

        #endregion

    }
}
