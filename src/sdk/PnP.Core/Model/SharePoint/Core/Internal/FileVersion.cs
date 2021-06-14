using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FileVersion class, write your custom code here
    /// </summary>
    [SharePointType("SP.FileVersion", Target = typeof(File), Uri = "_api/Web/getFileById('{Parent.Id}')/versions/getById({Id})", LinqGet = "_api/Web/getFileById('{Parent.Id}')/versions")]
    [SharePointType("SP.FileVersion", Target = typeof(ListItemVersion), Uri = "_api/web/lists/getbyid(guid'{List.Id}')/items({Item.Id})/versions/getbyid({Parent.Id})/fileversion")]
    internal partial class FileVersion : BaseDataModel<IFileVersion>, IFileVersion
    {
        #region Properties
        public string CheckInComment { get => GetValue<string>(); set => SetValue(value); }

        public DateTime Created { get => GetValue<DateTime>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool IsCurrentVersion { get => GetValue<bool>(); set => SetValue(value); }

        public int Size { get => GetValue<int>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public string VersionLabel { get => GetValue<string>(); set => SetValue(value); }

        public ISharePointUser CreatedBy { get => GetModelValue<ISharePointUser>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }
        #endregion

        #region Methods

        #region GetContent
        public async Task<Stream> GetContentAsync(bool streamContent = false)
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);
            string streamUrl = $"{entity.SharePointUri}/OpenBinaryStream";

            var apiCall = new ApiCall(streamUrl, ApiType.SPORest)
            {
                Interactive = true,
                ExpectBinaryResponse = true,
                StreamResponse = streamContent
            };

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            return response.BinaryContent;
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

        #endregion
    }
}
