using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Form object
    /// </summary>
    [ConcreteType(typeof(Form))]
    public interface IForm : IDataModel<IForm>, IDataModelGet<IForm>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServerRelativeUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int FormType { get; set; }

        #endregion

    }
}
