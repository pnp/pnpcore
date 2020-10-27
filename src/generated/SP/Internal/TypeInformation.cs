using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// TypeInformation class, write your custom code here
    /// </summary>
    [SharePointType("SP.TypeInformation", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class TypeInformation : BaseDataModel<ITypeInformation>, ITypeInformation
    {
        #region Construction
        public TypeInformation()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string BaseTypeFullName { get => GetValue<string>(); set => SetValue(value); }

        public string FullName { get => GetValue<string>(); set => SetValue(value); }

        public bool IsValueObject { get => GetValue<bool>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
