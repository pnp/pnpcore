using PnP.Core.Provisioning.Attributes;
using System;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
     Token = "{viewid:[listname],[viewname]}",
     Description = "Returns a id of the view given its name for a given list",
     Example = "{viewid:My List,My View}",
     Returns = "f2cd6d5b-1391-480e-a3dc-7f7f96137382")]
    internal class ListViewIdToken : TokenDefinition
    {
        private readonly string _viewId = null;

        public ListViewIdToken(PnPContext context, string listTitle, string viewTitle, Guid viewId)
            : base(context, $"{{viewid:{Regex.Escape(listTitle)},{Regex.Escape(viewTitle)}}}")
        {
            _viewId = viewId.ToString();
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _viewId;
            }
            return Task.FromResult<string>(CacheValue);
        }
    }
}