namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ViewFieldCollection class, write your custom code here
    /// </summary>
    [SharePointType("SP.ViewFieldCollection", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ViewFieldCollection : BaseDataModel<IViewFieldCollection>, IViewFieldCollection
    {
        #region New properties

        public string SchemaXml { get => GetValue<string>(); set => SetValue(value); }

        #endregion
    }
}
