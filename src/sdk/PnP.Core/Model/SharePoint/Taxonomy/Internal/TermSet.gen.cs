using System;

namespace PnP.Core.Model.SharePoint
{

    internal partial class TermSet : BaseDataModel<ITermSet>, ITermSet
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public ITermSetLocalizedNameCollection LocalizedNames { get => GetModelCollectionValue<ITermSetLocalizedNameCollection>(); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        [GraphProperty("children", Get = "termstore/sets/{GraphId}/children", Beta = true)]
        public ITermCollection Terms { get => GetModelCollectionValue<ITermCollection>(); }

        [GraphProperty("parentGroup", Expandable = true)]
        public ITermGroup Group 
        {
            get
            {
                // Since we quite often have the group already as part of the termset collection let's use that 
                if (Parent != null && Parent.Parent != null)
                {
                    InstantiateNavigationProperty();
                    SetValue(Parent.Parent as TermGroup);
                    return GetValue<ITermGroup>();
                }

                // Seems there was no group available, so process the loaded group and assign it
                return GetModelValue<ITermGroup>();
                //if (!NavigationPropertyInstantiated())
                //{
                //    var termGroup = new TermGroup
                //    {
                //        PnPContext = this.PnPContext,
                //        Parent = this,
                //    };
                //    SetValue(termGroup);
                //    InstantiateNavigationProperty();
                //}
                //return GetValue<ITermGroup>();
            }
            //set
            //{
            //    // Only set if there was no proper parent 
            //    if (Parent == null || Parent.Parent != null)
            //    {
            //        InstantiateNavigationProperty();
            //        SetValue(value);
            //    }
            //}
        }

        public ITermSetPropertyCollection Properties { get => GetModelCollectionValue<ITermSetPropertyCollection>(); }

        [GraphProperty("relations", Get = "termstore/sets/{GraphId}/relations?$expand=fromTerm,set,toTerm", Beta = true)]
        public ITermRelationCollection Relations { get => GetModelCollectionValue<ITermRelationCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
    }
}
