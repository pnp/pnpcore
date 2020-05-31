namespace PnP.Core.Model.SharePoint
{
    internal partial class ContentTypeId : BaseComplexType<IContentTypeId>, IContentTypeId
    {
        public string StringValue { get => GetValue<string>(); set => SetValue(value); }
    }
}
