using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Alert object
    /// </summary>
    [ConcreteType(typeof(Alert))]
    public interface IAlert : IDataModel<IAlert>, IDataModelGet<IAlert>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int AlertFrequency { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string AlertTemplateName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime AlertTime { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int AlertType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AlwaysNotify { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int DeliveryChannels { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int EventType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ItemID { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ListID { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ListUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IPropertyValues AllProperties { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IListItem Item { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IList List { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser User { get; }

        #endregion

    }
}
