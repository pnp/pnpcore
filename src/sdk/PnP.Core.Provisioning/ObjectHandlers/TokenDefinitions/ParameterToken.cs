using PnP.Core.Provisioning.Attributes;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
        Token = "{parameter:[parametername]}",
        Description = "Returns the value of a parameter defined in the template",
        Example = "{parameter:MyParameter}",
        Returns = "the value of the parameter")]
    internal class ParameterToken : TokenDefinition
    {
        private readonly string _value = null;
        public ParameterToken(PnPContext context, string name, string value)
            : base(context, $"{{parameter:{Regex.Escape(name)}}}", $"{{\\${Regex.Escape(name)}}}")
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