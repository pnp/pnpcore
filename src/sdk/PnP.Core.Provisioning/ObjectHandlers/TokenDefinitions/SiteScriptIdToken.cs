using PnP.Core.Provisioning.Attributes;
using System;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{sitescriptid:[scripttitle]}",
      Description = "Returns the id of the given site script",
      Example = "{sitescriptid:My Site Script}",
      Returns = "9188a794-cfcf-48b6-9ac5-df2048e8aa5d")]
    internal class SiteScriptIdToken : TokenDefinition
    {
        private Guid _scriptId;
        public SiteScriptIdToken(PnPContext context, string scriptTitle, Guid scriptId)
            : base(context, $"{{sitescriptid:{Regex.Escape(scriptTitle)}}}")
        {
            _scriptId = scriptId;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                CacheValue = _scriptId.ToString();
            }
            return Task.FromResult<string>(CacheValue);
        }
    }
}