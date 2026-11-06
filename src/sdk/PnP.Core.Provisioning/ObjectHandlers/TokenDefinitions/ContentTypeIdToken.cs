using PnP.Core.Provisioning.Attributes;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{contenttypeid:[contenttypename]}",
      Description = "Returns the ID of the specified content type",
      Example = "{contenttypeid:My Content Type}",
      Returns = "0x0102004F51EFDEA49C49668EF9C6744C8CF87D")]
    internal class ContentTypeIdToken : TokenDefinition
    {
        private readonly string _contentTypeId = null;
        public ContentTypeIdToken(PnPContext context, string name, string contenttypeid)
            : base(context, $"{{contenttypeid:{Regex.Escape(name)}}}")
        {
            _contentTypeId = contenttypeid;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _contentTypeId;
            }
            return Task.FromResult<string>(CacheValue);
        }
    }
}