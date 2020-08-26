namespace PnP.Core.Model.SharePoint
{
    internal partial class TermRelation : BaseDataModel<ITermRelation>, ITermRelation
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public TermRelationType Relationship { get => GetValue<TermRelationType>(); set => SetValue(value); }

        public ITermSet Set
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var termSet = new TermSet
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(termSet);
                    InstantiateNavigationProperty();
                }
                return GetValue<ITermSet>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        public ITerm FromTerm
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var term = new Term
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(term);
                    InstantiateNavigationProperty();
                }
                return GetValue<ITerm>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        public ITerm ToTerm
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var term = new Term
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(term);
                    InstantiateNavigationProperty();
                }
                return GetValue<ITerm>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }
    }
}
