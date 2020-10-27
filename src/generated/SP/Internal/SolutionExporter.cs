using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// SolutionExporter class, write your custom code here
    /// </summary>
    [SharePointType("SP.SolutionExporter", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class SolutionExporter : BaseDataModel<ISolutionExporter>, ISolutionExporter
    {
        #region Construction
        public SolutionExporter()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Id4a81de82eeb94d6080ea5bf63e27023a { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
