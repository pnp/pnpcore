using System.Collections.Generic;

namespace PnP.Core.QueryModel.Model
{
    /// <summary>
    /// Defines a group of filters
    /// </summary>
    public class FiltersGroup : ODataFilter
    {
        public FiltersGroup()
            : this(new List<ODataFilter>())
        {
        }

        public FiltersGroup(List<ODataFilter> filters)
        {
            Filters = filters;
        }

        public List<ODataFilter> Filters { get; private set; }
    }
}
