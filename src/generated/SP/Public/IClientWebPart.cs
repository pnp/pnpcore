using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ClientWebPart object
    /// </summary>
    [ConcreteType(typeof(ClientWebPart))]
    public interface IClientWebPart : IDataModel<IClientWebPart>, IDataModelGet<IClientWebPart>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; set; }

        #endregion

    }
}
