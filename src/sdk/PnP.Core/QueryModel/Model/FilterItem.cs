using PnP.Core.QueryModel.Enums;

namespace PnP.Core.QueryModel.Model
{
    /// <summary>
    /// Defines a filtering criteria item
    /// </summary>
    public class FilterItem : ODataFilter
    {
        /// <summary>
        /// The name of the field for the filtering criteria
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// The filtering criteria. Default: Equal.
        /// </summary>
        public FilteringCriteria Criteria { get; set; } = FilteringCriteria.Equal;

        /// <summary>
        /// The actual value for the filtering criteria
        /// </summary>
        public object Value { get; set; }
    }
}
