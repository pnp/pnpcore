namespace PnP.Core.QueryModel
{
    /// <summary>
    /// Enumeration of filtering criteria for queries
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1717:Only FlagsAttribute enums should have plural names", Justification = "<Pending>")]
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
        /// Corresponds to the &lt; operator
        /// </summary>
        LessThan,
        /// <summary>
        /// Corresponds to the &lt;= operator
        /// </summary>
        LessThanOrEqual,
        /// <summary>
        /// Corresponds to the ! operator
        /// </summary>
        Not
    }
}
