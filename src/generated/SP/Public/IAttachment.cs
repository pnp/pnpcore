using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Attachment object
    /// </summary>
    [ConcreteType(typeof(Attachment))]
    public interface IAttachment : IDataModel<IAttachment>, IDataModelGet<IAttachment>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServerRelativeUrl { get; set; }

        #endregion

    }
}
