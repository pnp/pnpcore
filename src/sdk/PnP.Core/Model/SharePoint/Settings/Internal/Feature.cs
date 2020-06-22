using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{

    //[SharePointType("SP.Feature", Target = typeof(Site), Uri = "_api/site/features/getbyid(guid'{Id}')')", Get = "_api/site/features", LinqGet = "_api/site/features")]
    [SharePointType("SP.Feature", Target = typeof(Web), Uri = "_api/Web/Features/GetById(guid'{Id}')", Get = "_api/Web/Features", LinqGet = "_api/Web/Features")]
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

                //var addParameters = new FeatureAdd(this, DefinitionId);
                return new ApiCall($"{entity.SharePointGet}/add(guid'{DefinitionId}')", ApiType.SPORest, null);
            };
        }

        /// <summary>
        /// Class to model the rest feature
        /// </summary>
        internal class FeatureAdd : RestBaseAdd<IFeature>
        {
            public Guid DefinitionId { get; set; }
            //public bool Force { get; set; }

            //public int FeatDefScope { get; set; }

            internal FeatureAdd(BaseDataModel<IFeature> model, Guid definitionId) : base(model)
            {
                // bool force = false, int featureDefScope = 0
                DefinitionId = definitionId;
                //Force = force;
                //FeatDefScope = featureDefScope;
            }
        }
    }
}
