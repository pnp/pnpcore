namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents the relationship between terms in a term store.Currently two types of relationships are supported: pin and reuse.
    ///
    /// In a pin relationship, a term can be pinned under a different term in a different term set.In a pinned relationship, new children to the term can only be added in the term set in which the term was created.Any change in the hierarchy under the term is reflected across the sets in which the term was pinned.
    ///
    /// The reuse relationship is similar to the pinned relationship except that changes to the reused term can be made from any hierarchy in which the term is reused.Also, a change in hierarchy made to the reused term does not get reflected in the other term sets in which the term is reused.
    /// </summary>
    [ConcreteType(typeof(TermRelation))]
    public interface ITermRelation : IDataModel<ITermRelation>, IDataModelGet<ITermRelation>
    {
        /// <summary>
        /// The Unique ID of the term relation.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The type of relation. Possible values are: pin, reuse.
        /// </summary>
        public TermRelationType Relationship { get; set; }

        /// <summary>
        /// The set in which the relation is relevant.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        public ITermSet Set { get; }

        /// <summary>
        /// The from term of the relation. The term from which the relationship is defined. A null value would indicate the relation is directly with the set.
        /// </summary>
        public ITerm FromTerm { get; }

        /// <summary>
        /// The to term of the relation. The term to which the realtionship is defined.
        /// </summary>
        public ITerm ToTerm { get; }
    }
}
