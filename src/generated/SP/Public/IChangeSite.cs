using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ChangeSite object
    /// </summary>
    [ConcreteType(typeof(ChangeSite))]
    public interface IChangeSite : IDataModel<IChangeSite>, IDataModelGet<IChangeSite>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        #endregion

    }
}
