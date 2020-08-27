using System.Collections.Generic;

namespace PnP.Core.QueryModel
{
    /// <summary>
    /// Defines a group of filters
    /// </summary>
    public class FiltersGroup : ODataFilter
    {
        /// <summary>
        /// Constructs a group of filters
        /// </summary>
        public FiltersGroup()
            : this(new List<ODataFilter>())
        {
        }

        /// <summary>
        /// Constructs a group of filters based upon the provide list of filters
        /// </summary>
        /// <param name="filters">List of <see cref="ODataFilter"/> filters</param>
        public FiltersGroup(List<ODataFilter> filters)
        {
            Filters = filters;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<ODataFilter> Filters { get; private set; }
    }
}
