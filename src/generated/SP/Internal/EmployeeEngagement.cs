using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// EmployeeEngagement class, write your custom code here
    /// </summary>
    [SharePointType("SP.EmployeeEngagement", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class EmployeeEngagement : BaseDataModel<IEmployeeEngagement>, IEmployeeEngagement
    {
        #region Construction
        public EmployeeEngagement()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Id4a81de82eeb94d6080ea5bf63e27023a { get => GetValue<string>(); set => SetValue(value); }

        public IAppConfiguration AppConfiguration { get => GetModelValue<IAppConfiguration>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
