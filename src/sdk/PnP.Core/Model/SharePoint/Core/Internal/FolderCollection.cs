using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class FolderCollection : QueryableDataModelCollection<IFolder>, IFolderCollection
    {
        public FolderCollection(PnPContext context, IDataModelParent parent, string memberName = null) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Add
        public async Task<IFolder> AddBatchAsync(string name)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, name).ConfigureAwait(false);
        }

        public IFolder AddBatch(string name)
        {
            return AddBatchAsync(name).GetAwaiter().GetResult();
        }

        public async Task<IFolder> AddBatchAsync(Batch batch, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newFolder = CreateNewAndAdd() as Folder;

            newFolder.Name = name;

            return await newFolder.AddBatchAsync(batch).ConfigureAwait(false) as Folder;
        }

        public IFolder AddBatch(Batch batch, string name)
        {
            return AddBatchAsync(batch, name).GetAwaiter().GetResult();
        }

        public async Task<IFolder> AddAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }


            var newFolder = CreateNewAndAdd() as Folder;

            newFolder.Name = name;

            return await newFolder.AddAsync().ConfigureAwait(false) as Folder;
        }

        public IFolder Add(string name)
        {
            return AddAsync(name).GetAwaiter().GetResult();
        }
        #endregion
    }
}
