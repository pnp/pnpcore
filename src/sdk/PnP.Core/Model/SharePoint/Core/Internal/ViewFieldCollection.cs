namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ViewFieldCollection", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal sealed class ViewFieldCollection : BaseDataModel<IViewFieldCollection>, IViewFieldCollection
    {
        public System.Collections.Generic.List<string> Items { get => GetValue<System.Collections.Generic.List<string>>(); set => SetValue(value); }

        public string SchemaXml { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(SchemaXml))]
        public override object Key { get => SchemaXml; set => SchemaXml = value.ToString(); }
    }
}
