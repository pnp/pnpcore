using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ApiMetadata class, write your custom code here
    /// </summary>
    [SharePointType("SP.ApiMetadata", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ApiMetadata : BaseDataModel<IApiMetadata>, IApiMetadata
    {
        #region Construction
        public ApiMetadata()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Id4a81de82eeb94d6080ea5bf63e27023a { get => GetValue<string>(); set => SetValue(value); }

        public IApiMetadata Current { get => GetModelValue<IApiMetadata>(); }


        public ITypeInformationCollection Types { get => GetModelCollectionValue<ITypeInformationCollection>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
