using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ApiMetadata object
    /// </summary>
    [ConcreteType(typeof(ApiMetadata))]
    public interface IApiMetadata : IDataModel<IApiMetadata>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Id4a81de82eeb94d6080ea5bf63e27023a { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IApiMetadata Current { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public ITypeInformationCollection Types { get; }

        #endregion

    }
}
