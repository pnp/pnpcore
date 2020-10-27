using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a TimeZone object
    /// </summary>
    [ConcreteType(typeof(TimeZone))]
    public interface ITimeZone : IDataModel<ITimeZone>, IDataModelGet<ITimeZone>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Id { get; set; }

        #endregion

    }
}
