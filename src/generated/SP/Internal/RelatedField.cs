using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// RelatedField class, write your custom code here
    /// </summary>
    [SharePointType("SP.RelatedField", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class RelatedField : BaseDataModel<IRelatedField>, IRelatedField
    {
        #region Construction
        public RelatedField()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public Guid FieldId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }

        public int RelationshipDeleteBehavior { get => GetValue<int>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        public IList LookupList { get => GetModelValue<IList>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
