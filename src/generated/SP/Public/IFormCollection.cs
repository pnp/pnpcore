using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Form objects
    /// </summary>
    public interface IFormCollection : IQueryable<IForm>, IDataModelCollection<IForm>
    {
    }
}