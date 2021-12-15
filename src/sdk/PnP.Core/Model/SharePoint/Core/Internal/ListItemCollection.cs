using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class ListItemCollection : QueryableDataModelCollection<IListItem>, IListItemCollection
    {
        public ListItemCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Replace override
        public override void Replace(int itemIndex, IListItem newItem)
        {
            (items[itemIndex] as TransientObject).Merge(newItem as TransientObject);
        }
        #endregion

        #region Add methods

        public async Task<IListItem> AddBatchAsync(Dictionary<string, object> values, string folderPath = null, FileSystemObjectType fileSystemObjectType = FileSystemObjectType.File)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, values, folderPath, fileSystemObjectType).ConfigureAwait(false);
        }

        public IListItem AddBatch(Dictionary<string, object> values, string folderPath = null, FileSystemObjectType fileSystemObjectType = FileSystemObjectType.File)
        {
            return AddBatchAsync(values, folderPath, fileSystemObjectType).GetAwaiter().GetResult();
        }

        public async Task<IListItem> AddBatchAsync(Batch batch, Dictionary<string, object> values, string folderPath = null, FileSystemObjectType fileSystemObjectType = FileSystemObjectType.File)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var newListItem = CreateNewAndAdd() as ListItem;

            // Assign field values
            newListItem.Values.SystemAddRange(values);

            return await newListItem.AddBatchAsync(batch, new Dictionary<string, object> { { ListItem.FolderPath, folderPath }, { ListItem.UnderlyingObjectType, fileSystemObjectType } }).ConfigureAwait(false) as ListItem;
        }

        public IListItem AddBatch(Batch batch, Dictionary<string, object> values, string folderPath = null, FileSystemObjectType fileSystemObjectType = FileSystemObjectType.File)
        {
            return AddBatchAsync(batch, values, folderPath, fileSystemObjectType).GetAwaiter().GetResult();
        }

        public async Task<IListItem> AddAsync(Dictionary<string, object> values, string folderPath = null, FileSystemObjectType fileSystemObjectType = FileSystemObjectType.File)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var newListItem = CreateNewAndAdd() as ListItem;

            // Assign field values
            newListItem.Values.SystemAddRange(values);

            return await newListItem.AddAsync(new Dictionary<string, object> { { ListItem.FolderPath, folderPath }, { ListItem.UnderlyingObjectType, fileSystemObjectType } }).ConfigureAwait(false) as ListItem;
        }

        public IListItem Add(Dictionary<string, object> values, string folderPath = null, FileSystemObjectType fileSystemObjectType = FileSystemObjectType.File)
        {
            return AddAsync(values, folderPath, fileSystemObjectType).GetAwaiter().GetResult();
        }

        #endregion

        #region Contains method

        public bool Contains(int id)
        {
            if (items.FirstOrDefault(p => p.Id == id) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region GetById methods

        public IListItem GetById(int id, params Expression<Func<IListItem, object>>[] selectors)
        {
            return GetByIdAsync(id, selectors).GetAwaiter().GetResult();
        }

        public async Task<IListItem> GetByIdAsync(int id, params Expression<Func<IListItem, object>>[] selectors)
        {
            return await this.QueryProperties(selectors).FirstOrDefaultAsync(l => l.Id == id).ConfigureAwait(false);
        }

        #endregion

        #region RecycleById methods

        public Guid RecycleById(int id)
        {
            return RecycleByIdAsync(id).GetAwaiter().GetResult();
        }

        public async Task<Guid> RecycleByIdAsync(int id)
        {
            // Get a reference to the item to recycle
            RecycleByIdImplementation(id, out object concreteEntity, out ApiCall apiCall);

            var response = await (concreteEntity as BaseDataModel<IListItem>).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                var document = JsonSerializer.Deserialize<JsonElement>(response.Json);
                if (document.TryGetProperty("value", out JsonElement recycleBinItemId))
                {
                    // return the recyclebin item id
                    return recycleBinItemId.GetGuid();
                }
            }

            return Guid.Empty;
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

            await (concreteEntity as BaseDataModel<IListItem>).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
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
            else
            {
                concreteEntity = new ListItem()
                {
                    Parent = Parent,
                    PnPContext = PnPContext
                };
                (concreteEntity as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataRestId, id.ToString());
            }

            // Recycle the item
            apiCall = new ApiCall("_api/Web/Lists(guid'{Parent.Id}')/items({Id})/recycle", ApiType.SPORest)
            {
                RemoveFromModel = removeFromModel
            };
        }

        #endregion

    }
}
