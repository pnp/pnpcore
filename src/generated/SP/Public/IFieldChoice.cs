using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldChoice object
    /// </summary>
    [ConcreteType(typeof(FieldChoice))]
    public interface IFieldChoice : IDataModel<IFieldChoice>, IDataModelGet<IFieldChoice>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int EditFormat { get; set; }

        #endregion

    }
}
