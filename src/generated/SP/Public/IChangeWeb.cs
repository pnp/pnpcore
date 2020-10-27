using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ChangeWeb object
    /// </summary>
    [ConcreteType(typeof(ChangeWeb))]
    public interface IChangeWeb : IDataModel<IChangeWeb>, IDataModelGet<IChangeWeb>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid WebId { get; set; }

        #endregion

    }
}
