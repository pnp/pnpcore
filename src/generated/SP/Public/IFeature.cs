using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Feature object
    /// </summary>
    [ConcreteType(typeof(Feature))]
    public interface IFeature : IDataModel<IFeature>, IDataModelGet<IFeature>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid DefinitionId { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DisplayName { get; }

        #endregion

        #region New properties

        #endregion

    }
}
