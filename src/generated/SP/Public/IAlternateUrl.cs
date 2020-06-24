using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a AlternateUrl object
    /// </summary>
    [ConcreteType(typeof(AlternateUrl))]
    public interface IAlternateUrl : IDataModel<IAlternateUrl>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int UrlZone { get; set; }

        #endregion

    }
}
