using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a TeamChannel object
    /// </summary>
    [ConcreteType(typeof(TeamChannel))]
    public interface ITeamChannel : IDataModel<ITeamChannel>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid FolderId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int GroupId { get; set; }

        #endregion

    }
}
