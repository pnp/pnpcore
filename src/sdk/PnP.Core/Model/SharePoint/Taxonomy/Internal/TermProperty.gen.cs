namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal partial class TermProperty : BaseDataModel<ITermProperty>, ITermProperty
    {
        [GraphProperty("key")]
        public string KeyField { get => GetValue<string>(); set => SetValue(value); }

        public string Value { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(KeyField))]
        public override object Key { get => this.KeyField; set => this.KeyField = value.ToString(); }
    }
}
