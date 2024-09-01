using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class AttachmentCollection : QueryableDataModelCollection<IAttachment>, IAttachmentCollection
    {
        public AttachmentCollection(PnPContext context, IDataModelParent parent, string memberName) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Add
        public async Task<IAttachment> AddAsync(string name, Stream content)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newAttachment = CreateNewAndAdd() as Attachment;
            newAttachment.FileName = name;

            newAttachment = await FileUpload(newAttachment, content).ConfigureAwait(false);

            return newAttachment;
        }

        public IAttachment Add(string name, Stream content)
        {
            return AddAsync(name, content).GetAwaiter().GetResult();
        }

        private static async Task<Attachment> FileUpload(Attachment newFile, Stream content)
        {
            var encodedServerFileName = WebUtility.UrlEncode(newFile.FileName.Replace("'", "''")).Replace("+", "%20");
            string fileCreateRequest = $"_api/web/lists/getbyid(guid'{{List.Id}}')/items({{Parent.Id}})/attachmentfiles/addusingpath(decodedUrl='{encodedServerFileName}')";
            var api = new ApiCall(fileCreateRequest, ApiType.SPORest)
            {
                Interactive = true,
                Content = new ByteArrayContent(ToByteArray(content))
            };

            await newFile.RequestAsync(api, HttpMethod.Post).ConfigureAwait(false);
            return newFile;
        }

        private static byte[] ToByteArray(Stream source)
        {
            using (var memoryStream = new MemoryStream())
            {
                source.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
        #endregion
    }
}
