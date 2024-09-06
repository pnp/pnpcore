using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class FileVersionCollection : QueryableDataModelCollection<IFileVersion>, IFileVersionCollection
    {
        public FileVersionCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region GetById methods

        public IFileVersion GetById(int id, params Expression<Func<IFileVersion, object>>[] selectors)
        {
            return GetByIdAsync(id, selectors).GetAwaiter().GetResult();
        }

        public async Task<IFileVersion> GetByIdAsync(int id, params Expression<Func<IFileVersion, object>>[] selectors)
        {
            return await this.QueryProperties(selectors).FirstOrDefaultAsync(l => l.Id == id).ConfigureAwait(false);
        }

        #endregion

        #region DeleteAll methods

        public void DeleteAll()
        {
            DeleteAllAsync().GetAwaiter().GetResult();
        }

        public async Task DeleteAllAsync()
        {
            // Get a reference to the item to Delete
            DeleteAllImplementation(out ApiCall apiCall);

            var response = await (PnPContext.Web as BaseDataModel<IWeb>).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void DeleteAllBatch()
        {
            DeleteAllBatchAsync().GetAwaiter().GetResult();
        }

        public async Task DeleteAllBatchAsync()
        {
            await DeleteAllBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void DeleteAllBatch(Batch batch)
        {
            DeleteAllBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task DeleteAllBatchAsync(Batch batch)
        {
            // Get a reference to the item to Delete
            DeleteAllImplementation(out ApiCall apiCall);

            await (PnPContext.Web as BaseDataModel<IWeb>).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private void DeleteAllImplementation(out ApiCall apiCall)
        {
            // Delete the item
            apiCall = new ApiCall($"_api/Web/getFileById('{(Parent as File).UniqueId}')/versions/DeleteAll()", ApiType.SPORest);
            
            // Remove the items from the model
            items.Clear();
        }

        #endregion

        #region DeleteById methods

        public new void DeleteById(int id)
        {
            DeleteByIdAsync(id).GetAwaiter().GetResult();
        }

        public new async Task DeleteByIdAsync(int id)
        {
            // Get a reference to the item to Delete
            DeleteByIdImplementation(id, out object concreteEntity, out ApiCall apiCall);

            var response = await (concreteEntity as BaseDataModel<IFileVersion>).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public new void DeleteByIdBatch(int id)
        {
            DeleteByIdBatchAsync(id).GetAwaiter().GetResult();
        }

        public new async Task DeleteByIdBatchAsync(int id)
        {
            await DeleteByIdBatchAsync(PnPContext.CurrentBatch, id).ConfigureAwait(false);
        }

        public new void DeleteByIdBatch(Batch batch, int id)
        {
            DeleteByIdBatchAsync(batch, id).GetAwaiter().GetResult();
        }

        public new async Task DeleteByIdBatchAsync(Batch batch, int id)
        {
            // Get a reference to the item to Delete
            DeleteByIdImplementation(id, out object concreteEntity, out ApiCall apiCall);

            await (concreteEntity as BaseDataModel<IFileVersion>).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private void DeleteByIdImplementation(int id, out object concreteEntity, out ApiCall apiCall)
        {
            concreteEntity = null;
            bool removeFromModel = false;
            var modelToDeleteIndex = items.FindIndex(i => ((IDataModelWithKey)i).Key.Equals(id));
            if (modelToDeleteIndex >= 0)
            {
                // Use the existing concrete entity for the delete
                concreteEntity = items[modelToDeleteIndex];
                removeFromModel = true;
            }

            // Delete the item
            apiCall = new ApiCall($"_api/Web/getFileById('{{Parent.Id}}')/versions/deletebyid(vid={id})", ApiType.SPORest)
            {
                RemoveFromModel = removeFromModel
            };
        }

        #endregion

        #region DeleteByLabel methods

        public void DeleteByLabel(string label)
        {
            DeleteByLabelAsync(label).GetAwaiter().GetResult();
        }

        public async Task DeleteByLabelAsync(string label)
        {
            // Get a reference to the item to Delete
            DeleteByLabelImplementation(label, out object concreteEntity, out ApiCall apiCall);

            var response = await (concreteEntity as BaseDataModel<IFileVersion>).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void DeleteByLabelBatch(string label)
        {
            DeleteByLabelBatchAsync(label).GetAwaiter().GetResult();
        }

        public async Task DeleteByLabelBatchAsync(string label)
        {
            await DeleteByLabelBatchAsync(PnPContext.CurrentBatch, label).ConfigureAwait(false);
        }

        public void DeleteByLabelBatch(Batch batch, string label)
        {
            DeleteByLabelBatchAsync(batch, label).GetAwaiter().GetResult();
        }

        public async Task DeleteByLabelBatchAsync(Batch batch, string label)
        {
            // Get a reference to the item to Delete
            DeleteByLabelImplementation(label, out object concreteEntity, out ApiCall apiCall);

            await (concreteEntity as BaseDataModel<IFileVersion>).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private void DeleteByLabelImplementation(string label, out object concreteEntity, out ApiCall apiCall)
        {
            concreteEntity = null;
            bool removeFromModel = false;
            var modelToDeleteIndex = items.FindIndex(i => i.VersionLabel.Equals(label));
            if (modelToDeleteIndex >= 0)
            {
                // Use the existing concrete entity for the delete
                concreteEntity = items[modelToDeleteIndex];
                removeFromModel = true;
            }

            // Delete the item
            apiCall = new ApiCall($"_api/Web/getFileById('{{Parent.Id}}')/versions/deletebylabel(versionlabel={label})", ApiType.SPORest)
            {
                RemoveFromModel = removeFromModel
            };
        }

        #endregion

        #region RecycleById methods

        public void RecycleById(int id)
        {
            RecycleByIdAsync(id).GetAwaiter().GetResult();
        }

        public async Task RecycleByIdAsync(int id)
        {
            // Get a reference to the item to recycle
            RecycleByIdImplementation(id, out object concreteEntity, out ApiCall apiCall);

            var response = await (concreteEntity as BaseDataModel<IFileVersion>).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void RecycleByIdBatch(int id)
        {
            RecycleByIdBatchAsync(id).GetAwaiter().GetResult();
        }

        public async Task RecycleByIdBatchAsync(int id)
        {
            await RecycleByIdBatchAsync(PnPContext.CurrentBatch, id).ConfigureAwait(false);
        }

        public void RecycleByIdBatch(Batch batch, int id)
        {
            RecycleByIdBatchAsync(batch, id).GetAwaiter().GetResult();
        }

        public async Task RecycleByIdBatchAsync(Batch batch, int id)
        {
            // Get a reference to the item to recycle
            RecycleByIdImplementation(id, out object concreteEntity, out ApiCall apiCall);

            await (concreteEntity as BaseDataModel<IFileVersion>).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private void RecycleByIdImplementation(int id, out object concreteEntity, out ApiCall apiCall)
        {
            concreteEntity = null;
            bool removeFromModel = false;
            var modelToDeleteIndex = items.FindIndex(i => ((IDataModelWithKey)i).Key.Equals(id));
            if (modelToDeleteIndex >= 0)
            {
                // Use the existing concrete entity for the delete
                concreteEntity = items[modelToDeleteIndex];
                removeFromModel = true;
            }

            // Recycle the item
            apiCall = new ApiCall($"_api/Web/getFileById('{{Parent.Id}}')/versions/recyclebyid(vid={id})", ApiType.SPORest)
            {
                RemoveFromModel = removeFromModel
            };
        }

        #endregion

        #region RecycleByLabel methods

        public void RecycleByLabel(string label)
        {
            RecycleByLabelAsync(label).GetAwaiter().GetResult();
        }

        public async Task RecycleByLabelAsync(string label)
        {
            // Get a reference to the item to recycle
            RecycleByLabelImplementation(label, out object concreteEntity, out ApiCall apiCall);

            var response = await (concreteEntity as BaseDataModel<IFileVersion>).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void RecycleByLabelBatch(string label)
        {
            RecycleByLabelBatchAsync(label).GetAwaiter().GetResult();
        }

        public async Task RecycleByLabelBatchAsync(string label)
        {
            await RecycleByLabelBatchAsync(PnPContext.CurrentBatch, label).ConfigureAwait(false);
        }

        public void RecycleByLabelBatch(Batch batch, string label)
        {
            RecycleByLabelBatchAsync(batch, label).GetAwaiter().GetResult();
        }

        public async Task RecycleByLabelBatchAsync(Batch batch, string label)
        {
            // Get a reference to the item to recycle
            RecycleByLabelImplementation(label, out object concreteEntity, out ApiCall apiCall);

            await (concreteEntity as BaseDataModel<IFileVersion>).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private void RecycleByLabelImplementation(string label, out object concreteEntity, out ApiCall apiCall)
        {
            concreteEntity = null;
            bool removeFromModel = false;
            var modelToDeleteIndex = items.FindIndex(i => i.VersionLabel.Equals(label));
            if (modelToDeleteIndex >= 0)
            {
                // Use the existing concrete entity for the delete
                concreteEntity = items[modelToDeleteIndex];
                removeFromModel = true;
            }

            // Recycle the item
            apiCall = new ApiCall($"_api/Web/getFileById('{{Parent.Id}}')/versions/RecycleByLabel(versionlabel={label})", ApiType.SPORest)
            {
                RemoveFromModel = removeFromModel
            };
        }

        #endregion

        #region RestoreByLabel methods

        public void RestoreByLabel(string label)
        {
            RestoreByLabelAsync(label).GetAwaiter().GetResult();
        }

        public async Task RestoreByLabelAsync(string label)
        {
            // Get a reference to the item to recycle
            RestoreByLabelImplementation(label, out ApiCall apiCall);

            var response = await (PnPContext.Web as BaseDataModel<IWeb>).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void RestoreByLabelBatch(string label)
        {
            RestoreByLabelBatchAsync(label).GetAwaiter().GetResult();
        }

        public async Task RestoreByLabelBatchAsync(string label)
        {
            await RestoreByLabelBatchAsync(PnPContext.CurrentBatch, label).ConfigureAwait(false);
        }

        public void RestoreByLabelBatch(Batch batch, string label)
        {
            RestoreByLabelBatchAsync(batch, label).GetAwaiter().GetResult();
        }

        public async Task RestoreByLabelBatchAsync(Batch batch, string label)
        {
            // Get a reference to the item to recycle
            RestoreByLabelImplementation(label, out ApiCall apiCall);

            await (PnPContext.Web as BaseDataModel<IWeb>).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private void RestoreByLabelImplementation(string label, out ApiCall apiCall)
        {
            // Recycle the item
            apiCall = new ApiCall($"_api/Web/getFileById('{(Parent as File).UniqueId}')/versions/restorebylabel(versionlabel={label})", ApiType.SPORest);
        }

        #endregion

    }
}
