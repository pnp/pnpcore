using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Feature class, write your custom code here
    /// </summary>
    [SharePointType("SP.Feature", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class Feature : BaseDataModel<IFeature>, IFeature
    {
        #region Construction
        public Feature()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        public Guid DefinitionId { get => GetValue<Guid>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #region New properties

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
