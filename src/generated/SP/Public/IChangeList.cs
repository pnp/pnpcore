using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ChangeList object
    /// </summary>
    [ConcreteType(typeof(ChangeList))]
    public interface IChangeList : IDataModel<IChangeList>, IDataModelGet<IChangeList>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int BaseTemplate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Editor { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ListId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string RootFolderUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid WebId { get; set; }

        #endregion

    }
}
