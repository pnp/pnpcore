using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
     Token = "{fieldid:[internalname]}",
     Description = "Returns the ID of a field given its internalname",
     Example = "{fieldid:LeaveEarly}",
     Returns = "20d5ad60-8662-4d06-92bb-3a434766f344")]
    internal class FieldIdToken : TokenDefinition
    {
        private readonly string _value = null;

        public FieldIdToken(PnPContext context, string InternalName, System.Guid fieldId)
            : base(context, $"{{fieldid:{InternalName}}}")
        {
            _value = fieldId.ToString();
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