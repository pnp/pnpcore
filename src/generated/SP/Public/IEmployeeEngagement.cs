using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a EmployeeEngagement object
    /// </summary>
    [ConcreteType(typeof(EmployeeEngagement))]
    public interface IEmployeeEngagement : IDataModel<IEmployeeEngagement>, IDataModelGet<IEmployeeEngagement>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Id4a81de82eeb94d6080ea5bf63e27023a { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IAppConfiguration AppConfiguration { get; }

        #endregion

    }
}
