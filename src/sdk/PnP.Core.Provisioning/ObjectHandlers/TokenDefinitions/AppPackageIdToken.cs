using PnP.Core.Provisioning.Attributes;
using System;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
        Token = "{apppackageid:[packagename]}",
        Description = "Returns the ID of an app package given its name",
        Example = "{apppackageid:MyPackageName}",
        Returns = "55898e77-a7bf-4799-8034-506db5521b98")]
    internal class AppPackageIdToken : TokenDefinition
    {
        private readonly string _appPackageId = null;

        public AppPackageIdToken(PnPContext context, string name, Guid appPackageId)
            : base(context, $"{{apppackageid:{Regex.Escape(name)}}}")
        {
            _appPackageId = appPackageId.ToString();
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _appPackageId;
            }
            return Task.FromResult<string>(CacheValue);
        }
    }
}