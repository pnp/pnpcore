using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a FieldLookup object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class FieldLookup : BaseDataModel<IFieldLookup>, IFieldLookup
    {

        #region New properties

        public bool AllowMultipleValues { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsDependentLookup { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsRelationship { get => GetValue<bool>(); set => SetValue(value); }

        public string LookupField { get => GetValue<string>(); set => SetValue(value); }

        public string LookupList { get => GetValue<string>(); set => SetValue(value); }

        public Guid LookupWebId { get => GetValue<Guid>(); set => SetValue(value); }

        public string PrimaryFieldId { get => GetValue<string>(); set => SetValue(value); }

        public int RelationshipDeleteBehavior { get => GetValue<int>(); set => SetValue(value); }

        public bool UnlimitedLengthInDocumentLibrary { get => GetValue<bool>(); set => SetValue(value); }

        #endregion

    }
}
