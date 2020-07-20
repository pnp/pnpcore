using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Field objects of SharePoint Online
    /// </summary>
    public interface IFieldLinkCollection : IQueryable<IFieldLink>, IDataModelCollection<IFieldLink>
    {
        Task<IFieldLink> AddBatchAsync(string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true);

        Task<IFieldLink> AddBatchAsync(Batch batch, string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true);

        Task<IFieldLink> AddAsync(string fieldInternalName, string displayName = null, bool hidden = false, bool required = false, bool readOnly = false, bool showInDisplayForm = true);
    }
}
