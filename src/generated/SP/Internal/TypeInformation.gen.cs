using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a TypeInformation object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class TypeInformation : BaseDataModel<ITypeInformation>, ITypeInformation
    {

        #region New properties

        public string BaseTypeFullName { get => GetValue<string>(); set => SetValue(value); }

        public string FullName { get => GetValue<string>(); set => SetValue(value); }

        public bool IsValueObject { get => GetValue<bool>(); set => SetValue(value); }

        #endregion

    }
}
