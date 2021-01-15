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
    [SharePointType("SP.FileVersion")]
    internal partial class FileVersion : BaseDataModel<IFileVersion>, IFileVersion
    {
        #region Properties
        public string CheckInComment { get => GetValue<string>(); set => SetValue(value); }

        public DateTime Created { get => GetValue<DateTime>(); set => SetValue(value); }

        [SharePointProperty("ID")]
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool IsCurrentVersion { get => GetValue<bool>(); set => SetValue(value); }

        public int Size { get => GetValue<int>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public string VersionLabel { get => GetValue<string>(); set => SetValue(value); }

        public ISharePointUser CreatedBy { get => GetModelValue<ISharePointUser>(); }

        [KeyProperty(nameof(Created))]
        public override object Key { get => Created; set => Created = DateTime.Parse(value.ToString()); }
        #endregion

        #region Methods

        #region GetContent
        public async Task<Stream> GetContentAsync(bool streamContent = false)
        {
            await EnsurePropertiesAsync(p => p.Url).ConfigureAwait(false);

            string downloadUrl = $"{PnPContext.Uri}/_layouts/15/download.aspx?SourceUrl={Url}";

            var apiCall = new ApiCall(downloadUrl, ApiType.SPORest)
            {
                Interactive = true,
                ExpectBinaryResponse = true,
                StreamResponse = streamContent
            };

            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
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
