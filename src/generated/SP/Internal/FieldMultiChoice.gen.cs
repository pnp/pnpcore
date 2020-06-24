using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a FieldMultiChoice object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class FieldMultiChoice : BaseDataModel<IFieldMultiChoice>, IFieldMultiChoice
    {

        #region New properties

        public bool FillInChoice { get => GetValue<bool>(); set => SetValue(value); }

        public string Mappings { get => GetValue<string>(); set => SetValue(value); }

        #endregion

    }
}
