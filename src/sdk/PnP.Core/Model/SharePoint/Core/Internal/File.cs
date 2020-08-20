using Microsoft.Extensions.Logging;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// File class, write your custom code here
    /// </summary>
    [SharePointType("SP.File", Target = typeof(Folder), Uri = "_api/Web/getFileById('{Id}')", Get = "_api/Web/getFolderById('{Parent.Id}')/Files", LinqGet = "_api/Web/getFolderById('{Parent.Id}')/Files")]
    // TODO To implement when a token can be used to identify the parent list
    //[GraphType(Get = "sites/{hostname}:{serverrelativepath}/lists/{ParentList.Id}/items/{Id}")]
    internal partial class File
    {
        internal const string AddFileContentAdditionalInformationKey = "Content";
        internal const string AddFileOverwriteAdditionalInformationKey = "Overwrite";

        public File()
        {
            MappingHandler = (FromJson input) =>
            {
                // implement custom mapping logic
                switch (input.TargetType.Name)
                {
                    case nameof(CustomizedPageStatus): return JsonMappingHelper.ToEnum<CustomizedPageStatus>(input.JsonElement);
                    case nameof(PageRenderType): return JsonMappingHelper.ToEnum<ListPageRenderType>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };

            // TODO Continue implementation when no batch call will be supported
            //AddApiCallHandler = (additionalInformation) =>
            //{
            //    Stream content = (Stream)(additionalInformation.ContainsKey(AddFileContentAdditionalInformationKey)
            //        ? additionalInformation[AddFileContentAdditionalInformationKey]
            //        : null);
            //    if (content == null)
            //        throw new ClientException(ErrorType.InvalidParameters, "Adding new file without content is not possible, please provide valid content for the file");

            //    bool overwrite = (bool)(additionalInformation.ContainsKey(AddFileOverwriteAdditionalInformationKey)
            //        ? additionalInformation[AddFileOverwriteAdditionalInformationKey]
            //        : false);

            //};
        }
    }
}
