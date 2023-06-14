using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.DateHelpers;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Xml.Linq;

namespace PnP.Core.Services.Core.CSOM.Requests.Terms
{
    internal class GetParentOfTermRequest : IRequest<Term>
    {
        private readonly Guid termId;
        private readonly PnPContext context;

        public GetParentOfTermRequest(PnPContext context, Guid termId)
        {
            this.context = context;
            this.termId = termId; 
        }

        public Term Result { get; set; }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        internal int GetTermIdentityPath { get; set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            List<ActionObjectPath> result = new List<ActionObjectPath>();

            #region XML Payload generated
            /*
            <Request AddExpandoFieldTypeSuffix="true" SchemaVersion="15.0.0.0" LibraryVersion="16.0.0.0" ApplicationName=".NET Library"
                xmlns="http://schemas.microsoft.com/sharepoint/clientquery/2009">
                <Actions>
                    <ObjectPath Id="2" ObjectPathId="1" />
                    <ObjectIdentityQuery Id="3" ObjectPathId="1" />
                    <ObjectPath Id="5" ObjectPathId="4" />
                    <ObjectIdentityQuery Id="6" ObjectPathId="4" />
                    <ObjectPath Id="8" ObjectPathId="7" />
                    <ObjectIdentityQuery Id="9" ObjectPathId="7" />
                    <Query Id="10" ObjectPathId="1">
                        <Query SelectAllProperties="true">
                            <Properties />
                        </Query>
                    </Query>
                    <Query Id="11" ObjectPathId="4">
                        <Query SelectAllProperties="true">
                            <Properties />
                        </Query>
                    </Query>
                    <Query Id="12" ObjectPathId="7">
                        <Query SelectAllProperties="false">
                            <Properties>
                                <Property Name="Id" ScalarProperty="true" />
                                <Property Name="Parent" SelectAll="true">
                                    <Query SelectAllProperties="false">
                                        <Properties />
                                    </Query>
                                </Property>
                            </Properties>
                        </Query>
                    </Query>
                </Actions>
                <ObjectPaths>
                    <StaticMethod Id="1" Name="GetTaxonomySession" TypeId="{981cbc68-9edc-4f8d-872f-71146fcbb84f}" />
                    <Method Id="4" ParentId="1" Name="GetDefaultSiteCollectionTermStore" />
                    <Method Id="7" ParentId="4" Name="GetTerm">
                        <Parameters>
                            <Parameter Type="Guid">{3c533617-3706-4f75-8a08-8ba636f3efea}</Parameter>
                        </Parameters>
                    </Method>
                </ObjectPaths>
            </Request>
            */
            #endregion

            #region GetTaxonomySession

            var staticMethodPathGetTaxonomySession = new StaticMethodPath
            {
                Id = idProvider.GetActionId(),
                Name = "GetTaxonomySession",
                TypeId = "{981cbc68-9edc-4f8d-872f-71146fcbb84f}",
                Parameters = new MethodParameter { Properties = new List<Parameter>() }
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

            ActionObjectPath identityQuery = new ActionObjectPath()
            {
                Action = new QueryAction.QueryAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = staticMethodPathGetTaxonomySession.Id.ToString(),
                    SelectQuery = new SelectQuery()
                    {
                        SelectAllProperties = true,
                        Properties = new List<Property>()
                    }
                },
            };

            result.Add(identityQuery);

            #endregion

            #region GetDefaultSiteCollectionTermStore

            var objectPathMethodGetDefaultSiteCollectionTermStore = new ObjectPathMethod
            {
                Id = idProvider.GetActionId(),
                ParentId = staticMethodPathGetTaxonomySession.Id,
                Name = "GetDefaultSiteCollectionTermStore",
                Parameters = new MethodParameter { Properties = new List<Parameter>() }
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

            ActionObjectPath getDefaultSiteCollectionTermStoreQuery = new ActionObjectPath()
            {
                Action = new QueryAction.QueryAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = objectPathMethodGetDefaultSiteCollectionTermStore.Id.ToString(),
                    SelectQuery = new SelectQuery()
                    {
                        SelectAllProperties = true,
                        Properties = new List<Property>()
                    }
                },
            };

            result.Add(getDefaultSiteCollectionTermStoreQuery);

            #endregion

            #region GetTerm

            var objectPathMethodGetTerm = new ObjectPathMethod
            {
                Id = idProvider.GetActionId(),
                ParentId = objectPathMethodGetDefaultSiteCollectionTermStore.Id,
                Name = "GetTerm",
                Parameters = new MethodParameter()
                {
                    Properties = new List<Parameter>()
                    {
                        new Parameter() {Type = "Guid", Value = termId}
                    }
                }
            };

            ActionObjectPath methodGroupsGetByIdActionPath = new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(), ObjectPathId = objectPathMethodGetTerm.Id.ToString()
                },
                ObjectPath = objectPathMethodGetTerm
            };

