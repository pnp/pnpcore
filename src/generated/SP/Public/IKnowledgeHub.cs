using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a KnowledgeHub object
    /// </summary>
    [ConcreteType(typeof(KnowledgeHub))]
    public interface IKnowledgeHub : IDataModel<IKnowledgeHub>, IDataModelGet<IKnowledgeHub>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Id4a81de82eeb94d6080ea5bf63e27023a { get; set; }

        #endregion

    }
}
