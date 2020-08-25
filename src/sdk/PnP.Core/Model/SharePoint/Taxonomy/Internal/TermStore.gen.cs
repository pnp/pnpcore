using System;
using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    internal partial class TermStore : BaseDataModel<ITermStore>, ITermStore
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("defaultLanguageTag")]
        public string DefaultLanguage { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("languageTags")]
        public List<string> Languages { get => GetValue<List<string>>(); set => SetValue(value); }

        [GraphProperty("groups", Get = "termstore/groups")]
        public ITermGroupCollection Groups
        {
            get
            {
                if (!HasValue(nameof(Groups)))
                {
                    var termGroups = new TermGroupCollection(this.PnPContext, this, "Groups");
                    SetValue(termGroups);
                }
                return GetValue<ITermGroupCollection>();
            }
        }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
    }
}
