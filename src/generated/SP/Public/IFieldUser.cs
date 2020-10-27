using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldUser object
    /// </summary>
    [ConcreteType(typeof(FieldUser))]
    public interface IFieldUser : IDataModel<IFieldUser>, IDataModelGet<IFieldUser>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowDisplay { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Presence { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int SelectionGroup { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int SelectionMode { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string UserDisplayOptions { get; set; }

        #endregion

    }
}
