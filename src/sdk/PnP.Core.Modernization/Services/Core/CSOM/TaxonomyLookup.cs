using System;
using System.Xml.Linq;

namespace PnP.Core.Modernization.Services.Core.CSOM
{
    /// <summary>
    /// Looks up a term when only its ID is known. The modern term-store REST API requires a term-set ID.
    /// </summary>
    internal static class TaxonomyLookup
    {
        internal static string GetLabel(PnPContext context, Guid termId)
        {
            var ns = ProcessQueryClient.Namespace;
            var request = new XElement(ns + "Request",
                new XAttribute("SchemaVersion", "15.0.0.0"), new XAttribute("LibraryVersion", "16.0.0.0"),
                new XAttribute("ApplicationName", "PnP.Core.Modernization"),
                new XElement(ns + "Actions",
                    new XElement(ns + "Query", new XAttribute("Id", 4), new XAttribute("ObjectPathId", 3),
                        new XElement(ns + "Query", new XAttribute("SelectAllProperties", false),
                            new XElement(ns + "Properties",
                                new XElement(ns + "Property", new XAttribute("Name", "Name"), new XAttribute("ScalarProperty", true)))))),
                new XElement(ns + "ObjectPaths",
                    new XElement(ns + "StaticMethod", new XAttribute("Id", 1), new XAttribute("Name", "GetTaxonomySession"),
                        new XAttribute("TypeId", "{981cbc68-9edc-4f8d-872f-71146fcbb84f}")),
                    new XElement(ns + "Method", new XAttribute("Id", 2), new XAttribute("ParentId", 1),
                        new XAttribute("Name", "GetDefaultSiteCollectionTermStore")),
                    new XElement(ns + "Method", new XAttribute("Id", 3), new XAttribute("ParentId", 2), new XAttribute("Name", "GetTerm"),
                        new XElement(ns + "Parameters", new XElement(ns + "Parameter", new XAttribute("Type", "Guid"), termId.ToString("B"))))));
            var result = ProcessQueryClient.GetResult(ProcessQueryClient.ExecuteAsync(context, request).GetAwaiter().GetResult(), 4);
            return result.GetProperty("Name").GetString();
        }
    }
}
