using PnP.Core.QueryModel.Model;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class ContentTypeCollection
    {
        public IContentType Add(string id, string name, string description = null, string group = null)
        {
            return Add(PnPContext.CurrentBatch, id, name, description, group);
        }

        public IContentType Add(Batch batch, string id, string name, string description = null, string group = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newContentType = AddNewContentType();

            newContentType.StringId = id;
            newContentType.Name = name;
            newContentType.Description = description;
            newContentType.Group = group;

            return newContentType.Add(batch) as ContentType;
        }

        public async Task<IContentType> AddAsync(string id, string name, string description = null, string group = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newContentType = AddNewContentType();

            newContentType.StringId = id;
            newContentType.Name = name;
            newContentType.Description = description;
            newContentType.Group = group;

            return await newContentType.AddAsync().ConfigureAwait(false) as ContentType;
        }
    }
}
