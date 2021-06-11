using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class FileVersionCollection : QueryableDataModelCollection<IFileVersion>, IFileVersionCollection
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
            return await this.QueryProperties(selectors).FirstOrDefaultAsync(l => l.ID == id).ConfigureAwait(false);
        }

        #endregion

        // TODO: Need to write test methods for this
        //#region RecycleById methods

        //public Guid RecycleById(int id)
        //{
        //    return RecycleByIdAsync(id).GetAwaiter().GetResult();
        //}

        //public async Task<Guid> RecycleByIdAsync(int id)
        //{
        //    // Get a reference to the item to recycle
        //    RecycleByIdImplementation(id, out object concreteEntity, out ApiCall apiCall);

        //    var response = await (concreteEntity as BaseDataModel<IFileVersion>).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

        //    if (!string.IsNullOrEmpty(response.Json))
        //    {
        //        var document = JsonSerializer.Deserialize<JsonElement>(response.Json);
        //        if (document.TryGetProperty("d", out JsonElement root))
        //        {
        //            if (root.TryGetProperty("Recycle", out JsonElement recycleBinItemId))
        //            {
        //                // return the recyclebin item id
        //                return recycleBinItemId.GetGuid();
        //            }
        //        }
        //    }

        //    return Guid.Empty;
        //}

        //public void RecycleByIdBatch(int id)
        //{
        //    RecycleByIdBatchAsync(id).GetAwaiter().GetResult();
        //}

        //public async Task RecycleByIdBatchAsync(int id)
        //{
        //    await RecycleByIdBatchAsync(PnPContext.CurrentBatch, id).ConfigureAwait(false);
        //}

        //public void RecycleByIdBatch(Batch batch, int id)
        //{
        //    RecycleByIdBatchAsync(batch, id).GetAwaiter().GetResult();
        //}

        //public async Task RecycleByIdBatchAsync(Batch batch, int id)
        //{
        //    // Get a reference to the item to recycle
        //    RecycleByIdImplementation(id, out object concreteEntity, out ApiCall apiCall);

        //    await (concreteEntity as BaseDataModel<IFileVersion>).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        //}

        //private void RecycleByIdImplementation(int id, out object concreteEntity, out ApiCall apiCall)
        //{
        //    concreteEntity = null;
        //    bool removeFromModel = false;
        //    var modelToDeleteIndex = items.FindIndex(i => ((IDataModelWithKey)i).Key.Equals(id));
        //    if (modelToDeleteIndex >= 0)
        //    {
        //        // Use the existing concrete entity for the delete
        //        concreteEntity = items[modelToDeleteIndex];
        //        removeFromModel = true;
        //    }
        //    else
        //    {
        //        concreteEntity = new FileVersion()
        //        {
        //            Parent = Parent,
        //            PnPContext = PnPContext
        //        };
        //        ((IMetadataExtensible)concreteEntity).Metadata.Add(PnPConstants.MetaDataRestId, id.ToString());
        //    }

        //    // Recycle the item
        //    apiCall = new ApiCall($"_api/Web/getFileById('{{Parent.Id}}')/versions/recyclebyid(vid={id})", ApiType.SPORest)
        //    {
        //        RemoveFromModel = removeFromModel
        //    };
        //}

        //#endregion
    }
}
