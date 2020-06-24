using PnP.Core.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{

    [SharePointType("SP.Feature", Target = typeof(Site), Uri = "_api/Site/Features/GetById(guid'{DefinitionId}')')", Get = "_api/Site/Features", LinqGet = "_api/Site/Features")]
    [SharePointType("SP.Feature", Target = typeof(Web), Uri = "_api/Web/Features/GetById(guid'{DefinitionId}')", Get = "_api/Web/Features", LinqGet = "_api/Web/Features")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class Feature
    {

        public Feature()
        {
            AddApiCallHandler = (keyValuePairs) =>
            {
                var entity = EntityManager.Instance.GetClassInfo(GetType(), this);
                return new ApiCall($"{entity.SharePointGet}/add(guid'{DefinitionId}')", ApiType.SPORest, null);
            };
        }

        public async Task RemoveAsync()
        {
            var entity = EntityManager.Instance.GetClassInfo(GetType(), this);
            var apiCall = new ApiCall($"{entity.SharePointGet}/remove(guid'{DefinitionId}')", ApiType.SPORest);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

    }
}
