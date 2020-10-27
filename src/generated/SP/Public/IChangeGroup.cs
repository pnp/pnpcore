using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ChangeGroup object
    /// </summary>
    [ConcreteType(typeof(ChangeGroup))]
    public interface IChangeGroup : IDataModel<IChangeGroup>, IDataModelGet<IChangeGroup>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int GroupId { get; set; }

        #endregion

    }
}
