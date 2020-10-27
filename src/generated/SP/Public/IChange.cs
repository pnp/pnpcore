using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Change object
    /// </summary>
    [ConcreteType(typeof(Change))]
    public interface IChange : IDataModel<IChange>, IDataModelGet<IChange>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int ChangeType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string RelativeTime { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime Time { get; set; }

        #endregion

    }
}
