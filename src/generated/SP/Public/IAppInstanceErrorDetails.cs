using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a AppInstanceErrorDetails object
    /// </summary>
    [ConcreteType(typeof(AppInstanceErrorDetails))]
    public interface IAppInstanceErrorDetails : IDataModel<IAppInstanceErrorDetails>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ErrorDetail { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ErrorType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ErrorTypeName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Source { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SourceName { get; set; }

        #endregion

    }
}
