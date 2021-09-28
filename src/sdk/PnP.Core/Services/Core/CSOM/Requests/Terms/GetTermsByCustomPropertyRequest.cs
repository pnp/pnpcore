using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.Requests.Terms
{
    internal class GetTermsByCustomPropertyRequest : IRequest<object>
    {
        private readonly string key;
        private readonly string value;
        private readonly bool trimUnavailable;

        public GetTermsByCustomPropertyRequest(string key, string value, bool trimUnavailable)
        {
            this.key = key;
            this.value = value;
            this.trimUnavailable = trimUnavailable;
        }
        public object Result { get; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            List<ActionObjectPath> result = new List<ActionObjectPath>();

            var staticMethodPathGetTaxonomySession = new StaticMethodPath
            {
                Id = idProvider.GetActionId(),
                Name = "GetTaxonomySession",
                TypeId = "{981cbc68-9edc-4f8d-872f-71146fcbb84f}",
                Parameters = new MethodParameter
                {
                    Properties = new List<Parameter>()
                }
            };

            var objectPathMethodGetDefaultSiteCollectionTermStore = new ObjectPathMethod
            {
                Id = idProvider.GetActionId(),
                ParentId = staticMethodPathGetTaxonomySession.Id,
                Name = "GetDefaultSiteCollectionTermStore",
                Parameters = new MethodParameter
                {
                    Properties = new List<Parameter>()
                }
            };

            var objectPathPropertyGroups = new Property
            {
                Id = idProvider.GetActionId(),
                ParentId = objectPathMethodGetDefaultSiteCollectionTermStore.Id,
                Name = "Groups"
            };

            var objectPathMethodGroupsGetByName = new ObjectPathMethod
            {
                Id = idProvider.GetActionId(),
                ParentId = objectPathPropertyGroups.Id,
                Name = "GetByName",
                Parameters = new MethodParameter()
                {
                    Properties = new List<Parameter>() {
                        new Parameter()
                        {
                            Type = "String",
                            Value = "People"
                        }
                    }
                }
            };

            var objectPathPropertyTermSets = new Property
            {
                Id = 11,
                ParentId = 8,
                Name = "TermSets"
            };

            var objectPathMethodTermSetsGetByName = new ObjectPathMethod
            {
                Id = 13,
                ParentId = 11,
                Name = "GetByName",
                Parameters = new MethodParameter()
                {
                    Properties = new List<Parameter>() {
                        new Parameter()
                        {
                            Type = "String",
                            Value = "Department"
                        }
                    }
                }
            };

            ConstructorPath customPropertyMatchInformationConstructorPath = new ConstructorPath
            {
                Id = 16,
                TypeId = "{56747951-df44-4bed-bf36-2b3bddf587f9}"
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

            ActionObjectPath identityQuery = new ActionObjectPath()
            {
                Action = new IdentityQueryAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = staticMethodPathGetTaxonomySession.Id.ToString()
                }
            };

            result.Add(identityQuery);

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

            ActionObjectPath propertyGroupsActionPath = new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = objectPathPropertyGroups.Id.ToString()
                },
                ObjectPath = objectPathPropertyGroups
            };

            result.Add(propertyGroupsActionPath);

            ActionObjectPath methodGroupsGetByNameActionPath = new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = objectPathMethodGroupsGetByName.Id.ToString()
                },
                ObjectPath = objectPathMethodGroupsGetByName
            };

            result.Add(methodGroupsGetByNameActionPath);


            ActionObjectPath methodGroupsGetByNameActionPathIdentityQuery = new ActionObjectPath()
            {
                Action = new IdentityQueryAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = objectPathMethodGroupsGetByName.Id.ToString()
                }
            };

            result.Add(methodGroupsGetByNameActionPathIdentityQuery);





            ActionObjectPath customPropertyKeyObjectPath = new ActionObjectPath()
            {
                Action = new SetPropertyAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = customPropertyMatchInformationConstructorPath.Id.ToString(),
                    Name = "CustomPropertyName",
                    SetParameter = new Parameter()
                    {
                        Type = "String",
                        Value = key
                    }
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
                    SetParameter = new Parameter()
                    {
                        Type = "String",
                        Value = value
                    }
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
                    SetParameter = new Parameter()
                    {
                        Type = "Boolean",
                        Value = trimUnavailable
                    }
                }
            };

            result.Add(customPropertyTrimUnavailableObjectPath);

            return result;
        }

        public void ProcessResponse(string response)
        {
            throw new NotImplementedException();
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
