using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal sealed class DocumentSet : BaseDataModel<IDocumentSet>, IDocumentSet
    {

        #region Properties

        public string ContentTypeId { get => GetValue<string>(); set => SetValue(value); }

        public IList<IContentTypeInfo> AllowedContentTypes { get => GetValue<IList<IContentTypeInfo>>(); set => SetValue(value); }

        public IList<IDocumentSetContent> DefaultContents { get => GetModelCollectionValue<IList<IDocumentSetContent>>(); set => SetValue(value); }

        public bool PropagateWelcomePageChanges { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShouldPrefixNameToFile { get => GetValue<bool>(); set => SetValue(value); }

        public string WelcomePageUrl { get => GetValue<string>(); set => SetValue(value); }

        public IList<IField> SharedColumns { get => GetValue<IList<IField>>(); set => SetValue(value); }

        public IList<IField> WelcomePageColumns { get => GetValue<IList<IField>>(); set => SetValue(value); }

        [KeyProperty(nameof(ContentTypeId))]
        public override object Key { get => ContentTypeId; set => ContentTypeId = value.ToString(); }

        #endregion

        #region Methods

        public async Task UpdateAsync(DocumentSetOptions options)
        {

        }

        public void Update(DocumentSetOptions options)
        {
            UpdateAsync(options).GetAwaiter().GetResult();
        }

        #endregion
    }
}
