namespace PnP.Core.QueryModel.OData.Enums
{
    /// <summary>
    /// Enumeration of filtering criteria for queries
    /// </summary>
    public enum FilteringCriteria
    {
        /// <summary>
        /// Corresponds to the = operator
        /// </summary>
        Equal,
        /// <summary>
        /// Corresponds to the != operator
        /// </summary>
        NotEqual,
        /// <summary>
        /// Corresponds to the > operator
        /// </summary>
        GreaterThan,
        /// <summary>
        /// Corresponds to the >= operator
        /// </summary>
        GreaterThanOrEqual,
        /// <summary>
        /// Corresponds to the < operator
        /// </summary>
        LessThan,
        /// <summary>
        /// Corresponds to the <= operator
        /// </summary>
        LessThanOrEqual,
        /// <summary>
        /// Corresponds to the ! operator
        /// </summary>
        Not
    }
}
