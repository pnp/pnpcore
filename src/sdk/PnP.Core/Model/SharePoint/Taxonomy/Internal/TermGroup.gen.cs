using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.SharePoint
{
    internal partial class TermGroup : BaseDataModel<ITermGroup>, ITermGroup
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("displayName")]
        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }
        
        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }
        
        public TermGroupScope Scope { get => GetValue<TermGroupScope>(); set => SetValue(value); }

        [GraphProperty("sets", Get = "termstore/groups/{GraphId}/sets")]
        public ITermSetCollection Sets
        {
            get
            {
                if (!HasValue(nameof(Sets)))
                {
                    var termSets = new TermSetCollection(this.PnPContext, this, "Sets");
                    SetValue(termSets);
                }
                return GetValue<ITermSetCollection>();
            }
        }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
    }
}
