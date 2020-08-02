using PnP.Core.QueryModel.Enums;

namespace PnP.Core.QueryModel.Model
{
    /// <summary>
    /// Defines a single sorting item
    /// </summary>
    public class OrderByItem
    {
        /// <summary>
        /// The name of the field to sort by
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// The direction (Ascending/Descending) for the sorting criteria. Default: Ascending.
        /// </summary>
        public OrderByDirection Direction { get; set; } = OrderByDirection.Asc;
    }
}
