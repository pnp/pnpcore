using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a TypeInformation object
    /// </summary>
    [ConcreteType(typeof(TypeInformation))]
    public interface ITypeInformation : IDataModel<ITypeInformation>, IDataModelGet<ITypeInformation>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string BaseTypeFullName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsValueObject { get; set; }

        #endregion

    }
}
