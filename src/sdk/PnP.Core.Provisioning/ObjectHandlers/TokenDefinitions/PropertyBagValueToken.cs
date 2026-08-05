using PnP.Core.Provisioning.Attributes;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
        Token = "{propertybagvalue:[key]}",
        Description = "Returns the value of a propertybag value",
        Example = "{propertybagvalue:MyKey}",
        Returns = "the value of the propertybag value defined by the key")]
    internal class PropertyBagValueToken : TokenDefinition
    {
        private readonly string _value = null;
        public PropertyBagValueToken(PnPContext context, string name, string value)
            : base(context, $"{{propertybagvalue:{Regex.Escape(name)}}}")
        {
            _value = value;
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