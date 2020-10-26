namespace PnP.Core.Model.SharePoint
{
    internal partial class TermRelation : BaseDataModel<ITermRelation>, ITermRelation
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public TermRelationType Relationship { get => GetValue<TermRelationType>(); set => SetValue(value); }

        public ITermSet Set { get => GetModelValue<ITermSet>(); set => SetModelValue(value); }

        public ITerm FromTerm { get => GetModelValue<ITerm>(); set => SetModelValue(value); }

        public ITerm ToTerm { get => GetModelValue<ITerm>(); set => SetModelValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
    }
}
