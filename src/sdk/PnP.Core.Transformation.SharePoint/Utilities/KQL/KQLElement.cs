namespace PnP.Core.Transformation.SharePoint.KQL
{
    /// <summary>
    /// Element in KQL query
    /// </summary>
    public class KQLElement
    {
        /// <summary>
        /// Filter attribute name
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Value of the filter
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Type of filter
        /// </summary>
        public KQLFilterType Type { get; set; }

        /// <summary>
        /// Filter operator
        /// </summary>
        public KQLPropertyOperator Operator { get; set; }

        /// <summary>
        /// Filter group
        /// </summary>
        public int Group { get; set; }
    }
}
