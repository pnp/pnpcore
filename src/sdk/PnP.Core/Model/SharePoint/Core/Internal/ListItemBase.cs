using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal class ListItemBase<TModel> : ExpandoBaseDataModel<TModel>
    {
        #region Properties
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool CommentsDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public string Title { get => (string)Values["Title"]; set => Values["Title"] = value; }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }
        #endregion

        #region Extension methods

        #region UpdateOverwriteVersion

        public async Task UpdateOverwriteVersionAsync()
        {
            var xmlPayload = BuildXmlPayload(true);
            if (!string.IsNullOrEmpty(xmlPayload))
            {
                var apiCall = new ApiCall(xmlPayload)
                {
                    Commit = true
                };
                await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                PnPContext.Logger.LogInformation(PnPCoreResources.Log_Information_NoChangesSkipSystemUpdate);
            }
        }

        public void UpdateOverwriteVersion()
        {
            UpdateOverwriteVersionAsync().GetAwaiter().GetResult();
        }

        public async Task UpdateOverwriteVersionBatchAsync()
        {
            await UpdateOverwriteVersionBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void UpdateOverwriteVersionBatch()
        {
            UpdateOverwriteVersionBatchAsync().GetAwaiter().GetResult();
        }

        public async Task UpdateOverwriteVersionBatchAsync(Batch batch)
        {
            var xmlPayload = BuildXmlPayload(true);
            if (!string.IsNullOrEmpty(xmlPayload))
            {
                var apiCall = new ApiCall(xmlPayload)
                {
                    Commit = true
                };
                await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                PnPContext.Logger.LogInformation(PnPCoreResources.Log_Information_NoChangesSkipSystemUpdate);
            }
        }

        public void UpdateOverwriteVersionBatch(Batch batch)
        {
            UpdateOverwriteVersionBatchAsync(batch).GetAwaiter().GetResult();
        }

        #endregion

        #region SystemUpdate

        public async Task SystemUpdateAsync()
        {
            var xmlPayload = BuildXmlPayload(false);
            if (!string.IsNullOrEmpty(xmlPayload))
            {
                var apiCall = new ApiCall(xmlPayload)
                {
                    Commit = true
                };
                await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                PnPContext.Logger.LogInformation(PnPCoreResources.Log_Information_NoChangesSkipSystemUpdate);
            }
        }

        public void SystemUpdate()
        {
            SystemUpdateAsync().GetAwaiter().GetResult();
        }

        public async Task SystemUpdateBatchAsync()
        {
            await SystemUpdateBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void SystemUpdateBatch()
        {
            SystemUpdateBatchAsync().GetAwaiter().GetResult();
        }

        public async Task SystemUpdateBatchAsync(Batch batch)
        {
            var xmlPayload = BuildXmlPayload(false);
            if (!string.IsNullOrEmpty(xmlPayload))
            {
                var apiCall = new ApiCall(xmlPayload)
                {
                    Commit = true
                };
                await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                PnPContext.Logger.LogInformation(PnPCoreResources.Log_Information_NoChangesSkipSystemUpdate);
            }
        }

        public void SystemUpdateBatch(Batch batch)
        {
            SystemUpdateBatchAsync(batch).GetAwaiter().GetResult();
        }

        private string BuildXmlPayload(bool updateOverwriteVersion)
        {
            string xml;

            if (updateOverwriteVersion)
            {
                xml = CsomHelper.ListItemUpdateOverwriteVersion;
            }
            else
            {
                xml = CsomHelper.ListItemSystemUpdate;
            }

            int counter = 1;
            StringBuilder fieldValues = new StringBuilder();

            var entity = EntityManager.GetClassInfo(GetType(), this);
            IEnumerable<EntityFieldInfo> fields = entity.Fields;

            var changedProperties = GetChangedProperties();

            bool changeFound = false;
            foreach (PropertyDescriptor cp in changedProperties)
            {
                changeFound = true;
                // Look for the corresponding property in the type
                var changedField = fields.FirstOrDefault(f => f.Name == cp.Name);

                // If we found a field 
                if (changedField != null)
                {
                    if (changedField.DataType.FullName == typeof(TransientDictionary).FullName)
                    {
                        // Get the changed properties in the dictionary
                        var dictionaryObject = (TransientDictionary)cp.GetValue(this);
                        foreach (KeyValuePair<string, object> changedProp in dictionaryObject.ChangedProperties)
                        {
                            // Let's set its value into the update message
                            fieldValues.AppendLine(SetFieldValueXml(changedProp.Key, changedProp.Value, changedProp.Value?.GetType().Name, ref counter));
                        }
                    }
                    else
                    {
                        // Let's set its value into the update message
                        fieldValues.AppendLine(SetFieldValueXml(changedField.SharePointName, GetValue(changedField.Name), changedField.DataType.Name, ref counter));
                    }
                }
            }

            // No changes, so bail out
            if (!changeFound)
            {
                return null;
            }

            // update field values
            xml = xml.Replace("{FieldValues}", fieldValues.ToString());

            // update counter
            xml = xml.Replace("{Counter}", counter.ToString());

            return xml;
        }

        private static string SetFieldValueXml(string fieldName, object fieldValue, string fieldType, ref int counter)
        {
            string xml = CsomHelper.ListItemSystemUpdateSetFieldValue;

            xml = xml.Replace("{Counter}", counter.ToString());
            xml = xml.Replace("{FieldName}", fieldName);
            // TODO: verify complex fieldtypes
            xml = xml.Replace("{FieldValue}", fieldValue == null ? "" : CsomHelper.XmlString(TypeSpecificHandling(fieldValue.ToString(), fieldType), false));
            xml = xml.Replace("{FieldType}", fieldType ?? "Null");

            counter++;
            return xml;
        }

        private static string TypeSpecificHandling(string value, string fieldType)
        {
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(fieldType) && fieldType.Equals("Boolean"))
            {
                return value.ToLowerInvariant();
            }

            return value;
        }
        #endregion

        #endregion

    }
}
