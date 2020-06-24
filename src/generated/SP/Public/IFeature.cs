using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Feature object
    /// </summary>
    [ConcreteType(typeof(Feature))]
    public interface IFeature : IDataModel<IFeature>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid DefinitionId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DisplayName { get; set; }

        #endregion

    }
}
