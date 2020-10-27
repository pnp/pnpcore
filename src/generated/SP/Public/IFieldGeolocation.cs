using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldGeolocation object
    /// </summary>
    [ConcreteType(typeof(FieldGeolocation))]
    public interface IFieldGeolocation : IDataModel<IFieldGeolocation>, IDataModelGet<IFieldGeolocation>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        #endregion

    }
}
