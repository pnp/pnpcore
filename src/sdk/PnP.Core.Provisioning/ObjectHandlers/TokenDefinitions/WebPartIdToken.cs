using PnP.Core.Provisioning.Attributes;
using System;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{webpartid:[webpartname]}",
      Description = "Returns the id of a webpart that is being provisioned to a page through a template",
      Example = "{webpartid:mywebpart}",
      Returns = "66e2b037-f749-402d-90b2-afd643850c26")]
    internal class WebPartIdToken : TokenDefinition
    {
        private readonly string _webpartId = null;
        public WebPartIdToken(PnPContext context, string name, Guid webpartid)
            : base(context, $"{{webpartid:{Regex.Escape(name)}}}")
        {
            _webpartId = webpartid.ToString();
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _webpartId;
            }
            return Task.FromResult<string>(CacheValue);
        }
    }
}