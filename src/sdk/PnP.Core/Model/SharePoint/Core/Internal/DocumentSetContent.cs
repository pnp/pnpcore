namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal sealed class DocumentSetContent : BaseDataModel<IDocumentSetContent>, IDocumentSetContent
    {
        #region Construction
        public DocumentSetContent()
        {
        }
        #endregion

        #region Properties

        public IContentTypeInfo ContentType { get => GetModelValue<IContentTypeInfo>(); set => SetModelValue(value); }

        public string FileName { get => GetValue<string>(); set => SetValue(value); }

        public string FolderName { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(FileName))]
        public override object Key { get => FileName; set => FileName = value.ToString(); }
       
        #endregion

    }
}
