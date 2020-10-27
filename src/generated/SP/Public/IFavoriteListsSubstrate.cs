using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FavoriteListsSubstrate object
    /// </summary>
    [ConcreteType(typeof(FavoriteListsSubstrate))]
    public interface IFavoriteListsSubstrate : IDataModel<IFavoriteListsSubstrate>, IDataModelGet<IFavoriteListsSubstrate>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Id4a81de82eeb94d6080ea5bf63e27023a { get; set; }

        #endregion

    }
}
