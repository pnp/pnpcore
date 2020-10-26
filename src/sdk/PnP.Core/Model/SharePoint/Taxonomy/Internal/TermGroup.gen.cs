using System;

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
        public ITermSetCollection Sets { get => GetModelCollectionValue<ITermSetCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
    }
}
