using System;
using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    internal partial class Term : BaseDataModel<ITerm>, ITerm
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public List<ITermLocalizedLabel> Labels
        {
            get
            {
                if (!HasValue(nameof(Labels)))
                {
                    SetValue(new List<ITermLocalizedLabel>());
                }
                return GetValue<List<ITermLocalizedLabel>>();
            }
        }

        public List<ITermLocalizedDescription> Descriptions
        {
            get
            {
                if (!HasValue(nameof(Descriptions)))
                {
                    SetValue(new List<ITermLocalizedDescription>());
                }
                return GetValue<List<ITermLocalizedDescription>>();
            }
        }

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
                // Only set if there was no proper parent 
                if (GetParentByType(typeof(TermSet)) == null)
                {
                    InstantiateNavigationProperty();
                    SetValue(value);
                }
            }
        }


        [GraphProperty("children", Get = "termstore/sets/{Parent.GraphId}/terms/{GraphId}/children")]
        public ITermCollection Children
        {
            get
            {
                if (!HasValue(nameof(Children)))
                {
                    var children = new TermCollection(this.PnPContext, this);
                    SetValue(children);
                }
                return GetValue<ITermCollection>();
            }
        }

        public List<ITermProperty> Properties
        {
            get
            {
                if (!HasValue(nameof(Properties)))
                {
                    SetValue(new List<ITermProperty>());
                }
                return GetValue<List<ITermProperty>>();
            }
        }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
    }
}
