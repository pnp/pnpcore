using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
     Token = "{fieldtitle:[internalname]}",
     Description = "Returns the title/displayname of a field given its internalname",
     Example = "{fieldtitle:LeaveEarly}",
     Returns = "Leaving Early")]
    internal class FieldTitleToken : TokenDefinition
    {
        private readonly string _value = null;
        public FieldTitleToken(PnPContext context, string InternalName, string Title)
            : base(context, $"{{fieldtitle:{InternalName}}}")
        {
            _value = Title;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _value;
            }
            return Task.FromResult<string>(CacheValue);
        }
    }
}