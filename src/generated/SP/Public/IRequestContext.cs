using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a RequestContext object
    /// </summary>
    [ConcreteType(typeof(RequestContext))]
    public interface IRequestContext : IDataModel<IRequestContext>, IDataModelGet<IRequestContext>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Id4a81de82eeb94d6080ea5bf63e27023a { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IRequestContext Current { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IList List { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public ISite Site { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IWeb Web { get; }

        #endregion

    }
}
