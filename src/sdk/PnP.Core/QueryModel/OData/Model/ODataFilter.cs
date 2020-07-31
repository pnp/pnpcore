using PnP.Core.QueryModel.OData.Enums;

namespace PnP.Core.QueryModel.OData.Model
{
    /// <summary>
    /// Interface to define the basic functionalities of a filtering item (either a single item or a group of items)
    /// </summary>
    public abstract class ODataFilter
    {
        /// <summary>
        /// The concatenation operator between the current filter item and the next one in the chain, within the current filtering group. Default: AND.
        /// </summary>
        public FilteringConcatOperator ConcatOperator { get; set; } = FilteringConcatOperator.AND;
    }
}
