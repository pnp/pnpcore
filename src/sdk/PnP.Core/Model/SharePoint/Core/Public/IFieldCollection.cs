using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Field objects of SharePoint Online
    /// </summary>
    public interface IFieldCollection : IQueryable<IField>, IDataModelCollection<IField>
    {
        /// For FieldType
        /// https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-server/ee540543%28v%3doffice.15%29

        #region Extension Methods
        IField Add(string title, FieldType fieldType, BaseFieldAddOptions options = null);

        // TODO Add /// Summary

        IField Add(Batch batch, string title, FieldType fieldType, BaseFieldAddOptions options = null);

       // TODO Add /// Summary
        Task<IField> AddAsync(string title, FieldType fieldType, BaseFieldAddOptions options = null);

        // TODO Add /// Summary

        Task<IField> AddAsync(string title, string internalName, FieldType fieldType, BaseFieldAddOptions options = null);
        #endregion
    }
}
