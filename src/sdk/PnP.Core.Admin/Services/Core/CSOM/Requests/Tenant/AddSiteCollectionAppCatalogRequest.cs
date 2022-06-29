using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant
{
    internal sealed class AddSiteCollectionAppCatalogRequest : IRequest<object>
    {
        internal Uri SiteCollection { get; private set; }

        internal AddSiteCollectionAppCatalogRequest(Uri siteCollection)
        {
            SiteCollection = siteCollection;
        }

        public object Result { get; set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            List<ActionObjectPath> result = new List<ActionObjectPath>();

            // Object Paths
            var constructor1Id = idProvider.GetActionId();
            ActionObjectPath path1 = new ActionObjectPath()
            {
                ObjectPath = new ConstructorPath()
                {
                    Id = constructor1Id,
                    TypeId = "{268004ae-ef6b-4e9b-8425-127220d84719}"
                }
            };

            var getSiteByUrlMethodId = idProvider.GetActionId();
            ActionObjectPath path2 = new ActionObjectPath()
            {
                ObjectPath = new ObjectPathMethod()
                {
                    ParentId = constructor1Id,
                    Id = getSiteByUrlMethodId,
                    Name = "GetSiteByUrl",
                    Parameters = new MethodParameter()
                    {
                        Properties = new List<Parameter>() {
                            new Parameter()
                            {
                                Type = "String",
                                Value = SiteCollection.ToString()
                            }
                        }
                    }
                }
            };

            var rootWebPropertyId = idProvider.GetActionId();
            ActionObjectPath path3 = new ActionObjectPath()
            {
                ObjectPath = new Property
                {
                    Id = rootWebPropertyId,
                    ParentId = getSiteByUrlMethodId,
                    Name = "RootWeb"
                }
            };

            var tenantAppCatalogPropertyId = idProvider.GetActionId();
            ActionObjectPath path4 = new ActionObjectPath()
            {
                ObjectPath = new Property
                {
                    Id = tenantAppCatalogPropertyId,
                    ParentId = rootWebPropertyId,
                    Name = "TenantAppCatalog"
                }
            };

            var siteCollectionAppCatalogsSitesPropertyId = idProvider.GetActionId();
            ActionObjectPath path5 = new ActionObjectPath()
            {
                ObjectPath = new Property
                {
                    Id = siteCollectionAppCatalogsSitesPropertyId,
                    ParentId = tenantAppCatalogPropertyId,
                    Name = "SiteCollectionAppCatalogsSites"
                }
            };

            var addMethodId = idProvider.GetActionId();
            ActionObjectPath path6 = new ActionObjectPath()
            {
                ObjectPath = new ObjectPathMethod()
                {
                    ParentId = siteCollectionAppCatalogsSitesPropertyId,
                    Id = addMethodId,
                    Name = "Add",
                    Parameters = new MethodParameter()
                    {
                        Properties = new List<Parameter>() {
                            new Parameter()
                            {
                                Type = "String",
                                Value = SiteCollection.ToString()
                            }
                        }
                    }
                }
            };

            // Actions
            ActionObjectPath path7 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = constructor1Id.ToString()
                },
            };
            result.Add(path7);

            ActionObjectPath path8 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = getSiteByUrlMethodId.ToString()
                },
            };
            result.Add(path8);

            ActionObjectPath path9 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = rootWebPropertyId.ToString()
                },
            };
            result.Add(path9);

            ActionObjectPath path10 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = tenantAppCatalogPropertyId.ToString()
                },
            };
            result.Add(path10);

            ActionObjectPath path11 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = siteCollectionAppCatalogsSitesPropertyId.ToString()
                },
            };
            result.Add(path11);

            ActionObjectPath path12 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = addMethodId.ToString()
                },
            };
            result.Add(path12);

            result.Add(path1);
            result.Add(path2);
            result.Add(path3);
            result.Add(path4);
            result.Add(path5);
            result.Add(path6);

            return result;
        }

        public void ProcessResponse(string response)
        {
        }
    }
}
