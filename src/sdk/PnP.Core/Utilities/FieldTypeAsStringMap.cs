using AngleSharp.Dom;
using AngleSharp.Io;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PnP.Core.Utilities
{
    internal static class FieldTypeAsStringMap
    {
        private static readonly Lazy<IDictionary<string, string>> mappings = new Lazy<IDictionary<string, string>>(BuildMappings);

        private static IDictionary<string, string> BuildMappings()
        {

            // CSOM FieldType Enum - https://learn.microsoft.com/en-us/dotnet/api/microsoft.sharepoint.client.fieldtype?view=sharepoint-csom#microsoft-sharepoint-client-fieldtype-geolocation
            // JavaScript API - https://learn.microsoft.com/en-us/previous-versions/office/sharepoint-visio/jj246996(v=office.15)
            // PnP.Core.Model.SharePoint.FieldType

            var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                //["Invalid"] = "",
                ["Integer"] = "SP.FieldNumber",
                ["Text"] = "SP.FieldText",
                ["Note"] = "SP.FieldMultiLineText",
                ["DateTime"] = "SP.FieldDateTime",
                //["Counter"] = ""
                ["Choice"] = "SP.FieldChoice",
                ["Lookup"] = "SP.FieldLookup",
                //["Boolean"] = ""
                ["Number"] = "SP.FieldNumber",
                ["Currency"] = "SP.FieldCurrency",
                ["URL"] = "SP.FieldUrl",
                ["Computed"] = "SP.FieldComputed",
                //["Threading"] = "",
                ["Guid"] = "SP.FieldGuid",
                ["MultiChoice"] = "SP.FieldMultiChoice",
                //["GridChoice"] = "",
                ["Calculated"] = "SP.FieldCalculated",
                //["File"] = ""
                //["Attachments"] = "",
                ["User"] = "SP.FieldUser",
                //["Recurrence"] = ""
                //["CrossProjectLink"] = ""
                //["ModStat"] = ""
                //["Error"] = ""
                ["ContentTypeId"] = "SP.FieldContentTypeId",
                //["PageSeparator"] = ""
                //["ThreadIndex"] = ""
                //["WorkflowStatus"] = ""
                //["AllDayEvent"] = ""
                //["WorkflowEventType"] = ""
                ["Geolocation"] = "SP.FieldGeolocation",
                //["OutcomeChoice"] = ""
                //["MaxItems"] = ""


                ["LookupMulti"] = "SP.FieldLookup",
                ["UserMulti"] = "SP.FieldUser",
                ["TaxonomyFieldType"] = "SP.Taxonomy.TaxonomyField",
                ["TaxonomyFieldTypeMulti"] = "SP.Taxonomy.TaxonomyField",

                //["HTML"] = "",
                //["Overbook"] = "",
                //["ContactInfo"] = "",

                //SP.FieldRatingScale

                //["Location"] = "",
                //["Thumbnail"] = "",
            };

            /*
            var cache = mappings.ToList(); // need ToList() to avoid modifying while still enumerating

            foreach (var mapping in cache)
            {
                if (!mappings.ContainsKey(mapping.Value))
                {
                    mappings.Add(mapping.Value, mapping.Key);
                }
            }
            */

            return mappings;
        }

        /// <summary>
        /// Tries to get the type of the entity from the provided string.
        /// </summary>
        /// <param name="typeAsString">The field type as string.</param>
        /// <param name="entityType">The variable to store the entity type.</param>
        /// <returns>The entity type.</returns>
        /// <exception cref="ArgumentNullException" />
        internal static bool TryGetFieldEntityType(string typeAsString, out string entityType)
        {
            if (typeAsString == null)
            {
                throw new ArgumentNullException(nameof(typeAsString));
            }

            return mappings.Value.TryGetValue(typeAsString, out entityType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeAsString"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetByTypeAsString(string typeAsString, string defaultValue = "SP.Field")
        {
            if (typeAsString == null)
            {
                throw new ArgumentNullException(nameof(typeAsString));
            }

            if (TryGetFieldEntityType(typeAsString, out var entityType))
            {
                return entityType;
            }

            return defaultValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextAwareObject"></param>
        /// <param name="entity"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string TryToGetConcretFieldMetadataType(IDataModelWithContext contextAwareObject, EntityInfo entity, string defaultValue = "SP.Field")
        {
            if (contextAwareObject == null)
            {
                throw new ArgumentNullException(nameof(contextAwareObject));
            }

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (contextAwareObject is Field field
                && !string.IsNullOrEmpty(field.TypeAsString)
                && TryGetFieldEntityType(field.TypeAsString, out var entityType))
            {
                return entityType;
            }

            return entity.SharePointType;
        }
    }
}
