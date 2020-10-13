using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of View objects
    /// </summary>
    public interface IViewCollection : IQueryable<IView>, IDataModelCollection<IView>
    {
        /// <summary>
        /// Adds a new view
        /// </summary>
        /// <returns></returns>
        public Task<IView> AddAsync(ViewOptions viewOptions);
    }
}