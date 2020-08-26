namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal partial class TermSetProperty : BaseComplexType<ITermSetProperty>, ITermSetProperty
    {
        public string Key { get => GetValue<string>(); set => SetValue(value); }

        public string Value { get => GetValue<string>(); set => SetValue(value); }
    }
}
