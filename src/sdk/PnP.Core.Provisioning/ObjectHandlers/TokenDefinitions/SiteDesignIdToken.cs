using PnP.Core.Provisioning.Attributes;
using System;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
        Token = "{sitedesignid:[designtitle]}",
        Description = "Returns the id of the given site design",
        Example = "{sitedesignid:My Site Design}",
        Returns = "9188a794-cfcf-48b6-9ac5-df2048e8aa5d")]
    internal class SiteDesignIdToken : TokenDefinition
    {
        private Guid _designId;
        public SiteDesignIdToken(PnPContext context, string designTitle, Guid designId)
            : base(context, $"{{sitedesignid:{Regex.Escape(designTitle)}}}")
        {
            _designId = designId;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                CacheValue = _designId.ToString();
            }
            return Task.FromResult<string>(CacheValue);
        }
    }
}