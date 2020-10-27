using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldGuid object
    /// </summary>
    [ConcreteType(typeof(FieldGuid))]
    public interface IFieldGuid : IDataModel<IFieldGuid>, IDataModelGet<IFieldGuid>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        #endregion

    }
}
