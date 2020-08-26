namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal partial class TermProperty : BaseComplexType<ITermProperty>, ITermProperty
    {
        public string Key { get => GetValue<string>(); set => SetValue(value); }

        public string Value { get => GetValue<string>(); set => SetValue(value); }
    }
}
