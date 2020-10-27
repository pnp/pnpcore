using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ViewFieldCollection object
    /// </summary>
    [ConcreteType(typeof(ViewFieldCollection))]
    public interface IViewFieldCollection : IDataModel<IViewFieldCollection>, IDataModelGet<IViewFieldCollection>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public string SchemaXml { get; set; }

        #endregion

        #region New properties

        #endregion

    }
}
