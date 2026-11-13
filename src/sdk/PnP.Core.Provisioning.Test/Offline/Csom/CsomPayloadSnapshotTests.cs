using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Audit;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.ContentTypes;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.InformationPolicy;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Lists;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Publishing;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Taxonomy;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Tenant;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.UserResources;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Web;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.WebParts;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Workflows;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PnP.Core.Provisioning.Test.Offline.Csom
{
    [TestClass]
    public class CsomPayloadSnapshotTests
    {
        private static readonly Guid SiteId = new Guid("11111111-1111-1111-1111-111111111111");
        private static readonly Guid WebId = new Guid("22222222-2222-2222-2222-222222222222");
        private static readonly Guid ListId = new Guid("33333333-3333-3333-3333-333333333333");
        private static readonly Guid FieldId = new Guid("44444444-4444-4444-4444-444444444444");
        private static readonly Guid TermId = new Guid("55555555-5555-5555-5555-555555555555");
        private static readonly Guid ChildTermId = new Guid("56565656-5656-5656-5656-565656565656");
        private static readonly Guid TermSetId = new Guid("66666666-6666-6666-6666-666666666666");
        private static readonly Guid GroupId = new Guid("77777777-7777-7777-7777-777777777777");
        private static readonly Guid WebPartId = new Guid("88888888-8888-8888-8888-888888888888");
        private static readonly Guid DefinitionId = new Guid("99999999-9999-9999-9999-999999999999");
        private static readonly Guid SubscriptionId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly Guid CustomActionId = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        private static readonly Guid EventSourceId = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc");
        private static readonly Guid FeatureId = new Guid("00bfea71-e717-4e80-aa17-d0c71b360101");

        private const string ContentTypeId = "0x0101009D1CB255DA76424F860D91F20E6C4118";
        private const string PageUrl = "/sites/snapshot/SitePages/Home.aspx";

        private const string WebPartXml =
            "<webParts><webPart xmlns=\"http://schemas.microsoft.com/WebPart/v3\"><metaData>" +
            "<type name=\"Microsoft.SharePoint.WebPartPages.ContentEditorWebPart, Microsoft.SharePoint\" />" +
            "</metaData><data><properties><property name=\"Title\" type=\"string\">News &amp; events</property>" +
            "</properties></data></webPart></webParts>";

        private static readonly Dictionary<string, Func<object>> Requests = BuildCatalogue();

        public static IEnumerable<object[]> RequestNames =>
            Requests.Keys.OrderBy(name => name, StringComparer.Ordinal).Select(name => new object[] { name });

        [DataTestMethod]
        [TestCategory("Offline")]
        [DynamicData(nameof(RequestNames))]
        public void RequestPayloadMatchesSnapshot(string name)
        {
            CsomSnapshot.Verify("Request." + name, Serialize((IRequest<object>)Requests[name]()));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void EveryProvisioningRequestTypeHasASnapshot()
        {
            Type openRequest = typeof(IRequest<>);

            List<string> requestTypes = typeof(GetAuditRequest).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && !type.IsNested)
                .Where(type => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openRequest))
                .Select(type => type.FullName)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();

            HashSet<string> covered = new HashSet<string>(Requests.Values.Select(create => create().GetType().FullName));

            List<string> missing = requestTypes.Where(type => !covered.Contains(type)).ToList();

            Assert.IsTrue(requestTypes.Count > 0, "No CSOM request types were found in PnP.Core.Provisioning.");
            Assert.AreEqual(0, missing.Count, "CSOM requests without a payload snapshot: " + string.Join(", ", missing));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void BatchedUserResourceWritesMatchSnapshot()
        {
            UserResourcePath title = UserResourcePath.ForWeb(SiteId, WebId, ResourceProperty.Title);
            UserResourcePath description = UserResourcePath.ForWeb(SiteId, WebId, ResourceProperty.Description);

            IRequest<object> batch = Composite(
                new SetValueForUICultureRequest(title, "de-DE", "Titel"),
                new SetValueForUICultureRequest(title, "nl-NL", "Titel NL"),
                new SetValueForUICultureRequest(description, "de-DE", "Beschreibung"));

            CsomSnapshot.Verify("Composite.UserResourceWrites", Serialize(batch));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void BatchedUserResourceReadsMatchSnapshot()
        {
            IRequest<object> batch = Composite(
                new GetValueForUICultureRequest(UserResourcePath.ForWeb(SiteId, WebId, ResourceProperty.Title), "de-DE"),
                new GetValueForUICultureRequest(UserResourcePath.ForList(SiteId, WebId, ListId, ResourceProperty.Title), "fr-FR"));

            CsomSnapshot.Verify("Composite.UserResourceReads", Serialize(batch));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void BatchOfDifferentRequestsMatchesSnapshot()
        {
            IRequest<object> batch = Composite(
                new GetAuditRequest(SiteId, WebId),
                new GetWebPartDefinitionsRequest(SiteId, WebId, PageUrl),
                new CreateTermGroupRequest("Snapshot Group", GroupId, "A description"));

            CsomSnapshot.Verify("Composite.Mixed", Serialize(batch));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void BatchNumbersItsRequestsInOrderAndHandsEveryOneTheResponse()
        {
            RecordingRequest first = new RecordingRequest();
            RecordingRequest second = new RecordingRequest();
            RecordingRequest third = new RecordingRequest();

            IRequest<object> batch = Composite(first, second, third);

            batch.GetRequest(new IteratorIdProvider());
            batch.ProcessResponse("the-response");

            Assert.AreEqual(1, first.ActionId);
            Assert.AreEqual(2, second.ActionId);
            Assert.AreEqual(3, third.ActionId);
            CollectionAssert.AreEqual(new[] { "the-response" }, first.Responses);
            CollectionAssert.AreEqual(new[] { "the-response" }, second.Responses);
            CollectionAssert.AreEqual(new[] { "the-response" }, third.Responses);
            Assert.IsNull(batch.Result);
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void CsomIdentityMatchesSnapshot()
        {
            StringBuilder identities = new StringBuilder();
            identities.Append("Web=").Append(CsomIdentity.Web(SiteId, WebId)).Append('\n');
            identities.Append("Site=").Append(CsomIdentity.Site(SiteId, WebId)).Append('\n');
            identities.Append("List=").Append(CsomIdentity.List(SiteId, WebId, ListId)).Append('\n');
            identities.Append("Field=").Append(CsomIdentity.Field(SiteId, WebId, FieldId)).Append('\n');
            identities.Append("ListField=").Append(CsomIdentity.ListField(SiteId, WebId, ListId, FieldId)).Append('\n');
            identities.Append("ContentType=").Append(CsomIdentity.ContentType(SiteId, WebId, ContentTypeId)).Append('\n');
            identities.Append("UserCustomAction=").Append(CsomIdentity.UserCustomAction(SiteId, WebId, CustomActionId)).Append('\n');
            identities.Append("SiteUserCustomAction=").Append(CsomIdentity.SiteUserCustomAction(SiteId, WebId, CustomActionId)).Append('\n');

            CsomSnapshot.Verify("CsomIdentity", identities.ToString(), ".txt");
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void CsomTypeIdsMatchSnapshot()
        {
            StringBuilder typeIds = new StringBuilder();

            foreach (FieldInfo field in typeof(CsomTypeIds)
                .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => field.IsLiteral)
                .OrderBy(field => field.Name, StringComparer.Ordinal))
            {
                typeIds.Append(field.Name).Append('=').Append(field.GetRawConstantValue()).Append('\n');
            }

            CsomSnapshot.Verify("CsomTypeIds", typeIds.ToString(), ".txt");
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void CsomBuildingBlocksMatchSnapshot()
        {
            StaticMethodAction action = new StaticMethodAction
            {
                Id = 7,
                Name = "SnapshotMethod",
                TypeId = CsomTypeIds.Utility,
                Parameters = new List<Parameter>
                {
                    new ObjectPathParameter { ReferencedObjectPathId = 3 },
                    new ObjectArrayParameter { ObjectPathIds = new List<int> { 4, 5, 6 } },
                    new Parameter { Type = "String", Value = "literal" },
                },
            };

            StaticMethodAction withoutParameters = new StaticMethodAction
            {
                Id = 8,
                Name = "SnapshotMethodWithoutParameters",
                TypeId = CsomTypeIds.Tenant,
            };

            string blocks = action.ToString() + "\n" + withoutParameters.ToString() + "\n";

            CsomSnapshot.Verify("BuildingBlocks", blocks, ".txt");
        }

        private static string Serialize(IRequest<object> request)
        {
            CSOMApiCallBuilder builder = new CSOMApiCallBuilder();
            builder.Requests.Add(request);
            return builder.SerializeCSOMRequests();
        }

        private static IRequest<object> Composite(params IRequest<object>[] requests)
        {
            Type compositeType = typeof(CsomRequestSender).GetNestedType("CompositeRequest", BindingFlags.NonPublic);
            Assert.IsNotNull(compositeType, "CsomRequestSender no longer has a nested CompositeRequest.");

            return (IRequest<object>)Activator.CreateInstance(
                compositeType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new object[] { requests.ToList() },
                CultureInfo.InvariantCulture);
        }

        private sealed class RecordingRequest : IRequest<object>
        {
            internal int ActionId { get; private set; }

            internal List<string> Responses { get; } = new List<string>();

            public object Result => null;

            public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
            {
                ActionId = idProvider.GetActionId();
                return new List<ActionObjectPath>();
            }

            public void ProcessResponse(string response)
            {
                Responses.Add(response);
            }
        }

        private static Dictionary<string, Func<object>> BuildCatalogue()
        {
            return new Dictionary<string, Func<object>>(StringComparer.Ordinal)
            {
                ["Audit.GetAuditRequest"] = () => new GetAuditRequest(SiteId, WebId),
                ["Audit.UpdateAuditRequest.Flags"] = () => new UpdateAuditRequest(SiteId, WebId, AuditMaskType.CheckOut | AuditMaskType.ObjectDelete),
                ["Audit.UpdateAuditRequest.All"] = () => new UpdateAuditRequest(SiteId, WebId, AuditMaskType.All),

                ["ContentTypes.ReorderFieldLinksRequest"] = () => new ReorderFieldLinksRequest(SiteId, WebId, ContentTypeId,
                    new List<string> { "Title", "PnPProjectCode", "PnPProjectStatus" }),
                ["ContentTypes.UpdateContentTypeRequest.UpdateChildren"] = () => new UpdateContentTypeRequest(SiteId, WebId, ContentTypeId, true),
                ["ContentTypes.UpdateContentTypeRequest.NoChildren"] = () => new UpdateContentTypeRequest(SiteId, WebId, ContentTypeId, false),

                ["InformationPolicy.ApplyProjectPolicyRequest"] = () => new ApplyProjectPolicyRequest(SiteId, WebId, 2),
                ["InformationPolicy.GetCurrentlyAppliedProjectPolicyRequest"] = () => new GetCurrentlyAppliedProjectPolicyRequest(SiteId, WebId),
                ["InformationPolicy.GetProjectPoliciesRequest"] = () => new GetProjectPoliciesRequest(SiteId, WebId),

                ["Lists.CreateListRequest.Custom"] = () => new CreateListRequest(SiteId, WebId, "Snapshot List", "Lists/SnapshotList",
                    "A description", 100, Guid.Empty, true),
                ["Lists.CreateListRequest.FeatureTemplate"] = () => new CreateListRequest(SiteId, WebId, "Snapshot Documents", "SnapshotDocuments",
                    null, 101, FeatureId, false),

                ["Publishing.GetImageRenditionsRequest"] = () => new GetImageRenditionsRequest(),
                ["Publishing.SetImageRenditionsRequest"] = () => new SetImageRenditionsRequest(new List<ImageRenditionInfo>
                {
                    new ImageRenditionInfo { Id = 1, Name = "Thumbnail", Width = 100, Height = 100, Version = 1 },
                    new ImageRenditionInfo { Id = 2, Name = "Banner & hero", Width = 1200, Height = 300, Version = 3 },
                }),

                ["Taxonomy.AddTermLabelRequest"] = () => new AddTermLabelRequest(TermId, "Snapshot label", 1033, true),
                ["Taxonomy.CreateTermGroupRequest.WithDescription"] = () => new CreateTermGroupRequest("Snapshot Group", GroupId, "A description"),
                ["Taxonomy.CreateTermGroupRequest.WithoutDescription"] = () => new CreateTermGroupRequest("Snapshot Group", GroupId),
                ["Taxonomy.CreateTermRequest.UnderTermSet"] = () => new CreateTermRequest(TermSetId, true, "Snapshot Term", TermId, 1033),
                ["Taxonomy.CreateTermRequest.UnderTermWithOptions"] = () => new CreateTermRequest(TermId, false, "Child", ChildTermId, 1031,
                    "A description", false, "a:b"),
                ["Taxonomy.CreateTermSetRequest.Defaults"] = () => new CreateTermSetRequest(GroupId, "Snapshot Set", TermSetId, 1033),
                ["Taxonomy.CreateTermSetRequest.WithOptions"] = () => new CreateTermSetRequest(GroupId, "Snapshot Set", TermSetId, 1043,
                    "A description", true, "a:b:c"),
                ["Taxonomy.DeprecateTermRequest.Deprecate"] = () => new DeprecateTermRequest(TermId, true),
                ["Taxonomy.DeprecateTermRequest.Restore"] = () => new DeprecateTermRequest(TermId, false),
                ["Taxonomy.GetSiteCollectionTermGroupRequest.CreateIfMissing"] = () => new GetSiteCollectionTermGroupRequest(SiteId, WebId, true),
                ["Taxonomy.GetSiteCollectionTermGroupRequest.ReadOnly"] = () => new GetSiteCollectionTermGroupRequest(SiteId, WebId, false),
                ["Taxonomy.ReuseTermRequest.Defaults"] = () => new ReuseTermRequest(TermSetId, true, TermId),
                ["Taxonomy.ReuseTermRequest.Branch"] = () => new ReuseTermRequest(TermSetId, true, TermId, reuseBranch: true),
                ["Taxonomy.ReuseTermRequest.Pin"] = () => new ReuseTermRequest(TermSetId, true, TermId, pin: true),
                ["Taxonomy.ReuseTermRequest.UnderTerm"] = () => new ReuseTermRequest(ChildTermId, false, TermId),
                ["Taxonomy.SetCustomSortOrderRequest.TermSet"] = () => new SetCustomSortOrderRequest(TermSetId, true, "a:b"),
                ["Taxonomy.SetCustomSortOrderRequest.Term"] = () => new SetCustomSortOrderRequest(TermId, false, "c:d"),
                ["Taxonomy.SetTermCustomPropertyRequest.Shared"] = () => new SetTermCustomPropertyRequest(TermId, "Key", "Value", false),
                ["Taxonomy.SetTermCustomPropertyRequest.Local"] = () => new SetTermCustomPropertyRequest(TermId, "Key", "Value", true),

                ["Tenant.GetTenantCdnEnabledRequest.Public"] = () => new GetTenantCdnEnabledRequest(TenantCdnType.Public),
                ["Tenant.GetTenantCdnEnabledRequest.Private"] = () => new GetTenantCdnEnabledRequest(TenantCdnType.Private),
                ["Tenant.GetTenantCdnStringsRequest.Origins"] = () => GetTenantCdnStringsRequest.Origins(TenantCdnType.Public),
                ["Tenant.GetTenantCdnStringsRequest.Policies"] = () => GetTenantCdnStringsRequest.Policies(TenantCdnType.Private),
                ["Tenant.SetTenantCdnEnabledRequest"] = () => new SetTenantCdnEnabledRequest(TenantCdnType.Public, true),
                ["Tenant.TenantCdnOriginRequest.Add"] = () => new TenantCdnOriginRequest(TenantCdnType.Public, CdnOriginAction.Add, "*/SITEASSETS"),
                ["Tenant.TenantCdnOriginRequest.Remove"] = () => new TenantCdnOriginRequest(TenantCdnType.Private, CdnOriginAction.Remove, "sites/snapshot/lib"),
                ["Tenant.TenantCdnOriginRequest.CreateDefaults"] = () => new TenantCdnOriginRequest(TenantCdnType.Public, CdnOriginAction.CreateDefaults),
                ["Tenant.SetTenantCdnPolicyRequest"] = () => new SetTenantCdnPolicyRequest(TenantCdnType.Public, TenantCdnPolicyType.IncludeFileExtensions, "JPG,PNG"),

                ["UserResources.GetValueForUICultureRequest"] = () => new GetValueForUICultureRequest(
                    UserResourcePath.ForWeb(SiteId, WebId, ResourceProperty.Title), "de-DE"),
                ["UserResources.SetValueForUICultureRequest.WebTitle"] = () => new SetValueForUICultureRequest(
                    UserResourcePath.ForWeb(SiteId, WebId, ResourceProperty.Title), "nl-NL", "Mijn site"),
                ["UserResources.SetValueForUICultureRequest.WebDescription"] = () => new SetValueForUICultureRequest(
                    UserResourcePath.ForWeb(SiteId, WebId, ResourceProperty.Description), "de-DE", "Beschreibung"),
                ["UserResources.SetValueForUICultureRequest.List"] = () => new SetValueForUICultureRequest(
                    UserResourcePath.ForList(SiteId, WebId, ListId, ResourceProperty.Title), "de-DE", "Liste"),
                ["UserResources.SetValueForUICultureRequest.Field"] = () => new SetValueForUICultureRequest(
                    UserResourcePath.ForField(SiteId, WebId, FieldId, ResourceProperty.Title), "de-DE", "Feld"),
                ["UserResources.SetValueForUICultureRequest.ListField"] = () => new SetValueForUICultureRequest(
                    UserResourcePath.ForListField(SiteId, WebId, ListId, FieldId, ResourceProperty.Title), "de-DE", "Listenfeld"),
                ["UserResources.SetValueForUICultureRequest.ContentType"] = () => new SetValueForUICultureRequest(
                    UserResourcePath.ForContentType(SiteId, WebId, ContentTypeId, ResourceProperty.ContentTypeName), "de-DE", "Inhaltstyp"),
                ["UserResources.SetValueForUICultureRequest.UserCustomAction"] = () => new SetValueForUICultureRequest(
                    UserResourcePath.ForUserCustomAction(SiteId, WebId, CustomActionId, ResourceProperty.Title), "de-DE", "Aktion"),
                ["UserResources.SetValueForUICultureRequest.SiteUserCustomAction"] = () => new SetValueForUICultureRequest(
                    UserResourcePath.ForSiteUserCustomAction(SiteId, WebId, CustomActionId, ResourceProperty.Title), "de-DE", "Websiteaktion"),
                ["UserResources.SetValueForUICultureRequest.ListUserCustomAction"] = () => new SetValueForUICultureRequest(
                    UserResourcePath.ForListUserCustomAction(SiteId, WebId, ListId, CustomActionId, ResourceProperty.Title), "de-DE", "Listenaktion"),

                ["Web.SetAssociatedGroupRequest.Owners"] = () => new SetAssociatedGroupRequest(SiteId, WebId, 3, AssociatedGroupKind.Owners),
                ["Web.SetAssociatedGroupRequest.Members"] = () => new SetAssociatedGroupRequest(SiteId, WebId, 5, AssociatedGroupKind.Members),
                ["Web.SetAssociatedGroupRequest.Visitors"] = () => new SetAssociatedGroupRequest(SiteId, WebId, 4, AssociatedGroupKind.Visitors),
                ["Web.SetRegionalSettingsRequest"] = () => new SetRegionalSettingsRequest(SiteId, WebId, new SortedDictionary<string, object>(StringComparer.Ordinal)
                {
                    ["AdjustHijriDays"] = 0,
                    ["CalendarType"] = 1,
                    ["FirstDayOfWeek"] = 1,
                    ["LocaleId"] = 1043,
                    ["ShowWeeks"] = true,
                    ["Time24"] = false,
                    ["WorkDayStartHour"] = 480,
                    ["WorkDays"] = 62,
                }),
                ["Web.SetSiteAdministratorRequest.Grant"] = () => new SetSiteAdministratorRequest(SiteId, WebId, 12, true),
                ["Web.SetSiteAdministratorRequest.Revoke"] = () => new SetSiteAdministratorRequest(SiteId, WebId, 12, false),
                ["Web.SetSupportedUILanguagesRequest.AddAndRemove"] = () => new SetSupportedUILanguagesRequest(SiteId, WebId,
                    new List<int> { 1031, 1043 }, new List<int> { 1036 }),
                ["Web.SetSupportedUILanguagesRequest.AddOnly"] = () => new SetSupportedUILanguagesRequest(SiteId, WebId,
                    new List<int> { 1031 }, null),

                ["WebParts.AddWebPartRequest"] = () => new AddWebPartRequest(SiteId, WebId, PageUrl, WebPartXml, "Left", 0),
                ["WebParts.DeleteWebPartRequest"] = () => new DeleteWebPartRequest(SiteId, WebId, PageUrl, WebPartId),
                ["WebParts.GetWebPartDefinitionsRequest"] = () => new GetWebPartDefinitionsRequest(SiteId, WebId, PageUrl),
                ["WebParts.MoveWebPartToRequest"] = () => new MoveWebPartToRequest(SiteId, WebId, PageUrl, WebPartId, "Right", 2),
                ["WebParts.SaveWebPartPropertiesRequest.WithTitle"] = () => new SaveWebPartPropertiesRequest(SiteId, WebId, PageUrl, WebPartId, "News & events"),
                ["WebParts.SaveWebPartPropertiesRequest.WithoutTitle"] = () => new SaveWebPartPropertiesRequest(SiteId, WebId, PageUrl, WebPartId),

                ["Workflows.DeleteWorkflowDefinitionRequest"] = () => new DeleteWorkflowDefinitionRequest(SiteId, WebId, DefinitionId),
                ["Workflows.DeleteWorkflowSubscriptionRequest"] = () => new DeleteWorkflowSubscriptionRequest(SiteId, WebId, SubscriptionId),
                ["Workflows.EnumerateWorkflowInstancesRequest"] = () => new EnumerateWorkflowInstancesRequest(SiteId, WebId, SubscriptionId),
                ["Workflows.GetWorkflowDefinitionsRequest.All"] = () => new GetWorkflowDefinitionsRequest(SiteId, WebId),
                ["Workflows.GetWorkflowDefinitionsRequest.PublishedOnly"] = () => new GetWorkflowDefinitionsRequest(SiteId, WebId, publishedOnly: true),
                ["Workflows.GetWorkflowSubscriptionsRequest.Web"] = () => new GetWorkflowSubscriptionsRequest(SiteId, WebId),
                ["Workflows.GetWorkflowSubscriptionsRequest.List"] = () => new GetWorkflowSubscriptionsRequest(SiteId, WebId, ListId),
                ["Workflows.PublishWorkflowDefinitionRequest"] = () => new PublishWorkflowDefinitionRequest(SiteId, WebId, DefinitionId),
                ["Workflows.PublishWorkflowSubscriptionRequest.Web"] = () => new PublishWorkflowSubscriptionRequest(SiteId, WebId, Subscription()),
                ["Workflows.PublishWorkflowSubscriptionRequest.List"] = () => new PublishWorkflowSubscriptionRequest(SiteId, WebId, Subscription(), ListId),
                ["Workflows.SaveWorkflowDefinitionRequest.WithId"] = () => new SaveWorkflowDefinitionRequest(SiteId, WebId, Definition(DefinitionId)),
                ["Workflows.SaveWorkflowDefinitionRequest.WithoutId"] = () => new SaveWorkflowDefinitionRequest(SiteId, WebId, Definition(Guid.Empty)),
            };
        }

        private static WorkflowSubscriptionInfo Subscription()
        {
            return new WorkflowSubscriptionInfo
            {
                Id = SubscriptionId,
                DefinitionId = DefinitionId,
                Name = "Snapshot subscription",
                Enabled = true,
                EventSourceId = EventSourceId,
                EventTypes = new List<string> { "ItemAdded", "ItemUpdated" },
                StatusFieldName = "SnapshotStatus",
                PropertyDefinitions = new Dictionary<string, string>
                {
                    ["TaskListId"] = ListId.ToString(),
                    ["HistoryListId"] = FieldId.ToString(),
                },
            };
        }

        private static WorkflowDefinitionInfo Definition(Guid id)
        {
            return new WorkflowDefinitionInfo
            {
                Id = id,
                DisplayName = "Snapshot workflow",
                Description = "A description",
                Xaml = "<Activity x:Class=\"Snapshot\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" />",
                Published = false,
                RestrictToScope = ListId.ToString(),
                RestrictToType = "List",
            };
        }
    }
}
