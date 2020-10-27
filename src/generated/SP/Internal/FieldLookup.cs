using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldLookup class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldLookup", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class FieldLookup : BaseDataModel<IFieldLookup>, IFieldLookup
    {
        #region Construction
        public FieldLookup()
        {
        }
        #endregion

        #region Properties
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

        #endregion

        #region Extension methods
        #endregion
    }
}
