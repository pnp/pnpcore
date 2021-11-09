using PnP.Core.Services;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Attachment class, write your custom code here
    /// </summary>
    [SharePointType("SP.Attachment", Uri = "_api/web/lists/getbyid(guid'{List.Id}')/items({Parent.Id})/attachmentfiles/getbyfilename('{Id}')", LinqGet = "_api/web/lists/getbyid(guid'{List.Id}')/items({Parent.Id})/attachmentfiles")]
    internal sealed class Attachment : BaseDataModel<IAttachment>, IAttachment
    {
        #region Construction
        public Attachment()
        {
        }
        #endregion

        #region Properties
        public string FileName { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("FileNameAsPath", JsonPath = "DecodedUrl")]
        public string FileNameAsPath { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("ServerRelativePath", JsonPath = "DecodedUrl")]
        public string ServerRelativePath { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(FileName))]
        public override object Key { get => FileName; set => FileName = value.ToString(); }

        [SharePointProperty("*")]
        public object All { get => null; }
        #endregion

        #region GetContent
        public async Task<Stream> GetContentAsync(bool streamContent = false)
        {
            var file = await PnPContext.Web.GetFileByServerRelativeUrlAsync(ServerRelativeUrl).ConfigureAwait(false);
            return file.GetContent(streamContent);
        }

        public Stream GetContent(bool streamContent = false)
        {
            return GetContentAsync(streamContent).GetAwaiter().GetResult();
        }

        public async Task<byte[]> GetContentBytesAsync()
        {
            using (var contentStream = await GetContentAsync().ConfigureAwait(false))
            {
                return ((MemoryStream)contentStream).ToArray();
            }
        }

        public byte[] GetContentBytes()
        {
            return GetContentBytesAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region Recycle

        public async Task RecycleAsync()
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);

            var apiCall = new ApiCall($"{entity.SharePointUri}/recycleobject", ApiType.SPORest);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void Recycle()
        {
            RecycleAsync().GetAwaiter().GetResult();
        }
        #endregion

    }
}
