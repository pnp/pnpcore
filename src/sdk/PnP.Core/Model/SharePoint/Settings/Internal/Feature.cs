using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.SharePoint
{

    //[SharePointType("SP.Feature", Target = typeof(Site), Uri = "_api/site/features/getbyid(guid'{Id}')')", Get = "_api/site/features", LinqGet = "_api/site/features")]
    [SharePointType("SP.Feature", Uri = "/_api/Web/Features/GetById(guid'{Id}')", Get = "/_api/Web/Features", LinqGet = "/_api/Web/Features")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
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
