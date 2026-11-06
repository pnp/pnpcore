using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
     Token = "{listid:[name]}",
     Description = "Returns a id of the list given its name",
     Example = "{listid:My List}",
     Returns = "f2cd6d5b-1391-480e-a3dc-7f7f96137382")]
    internal class ListIdToken : VolatileTokenDefinition
    {
        private string _listId;
        private readonly string _name;

        public ListIdToken(PnPContext context, string name, Guid listid)
            : base(context, $"{{listid:{Regex.Escape(name)}}}")
        {
            if (listid == Guid.Empty)
            {
                _name = name;
            }
            else
            {
                _listId = listid.ToString();
            }
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                if (_listId != null)
                {
                    CacheValue = _listId;
                }
                else
                {
                    IList list = await Context.Web.Lists.GetByTitleAsync(_name, l => l.Id).ConfigureAwait(false);
                    _listId = list.Id.ToString();
                    CacheValue = _listId;
                }
            }
            return CacheValue;
        }
    }
}
