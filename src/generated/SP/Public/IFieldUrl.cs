using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldUrl object
    /// </summary>
    [ConcreteType(typeof(FieldUrl))]
    public interface IFieldUrl : IDataModel<IFieldUrl>, IDataModelGet<IFieldUrl>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int DisplayFormat { get; set; }

        #endregion

    }
}
