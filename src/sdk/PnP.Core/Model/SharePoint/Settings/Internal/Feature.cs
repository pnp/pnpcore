using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{

    //[SharePointType("SP.Feature", Target = typeof(Site), Uri = "_api/site/features/getbyid(guid'{Id}')')", Get = "_api/site/features", LinqGet = "_api/site/features")]
    [SharePointType("SP.Feature", Target = typeof(Web), Uri = "_api/Web/Features/GetById(guid'{DefinitionId}')", Get = "_api/Web/Features", LinqGet = "_api/Web/Features")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class Feature
    {

        // Add Feature

        // Get Feature

        // Remove Feature

        public Feature()
        {
            AddApiCallHandler = (keyValuePairs) =>
            {
                var entity = EntityManager.Instance.GetClassInfo<IFeature>(GetType(), this);
                return new ApiCall($"{entity.SharePointGet}/add(guid'{DefinitionId}')", ApiType.SPORest, null);
            };
        }

        public async Task RemoveAsync()
        {
            var entity = EntityManager.Instance.GetClassInfo<IFeature>(GetType(), this);
            var apiCall = new ApiCall($"{entity.SharePointGet}/remove(guid'{DefinitionId}')", ApiType.SPORest);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

    }
}
