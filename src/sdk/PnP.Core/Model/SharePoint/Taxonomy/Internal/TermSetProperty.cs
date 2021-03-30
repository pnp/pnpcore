namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal partial class TermSetProperty : BaseDataModel<ITermSetProperty>, ITermSetProperty
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