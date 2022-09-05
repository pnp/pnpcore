namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ContentTypeId")]
    internal sealed class ContentTypeId : BaseDataModel<IContentTypeId>, IContentTypeId
    {
        #region Construction
        public ContentTypeId()
        {
        }
        #endregion

        #region Properties

        public string StringValue { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(StringValue))]
        public override object Key { get => StringValue; set => StringValue = value.ToString(); }
        #endregion

    }
}
