namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal sealed class ContentTypeInfo : BaseDataModel<IContentTypeInfo>, IContentTypeInfo
    {
        #region Construction
        public ContentTypeInfo()
        {
        }
        #endregion

        #region Properties

        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        
        #endregion

    }
}
