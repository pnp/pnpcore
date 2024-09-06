using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a location field value
    /// </summary>
    public sealed class FieldThumbnailValue: FieldValue, IFieldThumbnailValue
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FieldThumbnailValue() : base()
        {
        }

        internal override string SharePointRestType => "";

        internal override Guid CsomType => Guid.Empty;

        /// <summary>
        /// Filename identifiying this image
        /// </summary>
        public string FileName { get => GetValue<string>(); internal set => SetValue(value); }

        /// <summary>
        /// Server relative URL to access this image
        /// </summary>        
        public string ServerRelativeUrl { get => GetValue<string>(); internal set => SetValue(value); }

        /// <summary>
        /// Server URL
        /// </summary>
        public string ServerUrl { get => GetValue<string>(); internal set => SetValue(value); }

        /// <summary>
        /// Thumbnail renderer
        /// </summary>
        public object ThumbnailRenderer { get => GetValue<object>(); internal set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.Null || json.ValueKind == JsonValueKind.Undefined)
            {
                FileName = string.Empty;
            }
            else
            {
                return FromListDataAsStream(json.ToObject<Dictionary<string, string>>());
            }

            // Clear changes
            Commit();

            return this;
        }

        internal override IFieldValue FromListDataAsStream(Dictionary<string, string> properties)
        {
            if (properties.TryGetValue("fileName", out string valueFileName))
            {
                FileName = valueFileName;
            }
            if (properties.TryGetValue("serverRelativeUrl", out string valueServerRelativeUrl))
            {
                ServerRelativeUrl = valueServerRelativeUrl;
            }
            if (properties.TryGetValue("serverUrl", out string valueServerUrl))
            {
                ServerUrl = valueServerUrl;
            }
            if (properties.TryGetValue("thumbnailRenderer", out string valueThumbnailRenderer))
            {
                ThumbnailRenderer = valueThumbnailRenderer;
            }

            // Clear changes
            Commit();

            return this;
        }

        internal override object ToJson()
        {
            var updateMessage = new
            {

            };

            return updateMessage;
        }

        internal override object ToValidateUpdateItemJson()
        {
            return null;
        }

        internal override string ToCsomXml()
        {
            return "";
        }

        /// <summary>
        /// Uploads an image to a modern image field for the current list item
        /// </summary>
        /// <param name="item">List item to update</param>
        /// <param name="name">Image name</param>
        /// <param name="content">The content of the file.</param>
        /// <returns></returns>
        public async Task UploadImageAsync(IListItem item, string name, Stream content)
        {
            var encodedServerFileName = WebUtility.UrlEncode(name.Replace("'", "''").Replace("%20", " ")).Replace("+", "%20");
            string fileCreateRequest = $"_api/web/lists/getbyid(guid'{{List.Id}}')/Items(@a1)/AddThumbnailFieldData(imageName=@a2,fieldInternalName=@a3)?@a1={item.Id}&@a2=%27{encodedServerFileName}%27&@a3=%27{Field.InternalName}%27";
            var api = new ApiCall(fileCreateRequest, ApiType.SPORest)
            {
                Interactive = true,
                BinaryBody = ToByteArray(content),
            };
            
            var response = await (item as ListItem).RawRequestAsync(api, HttpMethod.Post).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(response.Json))
            {
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json);
                if (json.TryGetProperty("Name", out JsonElement nameValue) && nameValue.ValueKind != JsonValueKind.Null)
                {
                    FileName = nameValue.GetString();
                }
                if (json.TryGetProperty("ServerRelativeUrl", out JsonElement serverRelativeUrlValue) && serverRelativeUrlValue.ValueKind != JsonValueKind.Null)
                {
                    ServerRelativeUrl = serverRelativeUrlValue.GetString();
                }
            }
        }

        private static byte[] ToByteArray(Stream source)
        {
            using (var memoryStream = new MemoryStream())
            {
                source.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
