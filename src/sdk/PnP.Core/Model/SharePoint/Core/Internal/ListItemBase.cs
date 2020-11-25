using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal class ListItemBase<TModel> : ExpandoBaseDataModel<TModel>
    {
        #region Properties
        public int Id { get => GetValue<int>(); set => SetValue(value); }

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

            int taxFieldObjectId = 100;
            int taxFieldIdentityObjectId = 200;
            StringBuilder taxonomyMultiValueObjectPaths = new StringBuilder();
            StringBuilder taxonomyMultiValueIdentities = new StringBuilder();

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
                            if (changedProp.Value is FieldValue)
                            {
                                if (changedProp.Value is FieldLookupValue && (changedProp.Value as FieldLookupValue).LookupId == -1)
                                {
                                    fieldValues.Append(SetFieldValueAsNullXml(changedProp.Key, ref counter));
                                }
                                else
                                {
                                    fieldValues.Append(SetSpecialFieldValueXml(changedProp.Key, changedProp.Value as FieldValue, ref counter));
                                }
                            }
                            else if (changedProp.Value == null)
                            {
                                fieldValues.Append(SetFieldValueAsNullXml(changedProp.Key, ref counter));
                            }
                            else if (changedProp.Value is FieldValueCollection)
                            {
                                var collection = changedProp.Value as FieldValueCollection;

                                string typeAsString = collection.TypeAsString;
                                if (string.IsNullOrEmpty(typeAsString))
                                {
                                    var firstElement = collection.Values.FirstOrDefault();
                                    if (firstElement is FieldLookupValue)
                                    {
                                        typeAsString = "LookupMulti";
                                    }
                                    else if (firstElement is FieldTaxonomyValue)
                                    {
                                        typeAsString = "TaxonomyFieldTypeMulti";
                                    }
                                }

                                if (typeAsString == "LookupMulti" || typeAsString == "Lookup" || typeAsString == "UserMulti")
                                {
                                    fieldValues.Append(SetArraySpecialFieldValueXml(changedProp.Key, changedProp.Value as FieldValueCollection, ref counter));
                                }
                                else if (typeAsString == "TaxonomyFieldTypeMulti")
                                {
                                    fieldValues.Append(SetManagedMetadataMultiValueXml(changedProp.Key, changedProp.Value as FieldValueCollection,
                                        taxonomyMultiValueObjectPaths, taxonomyMultiValueIdentities, ref counter, ref taxFieldObjectId, ref taxFieldIdentityObjectId));
                                }
                            }
                            else if (changedProp.Value is List<string>)
                            {
                                // multi value choice field
                                fieldValues.Append(SetArrayFieldValueXml(changedProp.Key, changedProp.Value as List<string>, ref counter));
                            }
                            else
                            {
                                // Let's set its value into the update message
                                fieldValues.Append(SetFieldValueXml(changedProp.Key, changedProp.Value, changedProp.Value?.GetType().Name, ref counter));
                            }
                        }
                    }
                    else
                    {
                        // Let's set its value into the update message
                        fieldValues.Append(SetFieldValueXml(changedField.SharePointName, GetValue(changedField.Name), changedField.DataType.Name, ref counter));
                    }
                }
            }

            // No changes, so bail out
            if (!changeFound)
            {
                return null;
            }

            // update field values
            xml = xml.Replace(CsomHelper.FieldValues, fieldValues.ToString());

            // update counter
            xml = xml.Replace(CsomHelper.Counter, counter.ToString());

            // replace the default taxonomy multi value field update placeholders
            xml = xml.Replace(CsomHelper.TaxonomyMultiValueIdentities, taxonomyMultiValueIdentities.ToString()); ;
            xml = xml.Replace(CsomHelper.TaxonomyMultiValueObjectPaths, taxonomyMultiValueObjectPaths.ToString());

            return xml;
        }

        private static string SetFieldValueAsNullXml(string fieldName, ref int counter)
        {
            string xml = CsomHelper.ListItemSystemUpdateSetFieldValueToNull;
            xml = xml.Replace(CsomHelper.Counter, counter.ToString());
            xml = xml.Replace(CsomHelper.FieldName, fieldName);

            counter++;
            return xml;
        }

        private static string SetManagedMetadataMultiValueXml(string fieldName, FieldValueCollection fieldValueCollection, 
            StringBuilder taxonomyMultiValueObjectPaths, StringBuilder taxonomyMultiValueIdentities,  
            ref int counter, ref int taxFieldObjectId, ref int taxFieldIdentityObjectId)
        {
            #region Sample XML snippets
            /*
            <Request AddExpandoFieldTypeSuffix="true" SchemaVersion="15.0.0.0" LibraryVersion="16.0.0.0" ApplicationName=".NET Library"
                xmlns="http://schemas.microsoft.com/sharepoint/clientquery/2009">
                <Actions>
                    <ObjectPath Id="24" ObjectPathId="23" />
                    <Method Name="PopulateFromLabelGuidPairs" Id="25" ObjectPathId="23">
                        <Parameters>
                            <Parameter Type="String">MBI|1824510b-00e1-40ac-8294-528b1c9421e0;LBI|ed5449ec-4a4f-4102-8f07-5a207c438571</Parameter>
                        </Parameters>
                    </Method>
                    <Method Name="SetFieldValue" Id="26" ObjectPathId="13" Version="50">
                        <Parameters>
                            <Parameter Type="String">MMMultiple</Parameter>
                            <Parameter ObjectPathId="23" />
                        </Parameters>
                    </Method>
                    <Method Name="UpdateOverwriteVersion" Id="27" ObjectPathId="13" Version="50" />
                </Actions>
                <ObjectPaths>
                    <Constructor Id="23" TypeId="{c3dfae10-f3bf-4894-9012-bb60665b6d91}">
                        <Parameters>
                            <Parameter Type="Null" />
                            <Parameter ObjectPathId="19" />
                        </Parameters>
                    </Constructor>
                    <Identity Id="13" Name="86e78d9f-e06b-2000-915f-2825d672e6ac|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:b56adf79-ff6a-4964-a63a-ff1fa23be9f8:web:8c8e101c-1b0d-4253-85e7-c30039bf46e2:list:7d644d41-d86e-4594-99de-479594a68fd9:item:1,1" />
                    <Identity Id="19" Name="86e78d9f-90e2-2000-915f-217ff0ac791d|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:b56adf79-ff6a-4964-a63a-ff1fa23be9f8:web:8c8e101c-1b0d-4253-85e7-c30039bf46e2:list:7d644d41-d86e-4594-99de-479594a68fd9:field:9295d18b-0742-47f8-b3af-ee73c8b2692e" />
                </ObjectPaths>
            </Request>
            */
            #endregion

            // <Actions> content
            string xml = CsomHelper.ListItemTaxonomyMultiValueFieldAction;
            xml = xml.Replace(CsomHelper.Counter, counter.ToString());
            counter++;
            xml = xml.Replace(CsomHelper.Counter2, counter.ToString());
            counter++;
            xml = xml.Replace(CsomHelper.Counter3, counter.ToString());
            xml = xml.Replace(CsomHelper.TaxFieldObjectId, taxFieldObjectId.ToString());
            xml = xml.Replace(CsomHelper.FieldName, fieldName);

            StringBuilder sb = new StringBuilder();
            foreach (var fieldValue in fieldValueCollection.Values)
            {
                sb.Append($"{(fieldValue as FieldTaxonomyValue).Label}|{(fieldValue as FieldTaxonomyValue).TermId};");
            }
            xml = xml.Replace(CsomHelper.FieldValue, sb.ToString().TrimEnd(';'));

            // <ObjectPath> constructors
            taxonomyMultiValueObjectPaths.Append(CsomHelper.ListItemTaxonomyMultiValueFieldObjectPath
                .Replace(CsomHelper.TaxFieldObjectId, taxFieldObjectId.ToString())
                .Replace(CsomHelper.TaxFieldIdentityObjectId, taxFieldIdentityObjectId.ToString())
                );

            // <Identity> nodes
            taxonomyMultiValueIdentities.Append(CsomHelper.ListItemTaxonomyMultiValueFieldIdentity
                .Replace(CsomHelper.TaxFieldIdentityObjectId, taxFieldIdentityObjectId.ToString())
                .Replace(CsomHelper.TaxonomyFieldId, fieldValueCollection.Field.Id.ToString())
                );

            // update counters and return
            counter++;
            taxFieldObjectId++;
            taxFieldIdentityObjectId++;
            return xml;
        }

        private static string SetSpecialFieldValueXml(string fieldName, FieldValue fieldValue, ref int counter)
        {
            #region Sample XML snippets
            /* Sample XML snippet for Url field
            <Method Name="SetFieldValue" Id="17" ObjectPathId="13" Version="8">
                <Parameters>
                    <Parameter Type="String">Url</Parameter>
                    <Parameter TypeId="{fa8b44af-7b43-43f2-904a-bd319497011e}">
                        <Property Name="Description" Type="String">fdmslkfqmsl</Property>
                        <Property Name="Url" Type="String">https://bla1</Property>
                    </Parameter>
                </Parameters>
            </Method>

            Sample snippet for a user field
            <Method Name="SetFieldValue" Id="17" ObjectPathId="13" Version="11">
                <Parameters>
                    <Parameter Type="String">PersonSingle</Parameter>
                    <Parameter TypeId="{c956ab54-16bd-4c18-89d2-996f57282a6f}">
                        <Property Name="Email" Type="Null" />
                        <Property Name="LookupId" Type="Int32">6</Property>
                        <Property Name="LookupValue" Type="Null" />
                    </Parameter>
                </Parameters>
            </Method>

            Taxonomy field
            <Method Name="SetFieldValue" Id="17" ObjectPathId="13" Version="40">
                <Parameters>
                    <Parameter Type="String">MMSingle</Parameter>
                    <Parameter TypeId="{19e70ed0-4177-456b-8156-015e4d163ff8}">
                        <Property Name="Label" Type="String">MBI</Property>
                        <Property Name="TermGuid" Type="String">1824510b-00e1-40ac-8294-528b1c9421e0</Property>
                        <Property Name="WssId" Type="Int32">-1</Property>
                    </Parameter>
                </Parameters>
            </Method>
            */
            #endregion

            string xml = CsomHelper.ListItemSpecialField;

            xml = xml.Replace(CsomHelper.Counter, counter.ToString());
            xml = xml.Replace(CsomHelper.ObjectId, $"{{{fieldValue.CsomType}}}");
            xml = xml.Replace(CsomHelper.FieldName, fieldName);
            xml = xml.Replace(CsomHelper.FieldValues, fieldValue.ToCsomXml());
            xml = xml.Replace(CsomHelper.FieldType, "String");

            counter++;
            return xml;
        }

        private static string SetArraySpecialFieldValueXml(string fieldName, FieldValueCollection fieldValueCollection, ref int counter)
        {
            #region Sample XML snippets
            /* 
            Sample snippet for an array of User fields    
            <Method Name="SetFieldValue" Id="18" ObjectPathId="13" Version="28">
                <Parameters>
                    <Parameter Type="String">PersonMultiple</Parameter>
                    <Parameter Type="Array">
                        <Object TypeId="{c956ab54-16bd-4c18-89d2-996f57282a6f}">
                            <Property Name="Email" Type="Null" />
                            <Property Name="LookupId" Type="Int32">15</Property>
                            <Property Name="LookupValue" Type="Null" />
                        </Object>
                        <Object TypeId="{c956ab54-16bd-4c18-89d2-996f57282a6f}">
                            <Property Name="Email" Type="Null" />
                            <Property Name="LookupId" Type="Int32">6</Property>
                            <Property Name="LookupValue" Type="Null" />
                        </Object>
                    </Parameter>
                </Parameters>
            </Method>
            */
            #endregion

            string xml = CsomHelper.ListItemSpecialArrayField;

            xml = xml.Replace(CsomHelper.Counter, counter.ToString());
            xml = xml.Replace(CsomHelper.FieldName, fieldName);

            StringBuilder sb = new StringBuilder();
            foreach (var fieldValue in fieldValueCollection.Values)
            {
                sb.Append(CsomHelper.ListItemSpecialArrayObject
                    .Replace(CsomHelper.ObjectId, $"{{{(fieldValue as FieldValue).CsomType}}}")
                    .Replace(CsomHelper.FieldValue, (fieldValue as FieldValue).ToCsomXml())
                    );
            }
            xml = xml.Replace(CsomHelper.ArrayValues, sb.ToString());

            counter++;
            return xml;


        }

        private static string SetArrayFieldValueXml(string fieldName, IEnumerable fieldValue, ref int counter)
        {
            #region Sample XML snippets
            /*
            <Method Name="SetFieldValue" Id="18" ObjectPathId="13" Version="26">
                <Parameters>
                    <Parameter Type="String">ChoiceMultiple</Parameter>
                    <Parameter Type="Array">
                        <Object Type="String">Choice 2</Object>
                        <Object Type="String">Choice 3</Object>
                    </Parameter>
                </Parameters>
            </Method>              
             */
            #endregion

            string xml = CsomHelper.ListItemSystemUpdateSetArrayFieldValue;
            xml = xml.Replace(CsomHelper.Counter, counter.ToString());
            xml = xml.Replace(CsomHelper.FieldName, fieldName);

            StringBuilder sb = new StringBuilder();
            foreach(var item in fieldValue)
            {
                sb.Append(CsomHelper.ListItemArrayFieldProperty
                    .Replace(CsomHelper.FieldType, item.GetType().Name))
                    .Replace(CsomHelper.FieldValue, CsomHelper.XmlString(item.ToString())
                    );
            }
            xml = xml.Replace(CsomHelper.ArrayValues, sb.ToString());

            counter++;
            return xml;
        }

        private static string SetFieldValueXml(string fieldName, object fieldValue, string fieldType, ref int counter)
        {
            #region Sample XML snippets
            /*
            <Method Name="SetFieldValue" Id="17" ObjectPathId="13" Version="26">
                <Parameters>
                    <Parameter Type="String">ChoiceSingle</Parameter>
                    <Parameter Type="String">Choice 2</Parameter>
                </Parameters>
            </Method>
             */
            #endregion

            string xml = CsomHelper.ListItemSystemUpdateSetFieldValue;

            xml = xml.Replace(CsomHelper.Counter, counter.ToString());
            xml = xml.Replace(CsomHelper.FieldName, fieldName);
            xml = xml.Replace(CsomHelper.FieldValue, fieldValue == null ? "" : CsomHelper.XmlString(TypeSpecificHandling(fieldValue, fieldType), false));
            xml = xml.Replace(CsomHelper.FieldType, fieldType ?? "Null");

            counter++;
            return xml;
        }

        private static string TypeSpecificHandling(object value, string fieldType)
        {
            if (!string.IsNullOrEmpty(fieldType))
            {
                if (fieldType.Equals("Boolean"))
                {
                    return value.ToString().ToLowerInvariant();
                }
                else if (fieldType.Equals("DateTime"))
                {
                    if (value is DateTime time)
                    {
                        return time.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffzzz"); 
                    }
                }
                else
                {
                    return value.ToString();
                }
            }

            return value.ToString();
        }
        #endregion

        #region Comment handling

        public bool AreCommentsDisabled()
        {
            return AreCommentsDisabledAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> AreCommentsDisabledAsync()
        {
            var apiCall = new ApiCall("_api/web/lists/getbyid(guid'{Parent.Id}')/items({Id})/CommentsDisabled", ApiType.SPORest);

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                var json = JsonDocument.Parse(response.Json).RootElement.GetProperty("d");

                if (json.TryGetProperty("CommentsDisabled", out JsonElement commentsDisabled))
                {
                    return commentsDisabled.GetBoolean();
                }
            }

            return false;
        }

        public void SetCommentsDisabled(bool commentsDisabled)
        {
            SetCommentsDisabledAsync(commentsDisabled).GetAwaiter().GetResult();
        }

        public async Task SetCommentsDisabledAsync(bool commentsDisabled)
        {
            // Build the API call to ensure this graph user as a SharePoint User in this site collection
            var parameters = new
            {
                value = commentsDisabled
            };

            string body = JsonSerializer.Serialize(parameters);

            var apiCall = new ApiCall("_api/web/lists/getbyid(guid'{Parent.Id}')/items({Id})/SetCommentsDisabled", ApiType.SPORest, body);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);            
        }

        #endregion

        #endregion

    }
}
