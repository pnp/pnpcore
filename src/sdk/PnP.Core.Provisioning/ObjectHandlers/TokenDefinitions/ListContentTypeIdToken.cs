using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
     Token = "{listContentTypeId:[listname],[contentTypeName]}",
     Description = "Returns an id of the content type given its name for a given list",
     Example = "{listContentTypeId:My List,Document}",
     Returns = "0x010100F0D7B2FF0128AD459168DFA77A2A1BD0")]
    [TokenDefinitionDescription(
     Token = "{listContentTypeId:[listname],[contentTypeId]}",
     Description = "Returns an id of the content type given its direct parent id for a given list",
     Example = "{listContentTypeId:My List,0x0101}",
     Returns = "0x010100F0D7B2FF0128AD459168DFA77A2A1BD0")]
    internal class ListContentTypeIdToken : TokenDefinition
    {
        private readonly string _contentTypeId;
        private const string TokenPrefix = "listcontenttypeid";

        public ListContentTypeIdToken(PnPContext context, string listTitle, IContentType contentType)
            : base(context,
                  CreateToken(listTitle, contentType.StringId),
                  CreateToken(listTitle, contentType.Name))
        {
            _contentTypeId = contentType.StringId;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _contentTypeId;
            }
            return Task.FromResult(CacheValue);
        }

        /// <summary>
        /// Creates a token for the specified list title and list content type name.
        /// </summary>
        /// <param name="listTitle">Title of the list</param>
        /// <param name="contentTypeName">Name of the list content type</param>
        /// <returns>A token such as <c>{listcontenttypeid:My List,Document}</c></returns>
        public static string CreateToken(string listTitle, string contentTypeName)
        {
            return $"{{{TokenPrefix}:{Regex.Escape(listTitle)},{Regex.Escape(contentTypeName)}}}";
        }

        /// <summary>
        /// Creates a token for the specified list title and list content type id, keyed on the
        /// content type's <em>parent</em> id.
        /// </summary>
        /// <param name="listTitle">Title of the list</param>
        /// <param name="contentTypeId">Full string id of the list content type</param>
        /// <returns>A token such as <c>{listcontenttypeid:My List,0x0101}</c></returns>
        public static string CreateTokenFromId(string listTitle, string contentTypeId)
        {
            return $"{{{TokenPrefix}:{Regex.Escape(listTitle)},{Regex.Escape(GetParentIdValue(contentTypeId))}}}";
        }

        /// <summary>
        /// Derives the parent content type id from a content type id.
        /// </summary>
        internal static string GetParentIdValue(string contentTypeId)
        {
            if (string.IsNullOrEmpty(contentTypeId))
            {
                return contentTypeId;
            }

            int lastDeep = contentTypeId.LastIndexOf("00", System.StringComparison.Ordinal);
            if (lastDeep > 0 && contentTypeId.Length - lastDeep == 34)
            {
                return contentTypeId.Substring(0, lastDeep);
            }

            return contentTypeId.Length > 2 ? contentTypeId.Substring(0, contentTypeId.Length - 2) : contentTypeId;
        }
    }
}
