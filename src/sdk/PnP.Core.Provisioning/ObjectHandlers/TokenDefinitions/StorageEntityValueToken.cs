using PnP.Core.Provisioning.Attributes;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{storageentityvalue:[key]}",
      Description = "Returns the value of a storage entity provided by the key",
      Example = "{storageentityvalue:MyKey}",
      Returns = "My Value")]
    internal class StorageEntityValueToken : TokenDefinition
    {
        private readonly string _value;
        public StorageEntityValueToken(PnPContext context, string key, string value)
            : base(context, $"{{storageentityvalue:{Regex.Escape(key)}}}")
        {
            _value = value;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                CacheValue = _value;
            }
            return Task.FromResult<string>(CacheValue);
        }
    }
}