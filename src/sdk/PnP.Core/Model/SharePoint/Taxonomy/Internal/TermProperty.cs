/*
namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal partial class TermProperty : BaseDataModel<ITermProperty>, ITermProperty
    {
        #region Properties
        [GraphProperty("key")]
        public string KeyField { get => GetValue<string>(); set => SetValue(value); }

        public string Value { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(KeyField))]
        public override object Key { get => KeyField; set => KeyField = value.ToString(); }
        #endregion
    }
}
*/