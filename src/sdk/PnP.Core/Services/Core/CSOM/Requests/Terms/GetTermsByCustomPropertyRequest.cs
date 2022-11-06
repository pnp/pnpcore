using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Services.Core.CSOM.Requests.Terms
{
    internal class GetTermsByCustomPropertyRequest : IRequest<List<Guid>>
    {
        private readonly string key;
        private readonly string value;
        private readonly bool trimUnavailable;
        private readonly string termSetId;
        private readonly string termGroupId;

        public GetTermsByCustomPropertyRequest(
            string key, 
            string value, 
            bool trimUnavailable,
            string termSetId,
            string termGroupId)
        {
            this.key = key;
            this.value = value;
            this.trimUnavailable = trimUnavailable;
            this.termSetId = termSetId;
            this.termGroupId = termGroupId;
        }

        public List<Guid> Result { get; } = new List<Guid>();

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        internal int IdentityPath { get; set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            List<ActionObjectPath> result = new List<ActionObjectPath>();

            #region XML Payload generated

            // todo

            #endregion

            #region GetTaxonomySession

            var staticMethodPathGetTaxonomySession = new StaticMethodPath
            {
                Id = idProvider.GetActionId(),
                Name = "GetTaxonomySession",
                TypeId = "{981cbc68-9edc-4f8d-872f-71146fcbb84f}",
                Parameters = new MethodParameter {Properties = new List<Parameter>()}
            };

            ActionObjectPath baseActionPath = new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = staticMethodPathGetTaxonomySession.Id.ToString()
                },
                ObjectPath = staticMethodPathGetTaxonomySession
            };

            result.Add(baseActionPath);

            ActionObjectPath baseIdentityQuery = new ActionObjectPath()
            {
                Action = new IdentityQueryAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = staticMethodPathGetTaxonomySession.Id.ToString()
                }
            };

            result.Add(baseIdentityQuery);

            #endregion

            #region GetDefaultSiteCollectionTermStore

            var objectPathMethodGetDefaultSiteCollectionTermStore = new ObjectPathMethod
            {
                Id = idProvider.GetActionId(),
                ParentId = staticMethodPathGetTaxonomySession.Id,
                Name = "GetDefaultSiteCollectionTermStore",
                Parameters = new MethodParameter {Properties = new List<Parameter>()}
            };

            ActionObjectPath getDefaultSiteCollectionTermStoreActionPath = new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = objectPathMethodGetDefaultSiteCollectionTermStore.Id.ToString()
                },
                ObjectPath = objectPathMethodGetDefaultSiteCollectionTermStore
            };

            result.Add(getDefaultSiteCollectionTermStoreActionPath);

            ActionObjectPath getDefaultSiteCollectionTermStoreIdentityQuery = new ActionObjectPath()
            {
                Action = new IdentityQueryAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = objectPathMethodGetDefaultSiteCollectionTermStore.Id.ToString()
                }
            };

            result.Add(getDefaultSiteCollectionTermStoreIdentityQuery);

            #endregion

            #region GROUPS

            var objectPathPropertyGroups = new Property
            {
                Id = idProvider.GetActionId(),
                ParentId = objectPathMethodGetDefaultSiteCollectionTermStore.Id,
                Name = "Groups"
            };

            ActionObjectPath groupsActionPath = new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(), ObjectPathId = objectPathPropertyGroups.Id.ToString()
                },
                ObjectPath = objectPathPropertyGroups
            };

            result.Add(groupsActionPath);

            #endregion

            #region GROUPS GetById

            var objectPathMethodGroupsGetById = new ObjectPathMethod
            {
                Id = idProvider.GetActionId(),
                ParentId = objectPathPropertyGroups.Id,
                Name = "GetById",
                Parameters = new MethodParameter()
                {
                    Properties = new List<Parameter>()
                    {
                        new Parameter() {Type = "String", Value = this.termGroupId}
                    }
                }
            };

            ActionObjectPath methodGroupsGetByIdActionPath = new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(), ObjectPathId = objectPathMethodGroupsGetById.Id.ToString()
                },
                ObjectPath = objectPathMethodGroupsGetById
            };

            result.Add(methodGroupsGetByIdActionPath);


            ActionObjectPath methodGroupsGetByIdActionPathIdentityQuery = new ActionObjectPath()
            {
                Action = new IdentityQueryAction()
                {
                    Id = idProvider.GetActionId(), ObjectPathId = objectPathMethodGroupsGetById.Id.ToString()
                }
            };

            result.Add(methodGroupsGetByIdActionPathIdentityQuery);

            #endregion

            #region TERMSETS

            var objectPathPropertyTermSets = new Property
            {
                Id = idProvider.GetActionId(), ParentId = objectPathMethodGroupsGetById.Id, Name = "TermSets"
            };

            ActionObjectPath termSetsActionPath = new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(), ObjectPathId = objectPathPropertyTermSets.Id.ToString()
                },
                ObjectPath = objectPathPropertyTermSets
            };

            result.Add(termSetsActionPath);

            #endregion

            #region TERMSETS GetById

            var objectPathMethodTermSetsGetById = new ObjectPathMethod
            {
                Id = idProvider.GetActionId(),
                ParentId = objectPathPropertyTermSets.Id,
                Name = "GetById",
                Parameters = new MethodParameter()
                {
                    Properties = new List<Parameter>()
                    {
                        new Parameter() {Type = "String", Value = this.termSetId}
                    }
                }
            };

            ActionObjectPath methodTermSetsGetByIdActionPath = new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(), ObjectPathId = objectPathMethodTermSetsGetById.Id.ToString()
                },
                ObjectPath = objectPathMethodTermSetsGetById
            };

            result.Add(methodTermSetsGetByIdActionPath);

            ActionObjectPath methodTermSetsGetByIdActionPathIdentityQuery = new ActionObjectPath()
            {
                Action = new IdentityQueryAction()
                {
                    Id = idProvider.GetActionId(), ObjectPathId = objectPathMethodTermSetsGetById.Id.ToString()
                }
            };

            result.Add(methodTermSetsGetByIdActionPathIdentityQuery);

            #endregion

            #region CustomPropertyMatchInformationConstructor

            ConstructorPath customPropertyMatchInformationConstructorPath = new ConstructorPath
            {
                Id = idProvider.GetActionId(),
                TypeId = "{56747951-df44-4bed-bf36-2b3bddf587f9}",
                Parameters = new MethodParameter {Properties = new List<Parameter>()}
            };

            ActionObjectPath customPropertyMatchInformationConstructorActionPath = new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = customPropertyMatchInformationConstructorPath.Id.ToString()
                },
                ObjectPath = customPropertyMatchInformationConstructorPath
            };

            result.Add(customPropertyMatchInformationConstructorActionPath);

            #endregion

            #region CustomPropertyMatchInformationConstructor Properties

            ActionObjectPath customPropertyKeyObjectPath = new ActionObjectPath()
            {
                Action = new SetPropertyAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = customPropertyMatchInformationConstructorPath.Id.ToString(),
                    Name = "CustomPropertyName",
                    SetParameter = new Parameter() {Type = "String", Value = key}
                }
            };

            result.Add(customPropertyKeyObjectPath);

            ActionObjectPath customPropertyValueObjectPath = new ActionObjectPath()
            {
                Action = new SetPropertyAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = customPropertyMatchInformationConstructorPath.Id.ToString(),
                    Name = "CustomPropertyValue",
                    SetParameter = new Parameter() {Type = "String", Value = value}
                }
            };

            result.Add(customPropertyValueObjectPath);

            ActionObjectPath customPropertyTrimUnavailableObjectPath = new ActionObjectPath()
            {
                Action = new SetPropertyAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = customPropertyMatchInformationConstructorPath.Id.ToString(),
                    Name = "TrimUnavailable",
                    SetParameter = new Parameter() {Type = "Boolean", Value = trimUnavailable}
                }
            };

            result.Add(customPropertyTrimUnavailableObjectPath);

            #endregion

            #region GetTermsWithCustomProperty

            var objectPathMethodGetTermsWithCustomProperty = new ObjectPathMethod
            {
                Id = idProvider.GetActionId(),
                ParentId = objectPathMethodTermSetsGetById.Id,
                Name = "GetTermsWithCustomProperty",
                Parameters = new MethodParameter
                {
                    Properties = new List<Parameter>()
                    {
                        new ObjectReferenceParameter()
                        {
                            ObjectPathId = customPropertyMatchInformationConstructorPath.Id,
                        }
                    }
                }
            };

            ActionObjectPath identityQueryBaseAction = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = objectPathMethodGetTermsWithCustomProperty.Id.ToString()
                }
            };

            result.Add(identityQueryBaseAction);

            IdentityPath = idProvider.GetActionId();

            ActionObjectPath identityQuery = new ActionObjectPath()
            {
                ObjectPath = objectPathMethodGetTermsWithCustomProperty,
                Action = new PnP.Core.Services.Core.CSOM.QueryAction.QueryAction()
                {
                    Id = IdentityPath,
                    ObjectPathId = objectPathMethodGetTermsWithCustomProperty.Id.ToString(),
                    SelectQuery = new SelectQuery()
                    {
                        SelectAllProperties = true, Properties = new List<Property>()
                    },
                    ChildItemQuery = new ChildItemQuery() {SelectAllProperties = true}
                },
            };
            result.Add(identityQuery);

            #endregion

            return result;
        }

        public void ProcessResponse(string response)
        {
            List<JsonElement> results = JsonSerializer.Deserialize<List<JsonElement>>(response,
                PnPConstants.JsonSerializer_SPGuidConverter_DateTimeConverter);

            if (results == null) return;

            int idIndex = results.FindIndex(r => CompareIdElement(r, IdentityPath));

            if (idIndex < 0) return;

            JsonElement result = results[idIndex + 1];
            result.TryGetProperty("_Child_Items_", out JsonElement childItemsProperty);

            List<JsonElement> childItems = JsonSerializer.Deserialize<List<JsonElement>>(
                childItemsProperty.GetRawText(), PnPConstants.JsonSerializer_SPGuidConverter_DateTimeConverter);

            if (childItems == null) return;

            foreach (JsonElement jsonElement in childItems)
            {
                jsonElement.TryGetProperty("Id", out JsonElement termGuidProperty);

                Guid.TryParse(termGuidProperty.GetString()?.Replace("/Guid(", "").Replace(")/", ""),
                    out Guid termGuid);
                Result.Add(termGuid);
            }
        }
        
        // todo: code duplication
        private static bool CompareIdElement(JsonElement element, long id)
        {
            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out long elementId))
            {
                return elementId == id;
            }

            return false;
        }
    }
}


