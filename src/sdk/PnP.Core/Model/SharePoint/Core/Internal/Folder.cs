using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Folder class, write your custom code here
    /// </summary>
    [SharePointType("SP.Folder", Target = typeof(Web), Uri = "_api/Web/getFolderById('{Id}')", LinqGet = "_api/Web/Folders")]
    [SharePointType("SP.Folder", Target = typeof(Folder), Uri = "_api/Web/getFolderById('{Id}')", Get = "_api/Web/getFolderById('{Parent.Id}')/Folders", LinqGet = "_api/Web/getFolderById('{Parent.Id}')/Folders")]
    [SharePointType("SP.Folder", Target = typeof(List), Uri = "_api/Web/Lists(guid'{Parent.Id}')/rootFolder", LinqGet = "_api/Web/Lists(guid'{Parent.Id}')/Folders")]
    internal partial class Folder
    {
        public Folder()
        {
            //MappingHandler = (FromJson input) =>
            //{
            //    // implement custom mapping logic
            //    switch (input.TargetType.Name)
            //    {
            //        case "SearchScopes": return JsonMappingHelper.ToEnum<SearchScopes>(input.JsonElement);
            //        case "SearchBoxInNavBar": return JsonMappingHelper.ToEnum<SearchBoxInNavBar>(input.JsonElement);
            //    }

            //    input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

            //    return null;
            //};

            // Handler to construct the Add request for this folder
            AddApiCallHandler = async (keyValuePairs) =>
            {
                // Given this method can apply on both Web.ContentTypes as List.ContentTypes we're getting the entity info which will 
                // automatically provide the correct 'parent'
                var entity = EntityManager.Instance.GetClassInfo(GetType(), this);

                //// Adding new content types on a list is not something we should allow
                //if (entity.Target == typeof(List))
                //{
                //throw new ClientException(ErrorType.Unsupported, "Adding new content types on a list is not possible, use the AddAvailableContentType method to add an existing site content type");
                //}

                return new ApiCall($"{entity.SharePointGet}/Add('{Name}')", ApiType.SPORest);
            };
        }


        #region Extension methods

        #region Add sub folder
        /// <summary>
        /// Add a sub folder to the current folder.
        /// </summary>
        /// <param name="name">The name of the sub folder to add.</param>
        /// <returns>The added sub folder.</returns>
        public async Task<IFolder> AddSubFolderAsync(string name)
        {
            return await Folders.AddAsync(name).ConfigureAwait(false);
        }

        /// <summary>
        /// Add a sub folder to the current folder.
        /// </summary>
        /// <param name="name">The name of the sub folder to add.</param>
        /// <returns>The added sub folder.</returns>
        public IFolder AddSubFolder(string name)
        {
            return AddSubFolderAsync(name).GetAwaiter().GetResult();
        }
        #endregion
        #endregion
    }
}
