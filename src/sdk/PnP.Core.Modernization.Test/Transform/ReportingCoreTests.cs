using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Telemetry.Observers;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Tests.Transform
{
    [TestClass]
    [TestCategory("Offline")]
    [DoNotParallelize]
    public class ReportingCoreTests
    {
        [TestInitialize]
        public void Initialize()
        {
            CacheManager.Instance.ClearClientSideComponents();
            MarkdownObserver.Logs.Clear();
        }

        [TestCleanup]
        public void Cleanup()
        {
            CacheManager.Instance.ClearClientSideComponents();
            MarkdownObserver.Logs.Clear();
        }

        [TestMethod]
        public async Task ComponentCacheUsesCoreWebIdentityAndDoesNotMixTargetWebs()
        {
            var first = new Fixture();
            var second = new Fixture();
            first.Components[0].Name = "First web";
            second.Components[0].Name = "Second web";

            var initial = await CacheManager.Instance.GetClientSideComponentsAsync(first.Web.Object, first.Page.Object).ConfigureAwait(false);
            var cached = CacheManager.Instance.GetClientSideComponents(first.Web.Object, first.Page.Object);
            var other = await CacheManager.Instance.GetClientSideComponentsAsync(second.Web.Object, second.Page.Object).ConfigureAwait(false);

            Assert.AreEqual("First web", initial[0].Name);
            Assert.AreEqual("First web", cached[0].Name);
            Assert.AreEqual("Second web", other[0].Name);
            first.Page.Verify(p => p.AvailablePageComponentsAsync(null), Times.Once);
            second.Page.Verify(p => p.AvailablePageComponentsAsync(null), Times.Once);
        }

        [DataTestMethod]
        [DataRow("Transformation-Reports")]
        [DataRow("Reports/run")]
        [DataRow("")]
        public async Task ReportUsesCoreRestAndPreservesTheRequestedLibraryRelativeFolder(string folder)
        {
            var fixture = new Fixture();
            var observer = new ReportObserver(fixture.Web.Object, folder);
            observer.Info(new LogEntry { Message = "Retirement verification" });

            var name = await observer.FlushAsync(clearLogData: false).ConfigureAwait(false);

            Assert.IsTrue(name.StartsWith(string.IsNullOrEmpty(folder)
                ? "Page-Transformation-Report-" : folder + "/Page-Transformation-Report-", StringComparison.Ordinal));
            Assert.AreEqual(ReportObserver.Report, (string)JObject.Parse(fixture.Markdown.Object.PropertiesJson)
                ["serverProcessedContent"]["searchablePlainTexts"]["code"]);
            fixture.Page.Verify(p => p.SaveAsync(name), Times.Once);
            fixture.Page.Verify(p => p.DisableCommentsAsync(), Times.Once);
            fixture.Page.Verify(p => p.AddControl(fixture.Markdown.Object, null), Times.Once);
            fixture.Web.Verify(w => w.ExecuteRequestAsync(It.Is<ApiRequest>(request =>
                request.HttpMethod == HttpMethod.Post && request.Type == ApiRequestType.SPORest &&
                request.Request == "_api/web/lists/ensuresitepageslibrary" &&
                request.Headers["Accept"] == "application/json;odata=nometadata")), Times.Once);
            fixture.Root.Verify(f => f.EnsureFolderAsync(folder, It.IsAny<Expression<Func<IFolder, object>>[]>()),
                string.IsNullOrEmpty(folder) ? Times.Never() : Times.Once());
            Assert.AreEqual(1, MarkdownObserver.Logs.Count);

            await observer.FlushAsync().ConfigureAwait(false);
            Assert.AreEqual(0, MarkdownObserver.Logs.Count);
        }

        [DataTestMethod]
        [DataRow("ensure")]
        [DataRow("save")]
        [DataRow("comments")]
        public async Task AsyncReportPropagatesFailuresAndKeepsBufferedLogs(string failingOperation)
        {
            var fixture = new Fixture();
            var failure = new InvalidOperationException("Simulated " + failingOperation + " failure");
            if (failingOperation == "ensure")
            {
                fixture.Web.Setup(w => w.ExecuteRequestAsync(It.IsAny<ApiRequest>())).ThrowsAsync(failure);
            }
            else if (failingOperation == "save")
            {
                fixture.Page.Setup(p => p.SaveAsync(It.IsAny<string>())).ThrowsAsync(failure);
            }
            else
            {
                fixture.Page.Setup(p => p.DisableCommentsAsync()).ThrowsAsync(failure);
            }
            var observer = new ReportObserver(fixture.Web.Object, "Reports");
            observer.Info(new LogEntry { Message = "Keep this entry on failure" });

            var actual = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => observer.FlushAsync()).ConfigureAwait(false);

            Assert.AreSame(failure, actual);
            Assert.AreEqual(1, MarkdownObserver.Logs.Count);
            if (failingOperation != "comments")
            {
                fixture.Page.Verify(p => p.DisableCommentsAsync(), Times.Never);
            }
        }

        [TestMethod]
        public async Task MissingMarkdownComponentFailsBeforeSaving()
        {
            var fixture = new Fixture();
            fixture.Components.Clear();
            var observer = new ReportObserver(fixture.Web.Object, "Reports");

            var error = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => observer.FlushAsync()).ConfigureAwait(false);

            StringAssert.Contains(error.Message, "Markdown web part");
            fixture.Page.Verify(p => p.SaveAsync(It.IsAny<string>()), Times.Never);
        }

        private sealed class ReportObserver : MarkdownToSharePointObserver
        {
            internal const string Report = "# Report\nQuoted \"text\", <markup>, and a link: [page](https://example.test/).";

            internal ReportObserver(IWeb web, string folder) : base(web, folder, "verification")
            {
                _reportDate = new DateTime(2026, 1, 2, 3, 4, 5);
            }

            protected override string GenerateReportWithSummaryAtTop(bool includeHeading = true) => Report;
        }

        private sealed class Fixture
        {
            internal readonly Mock<IWeb> Web = new Mock<IWeb>();
            internal readonly Mock<IFolder> Root = new Mock<IFolder>();
            internal readonly Mock<IPage> Page = new Mock<IPage>();
            internal readonly Mock<IPageWebPart> Markdown = new Mock<IPageWebPart>();
            internal readonly List<IPageComponent> Components = new List<IPageComponent>();

            internal Fixture()
            {
                var component = new Mock<IPageComponent>();
                component.SetupAllProperties();
                component.Object.Id = Guid.NewGuid().ToString();
                component.Object.Name = "Markdown";
                component.Object.Manifest = "{}";
                Components.Add(component.Object);
                var libraryId = Guid.NewGuid();
                using var context = CoreTestContext.Create(respond: (_, _) => CoreTestContext.Json(new { Id = libraryId }));
                var response = context.Web.ExecuteRequest(new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest,
                    "_api/web/lists/ensuresitepageslibrary", null));
                var lists = new Mock<IListCollection>();
                var library = new Mock<IList>();
                var destination = new Mock<IFolder>();
                Web.SetupGet(w => w.Id).Returns(Guid.NewGuid());
                Web.Setup(w => w.EnsurePropertiesAsync(It.IsAny<Expression<Func<IWeb, object>>[]>()))
                    .Returns(Task.CompletedTask);
                Web.SetupGet(w => w.Lists).Returns(lists.Object);
                Web.Setup(w => w.ExecuteRequestAsync(It.IsAny<ApiRequest>())).ReturnsAsync(response);
                lists.Setup(l => l.GetByIdAsync(libraryId, It.IsAny<Expression<Func<IList, object>>[]>()))
                    .ReturnsAsync(library.Object);
                library.SetupGet(l => l.RootFolder).Returns(Root.Object);
                Root.Setup(f => f.EnsureFolderAsync(It.IsAny<string>(), It.IsAny<Expression<Func<IFolder, object>>[]>()))
                    .ReturnsAsync(destination.Object);
                Web.Setup(w => w.NewPageAsync(PageLayoutType.Article, EditorType.CK5)).ReturnsAsync(Page.Object);
                Page.Setup(p => p.AvailablePageComponentsAsync(null)).ReturnsAsync(Components);
                Page.Setup(p => p.DefaultWebPartToWebPartId(DefaultWebPart.MarkDown)).Returns("Markdown");
                Page.Setup(p => p.NewWebPart(It.IsAny<IPageComponent>())).Returns(Markdown.Object);
                Page.Setup(p => p.SaveAsync(It.IsAny<string>())).ReturnsAsync((string)null);
                Page.Setup(p => p.DisableCommentsAsync()).Returns(Task.CompletedTask);
                Markdown.SetupAllProperties();
            }
        }
    }
}
