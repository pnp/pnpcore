using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.SharePoint
{

    [SharePointType("SP.Feature", Target = typeof(Site), Uri = "_api/site/feature('{Id}')", Get = "_api/site/feature", LinqGet = "_api/site/feature")]
    [SharePointType("SP.Feature", Target = typeof(Web), Uri = "_api/web/feature('{Id}')", Get = "_api/web/feature", LinqGet = "_api/web/feature")]
    internal partial class Feature
    {

        // Add Feature

        // Get Feature

        // Remove Feature

        public Feature()
        {

        }
    }
}
