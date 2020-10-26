using System;

namespace PnP.Core.Model.SharePoint
{
    internal partial class Term : BaseDataModel<ITerm>, ITerm
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public ITermLocalizedLabelCollection Labels { get => GetModelCollectionValue<ITermLocalizedLabelCollection>(); }

        public ITermLocalizedDescriptionCollection Descriptions { get => GetModelCollectionValue<ITermLocalizedDescriptionCollection>(); }

        public DateTimeOffset LastModifiedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        [GraphProperty("set", Expandable = true)]
        public ITermSet Set
        {
            get
            {
                // Since we quite often have the termset already as part of the term collection let's use that 
                var parentSet = GetParentByType(typeof(TermSet));
                if (parentSet != null)
                {
                    InstantiateNavigationProperty();
                    SetValue(parentSet as TermSet);
                    return GetValue<ITermSet>();
                }

                return GetModelValue<ITermSet>();

                //if (!NavigationPropertyInstantiated())
                //{
                //    var termSet = new TermSet
                //    {
                //        PnPContext = this.PnPContext,
                //        Parent = this,
                //    };
                //    SetValue(termSet);
                //    InstantiateNavigationProperty();
                //}
                //return GetValue<ITermSet>();
            }
            //set
            //{
            //    // Only set if there was no proper parent 
            //    if (GetParentByType(typeof(TermSet)) == null)
            //    {
            //        InstantiateNavigationProperty();
            //        SetValue(value);
            //    }
            //}
        }


        [GraphProperty("children", Get = "termstore/sets/{Parent.GraphId}/terms/{GraphId}/children", Beta = true)]
        public ITermCollection Terms { get => GetModelCollectionValue<ITermCollection>(); }        

        public ITermPropertyCollection Properties { get => GetModelCollectionValue<ITermPropertyCollection>(); }        

        [GraphProperty("relations", Get = "termstore/sets/{Parent.GraphId}/terms/{GraphId}/relations?$expand=fromTerm,set,toTerm", Beta = true)]
        public ITermRelationCollection Relations { get => GetModelCollectionValue<ITermRelationCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
    }
}