//```xml
//<Request xmlns="http://schemas.microsoft.com/sharepoint/clientquery/2009" SchemaVersion="15.0.0.0" LibraryVersion="16.0.0.0" ApplicationName="Javascript Library">
//    <Actions>
//        <ObjectPath Id="1" ObjectPathId="0" />
//        <ObjectIdentityQuery Id="2" ObjectPathId="0" />
//        <ObjectPath Id="4" ObjectPathId="3" />
//        <ObjectIdentityQuery Id="5" ObjectPathId="3" />
//        <ObjectPath Id="7" ObjectPathId="6" />
//        <ObjectPath Id="9" ObjectPathId="8" />
//        <ObjectIdentityQuery Id="10" ObjectPathId="8" />
//        <ObjectPath Id="12" ObjectPathId="11" />
//        <ObjectPath Id="14" ObjectPathId="13" />
//        <ObjectIdentityQuery Id="15" ObjectPathId="13" />
//        <ObjectPath Id="17" ObjectPathId="16" />
//        <SetProperty Id="18" ObjectPathId="16" Name="CustomPropertyName">
//            <Parameter Type="String">Key1</Parameter>
//        </SetProperty>
//        <SetProperty Id="19" ObjectPathId="16" Name="CustomPropertyValue">
//            <Parameter Type="String">Value1</Parameter>
//        </SetProperty >
//        <SetProperty Id="20" ObjectPathId="16" Name="TrimUnavailable">
//            <Parameter Type="Boolean">true</Parameter>
//        </SetProperty>
//        <ObjectPath Id="22" ObjectPathId="21" />
//        <Query Id="23" ObjectPathId="21">
//            <Query SelectAllProperties = "true" >
//                <Properties />
//            </Query>
//            <ChildItemQuery SelectAllProperties="true">
//                <Properties />
//            </ChildItemQuery>
//        </Query>
//    </Actions>
//    <ObjectPaths>
//        <StaticMethod Id="0" Name="GetTaxonomySession" TypeId="{981cbc68-9edc-4f8d-872f-71146fcbb84f}" />
//        <Method Id="3" ParentId="0" Name="GetDefaultSiteCollectionTermStore" />
//        <Property Id="6" ParentId="3" Name="Groups" />
//        <Method Id="8" ParentId="6" Name="GetByName">
//            <Parameters>
//                <Parameter Type="String">People</Parameter>
//            </Parameters>
//        </Method>
//        <Property Id="11" ParentId="8" Name="TermSets" />
//        <Method Id="13" ParentId="11" Name="GetByName">
//            <Parameters>
//                <Parameter Type="String">Department</Parameter>
//            </Parameters>
//        </Method>
//        <Constructor Id="16" TypeId="{56747951-df44-4bed-bf36-2b3bddf587f9}" />
//        <Method Id="21" ParentId="13" Name="GetTermsWithCustomProperty">
//            <Parameters>
//                <Parameter ObjectPathId="16" />
//            </Parameters>
//        </Method>
//    </ObjectPaths>
//</Request>
//```