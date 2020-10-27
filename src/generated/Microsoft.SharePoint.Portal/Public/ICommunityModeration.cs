using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a CommunityModeration object
    /// </summary>
    [ConcreteType(typeof(CommunityModeration))]
    public interface ICommunityModeration : IDataModel<ICommunityModeration>, IDataModelGet<ICommunityModeration>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Id4a81de82eeb94d6080ea5bf63e27023a { get; set; }

        #endregion

    }
}
