using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldLink class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldLink", Target = typeof(ContentType), Uri = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks')", Get = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks", LinqGet = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks")]
    // TODO A special target should be achieve to support List Content Types
    //[SharePointType("SP.FieldLink", Target = typeof(ContentType), Uri = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks')", Get = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks", LinqGet = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks")]
    internal partial class FieldLink : BaseDataModel<IFieldLink>, IFieldLink
    {
        #region Construction
        public FieldLink()
        {
            // Handler to construct the Add request for this content type
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (keyValuePairs) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                // Given this method can apply on both Web.ContentTypes as List.ContentTypes we're getting the entity info which will 
                // automatically provide the correct 'parent'
                var entity = EntityManager.GetClassInfo(GetType(), this);

                // The FieldLinkCollection Add REST endpoint has many limitations
                // https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-visio/jj245869%28v%3doffice.15%29#rest-resource-endpoint
                // It ONLY works with Site Columns already present in the parent content content type or  with list columns if they are already added to the list
                // TODO: Probably worthy to recommend not using this endpoint for adding fieldlinks... What alternative ?
                var addParameters = new
                {
                    __metadata = new { type = entity.SharePointType },
                    FieldInternalName,
                    Hidden,
                    Required,
                    ReadOnly,
                    ShowInDisplayForm,
                    DisplayName = HasValue(nameof(DisplayName)) ? DisplayName : null
                }.AsExpando(ignoreNullValues: true);
                return new ApiCall(entity.SharePointGet, ApiType.SPORest, JsonSerializer.Serialize(addParameters, typeof(ExpandoObject)));
            };
        }
        #endregion

        #region Properties
        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string FieldInternalName { get => GetValue<string>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public bool ReadOnly { get => GetValue<bool>(); set => SetValue(value); }

        public bool Required { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShowInDisplayForm { get => GetValue<bool>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }
        #endregion
    }
}
