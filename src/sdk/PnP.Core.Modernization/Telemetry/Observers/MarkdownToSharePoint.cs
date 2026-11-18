using Newtonsoft.Json;
using PnP.Core.Modernization.Cache;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;


namespace PnP.Core.Modernization.Telemetry.Observers
{
    /// <summary>
    /// Writes a Markdown report page to a folder (default = Transformation-Reports) inside the Site Pages library.
    /// </summary>
    public class MarkdownToSharePointObserver : MarkdownObserver
    {

        private readonly IWeb _web;
        private readonly string _folderName;


        /// <summary>
        /// Constructor to save a Markdown report to the SharePoint Site Pages library.
        /// </summary>
        /// <param name="web">Target Core web. Its context remains owned by the caller.</param>
        /// <param name="folderName">Folder that will hold the log file</param>
        /// <param name="fileName">Name used to construct the log file name</param>
        /// <param name="includeDebugEntries">Include Debug Log Entries</param>
        /// <param name="includeVerbose">Include verbose details</param>
        public MarkdownToSharePointObserver(IWeb web, string folderName = "Transformation-Reports", string fileName = "", bool includeDebugEntries = false, bool includeVerbose = false) : base(fileName, null, includeDebugEntries, includeVerbose)
        {
            _web = web ?? throw new ArgumentNullException(nameof(web));
            _folderName = folderName?.Trim('/');
        }

        /// <summary>
        /// Ensure Folder - Just make sure the location exists 
        /// </summary>
        /// <returns></returns>
        public IFolder EnsureDestination()
        {
            return EnsureDestinationAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Ensures the Site Pages library and configured report folder exist using SharePoint REST.
        /// </summary>
        /// <returns>The report folder.</returns>
        public async Task<IFolder> EnsureDestinationAsync()
        {
            var response = await _web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest,
                "_api/web/lists/ensuresitepageslibrary", null,
                new Dictionary<string, string> { ["Accept"] = "application/json;odata=nometadata" })).ConfigureAwait(false);
            Guid libraryId;
            using (var json = JsonDocument.Parse(response.Response))
            {
                libraryId = json.RootElement.GetProperty("Id").GetGuid();
            }
            var library = await _web.Lists.GetByIdAsync(libraryId, p => p.RootFolder).ConfigureAwait(false);
            var sitePagesFolder = library.RootFolder;

            if (!string.IsNullOrEmpty(_folderName))
            {
                sitePagesFolder = await sitePagesFolder.EnsureFolderAsync(_folderName).ConfigureAwait(false);
            }

            return sitePagesFolder;
        }

        /// <summary>
        /// Write the report to SharePoint
        /// </summary>
        /// <param name="clearLogData">Also clear the log data</param>
        public override void Flush(bool clearLogData)
        {
            try
            {
                FlushAsync(clearLogData).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to log file: {0} {1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Writes the report using Core APIs. Failures propagate without clearing buffered logs.
        /// </summary>
        /// <param name="clearLogData">Clear buffered logs after a successful write.</param>
        /// <returns>The saved page name relative to the Site Pages library.</returns>
        public async Task<string> FlushAsync(bool clearLogData = true)
        {
            var report = GenerateReportWithSummaryAtTop(includeHeading: false);
            string logRunTime = _reportDate.ToString().Replace('/', '-').Replace(":", "-").Replace(" ", "-");
            string logFileName = $"Page-Transformation-Report-{logRunTime}{_reportFileName}.aspx";
            await EnsureDestinationAsync().ConfigureAwait(false);
            var pageName = string.IsNullOrEmpty(_folderName) ? logFileName : $"{_folderName}/{logFileName}";

            var reportPage = await _web.NewPageAsync().ConfigureAwait(false);
            reportPage.PageTitle = _includeVerbose ? LogStrings.Report_ModernisationReport : LogStrings.Report_ModernisationSummaryReport;
            var components = await CacheManager.Instance.GetClientSideComponentsAsync(_web, reportPage).ConfigureAwait(false);
            var webPartName = reportPage.DefaultWebPartToWebPartId(DefaultWebPart.MarkDown);
            var component = components.FirstOrDefault(p => string.Equals(p.Name, webPartName, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("The Markdown web part is not available in the target web.");
            var jsonReport = JsonConvert.SerializeObject(report, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml });
            var markdown = reportPage.NewWebPart(component);
            markdown.PropertiesJson = GetMarkdownJsonProperties(jsonReport);
            reportPage.AddControl(markdown);
            await reportPage.SaveAsync(pageName).ConfigureAwait(false);
            await reportPage.DisableCommentsAsync().ConfigureAwait(false);

            if (clearLogData)
            {
                Logs.Clear();
            }
            Console.WriteLine($"Report saved as: {pageName}");
            return pageName;
        }

        /// <summary>
        /// Construct a markdown web part properties
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        public string GetMarkdownJsonProperties(string report)
        {

            var markdown = "\"\"";

            //TODO: Add encoding for non json safe characters
            if (!string.IsNullOrEmpty(report))
            {
                markdown = report;
            }

            return @"
                    {
                      ""title"": ""Markdown"",
                      ""description"": ""Use markdown to add text, tables, links, and images to your page."",
                      ""serverProcessedContent"": {
                
                        ""searchablePlainTexts"": {
                                        ""code"": " + markdown + @"
                        },
                        ""imageSources"": { },
                        ""links"": { }
                                },
                      ""dataVersion"": ""2.0"",
                      ""properties"": {
                                    ""displayPreview"": true,
                        ""lineWrapping"": true,
                        ""miniMap"": {
                                        ""enabled"": false
                        },
                        ""previewState"": ""Show"",
                        ""theme"": ""Monokai""
                      }
                    }  
                ";
        }
    }
}