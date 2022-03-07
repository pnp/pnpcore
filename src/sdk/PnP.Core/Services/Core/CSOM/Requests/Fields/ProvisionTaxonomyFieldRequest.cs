using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.Requests.Fields
{
    internal sealed class ProvisionTaxonomyFieldRequest : IRequest<object>
    {
        public object Result { get; set; }

        public string ParentId { get; set; }

        public string SiteId { get; set; }

        public string FieldId { get; set; }

        public string WebId { get; set; }

        public Guid TermStoreId { get; set; }

        public Guid TermSetId { get; set; }

        public bool Open { get; set; }

        public ProvisionTaxonomyFieldRequest(string siteId, string webId, string fieldId, string parentId, Guid termStoreId, Guid termSetId, bool open)
        {
            SiteId = siteId;
            WebId = webId;
            FieldId = fieldId;
            ParentId = parentId;
            TermStoreId = termStoreId;
            TermSetId = termSetId;
            Open = open;
        }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            string identityObjectName = GenerateIdentityName();
            Identity identity = new Identity()
            {
                Name = identityObjectName,
                Id = idProvider.GetActionId()
            };

            List<ActionObjectPath> result = new List<ActionObjectPath>();
            //Set term store id
            ActionObjectPath setTermStoreId = new ActionObjectPath()
            {
                Action = new SetPropertyAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = identity.Id.ToString(),
                    Name = "SspId",
                    SetParameter = new Parameter()
                    {
                        Type = "Guid",
                        Value = TermStoreId
                    }
                }
            };
            result.Add(setTermStoreId);

            //Set term set id
            ActionObjectPath setTermSetId = new ActionObjectPath()
            {
                Action = new SetPropertyAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = identity.Id.ToString(),
                    Name = "TermSetId",
                    SetParameter = new Parameter()
                    {
                        Type = "Guid",
                        Value = TermSetId
                    }
                }
            };
            result.Add(setTermSetId);

            //Set target template
            ActionObjectPath setTargetTemplate = new ActionObjectPath()
            {
                Action = new SetPropertyAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = identity.Id.ToString(),
                    Name = "TargetTemplate",
                    SetParameter = new Parameter()
                    {
                        Type = "String",
                        Value = string.Empty
                    }
                }
            };
            result.Add(setTargetTemplate);

            //Set open
            ActionObjectPath open = new ActionObjectPath()
            {
                Action = new SetPropertyAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = identity.Id.ToString(),
                    Name = "Open",
                    SetParameter = new Parameter()
                    {
                        Type = "Boolean",
                        Value = Open
                    }
                }
            };
            result.Add(open);

            //Set AnchorId
            ActionObjectPath setAnchorId = new ActionObjectPath()
            {
                Action = new SetPropertyAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = identity.Id.ToString(),
                    Name = "AnchorId",
                    SetParameter = new Parameter()
                    {
                        Type = "Guid",
                        Value = Guid.Empty
                    }
                }
            };
            result.Add(setAnchorId);

            //Call update method
            ActionObjectPath updateMethod = new ActionObjectPath()
            {
                Action = new MethodAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = identity.Id.ToString(),
                    Name = "Update"
                },
                ObjectPath = identity
            };
            result.Add(updateMethod);

            return result;
        }

        private string GenerateIdentityName()
        {
            string identityObjectName = $"1e1a939f-60b2-2000-98a6-d25d3d400a3a|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{SiteId}:web:{WebId}";
            if (!string.IsNullOrEmpty(ParentId))
            {
                identityObjectName += ":list:" + ParentId;
            }
            identityObjectName += ":field:" + FieldId;
            return identityObjectName;
        }

        public void ProcessResponse(string response)
        {
        }
    }
}
