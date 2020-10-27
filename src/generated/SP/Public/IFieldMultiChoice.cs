using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldMultiChoice object
    /// </summary>
    [ConcreteType(typeof(FieldMultiChoice))]
    public interface IFieldMultiChoice : IDataModel<IFieldMultiChoice>, IDataModelGet<IFieldMultiChoice>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool FillInChoice { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Mappings { get; set; }

        #endregion

    }
}