            result.Add(methodGroupsGetByIdActionPath);


            ActionObjectPath methodGroupsGetByIdActionPathIdentityQuery = new ActionObjectPath()
            {
                Action = new IdentityQueryAction()
                {
                    Id = idProvider.GetActionId(), ObjectPathId = objectPathMethodGetTerm.Id.ToString()
                }
            };

            result.Add(methodGroupsGetByIdActionPathIdentityQuery);

            ActionObjectPath methodGetTermActionPathQuery = new ActionObjectPath()
            {
                Action = new QueryAction.QueryAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = objectPathMethodGetTerm.Id.ToString(),
                    SelectQuery = new SelectQuery()
                    {
                        SelectAllProperties = false,
                        Properties = new List<Property>()
                        {
                            new Property { Name="Id" },
                            new QueryProperty 
                            { 
                                Name="Parent", 
                                SelectAll=true, 
                                Query = new SelectQuery()
                                {
                                    SelectAllProperties = false,
                                    Properties = new List<Property>()
                                },
                            }
                        }
                    }
                },
            };

            GetTermIdentityPath = methodGetTermActionPathQuery.Action.Id;

            result.Add(methodGetTermActionPathQuery);

            #endregion

            return result;
        }

        public void ProcessResponse(string response)
        {
            JsonElement resultItem = ResponseHelper.ProcessResponse<JsonElement>(response, GetTermIdentityPath);

            if (resultItem.TryGetProperty("Parent", out JsonElement parent))
            {
                if (parent.ValueKind == JsonValueKind.Object)
                {
                    // This is a child term, so let's return an ITerm instance
                    var term = new Term()
                    {
                        PnPContext = context,                        
                    };

                    if (parent.TryGetProperty("Id", out JsonElement termIdProperty))
                    {
                        if (termIdProperty.GetString() != null && Guid.TryParse(termIdProperty.GetString().Replace("/Guid(", "").Replace(")/", ""), out Guid termIdString))
                        {
                            term.SetSystemProperty(p => p.Id, termIdString.ToString());
                            term.Metadata.Add(PnPConstants.MetaDataGraphId, termIdString.ToString());
                        }                        
                    }

                    var dateConverter = new CSOMDateConverter();
                    if (parent.TryGetProperty("CreatedDate", out JsonElement createdDateProperty))
                    {
                        term.SetSystemProperty(p => p.CreatedDateTime, dateConverter.ConverDate(createdDateProperty.GetString()));
                    }

                    if (parent.TryGetProperty("LastModifiedDate", out JsonElement lastModifiedDateProperty))
                    {
                        term.SetSystemProperty(p => p.LastModifiedDateTime, dateConverter.ConverDate(lastModifiedDateProperty.GetString()));
                    }

                    // Ensure the default language was loaded
                    context.TermStore.EnsurePropertiesAsync(p => p.DefaultLanguage);

                    if (parent.TryGetProperty("Name", out JsonElement nameProperty))
                    {
                        (term.Labels as TermLocalizedLabelCollection).Add(new TermLocalizedLabel() { Name = nameProperty.GetString(), LanguageTag = context.TermStore.DefaultLanguage, IsDefault = true });
                    }

                    if (parent.TryGetProperty("Description", out JsonElement descriptionProperty))
                    {
                        if (!string.IsNullOrEmpty(descriptionProperty.GetString()))
                        {
                            (term.Descriptions as TermLocalizedDescriptionCollection).Add(new TermLocalizedDescription() { Description = descriptionProperty.GetString(), LanguageTag = context.TermStore.DefaultLanguage });
                        }
                    }

                    term.Requested = true;

                    Result = term;
                }
                else
                {
                    // Null means that the term is a root term, it's parent is a termset
                }
            }
        }
    }
}
