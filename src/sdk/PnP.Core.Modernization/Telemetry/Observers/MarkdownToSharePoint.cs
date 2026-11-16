using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using PnP.Core.Modernization.Cache;
using System;
using System.Linq;
using PnPCore = PnP.Core.Model.SharePoint;

namespace PnP.Core.Modernization.Telemetry.Observers
{
    /// <summary>
    /// Writes an MD log file to a folder (default = Transformation-Reports) inside the sitepages library
    /// </summary>
    public class MarkdownToSharePointObserver : MarkdownObserver
    {

        private ClientContext _clientContext;
        private string _folderName;
        private string _fileName;


        /// <summary>
        /// Constructor to save a markdown report to SharePoint Modern Site Assets library
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folderName">Folder that will hold the log file</param>
        /// <param name="fileName">Name used to construct the log file name</param>
        /// <param name="includeDebugEntries">Include Debug Log Entries</param>
        /// <param name="includeVerbose">Include verbose details</param>
        public MarkdownToSharePointObserver(ClientContext context, string folderName = "Transformation-Reports", string fileName = "", bool includeDebugEntries = false, bool includeVerbose = false) : base(fileName, null, includeDebugEntries, includeVerbose)
        {
            _clientContext = context;
            _folderName = folderName;
            _fileName = fileName;
        }

        /// <summary>
        /// Ensure Folder - Just make sure the location exists 
        /// </summary>
        /// <returns></returns>
        public Folder EnsureDestination()
        {
            //Ensure that the Site Assets library is created using the out of the box creation mechanism
            //Site Assets that are created using the EnsureSiteAssetsLibrary method slightly differ from
            //default Document Libraries. See issue 512 (https://github.com/SharePoint/PnP-Sites-Core/issues/512)
            //for details about the issue fixed by this approach.
            var library = _clientContext.Web.Lists.EnsureSitePagesLibrary();
            //Check that Title and Description have the correct values
            this._clientContext.Web.Context.Load(library, l => l.Title, l => l.RootFolder);
            this._clientContext.Web.Context.ExecuteQueryRetry();

            var sitePagesFolder = library.RootFolder;

            if (!string.IsNullOrEmpty(_folderName))
            {
                sitePagesFolder = library.RootFolder.EnsureFolder(_folderName);
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
                if (_clientContext == null)
                {
                    throw new ArgumentNullException("ClientContext is null");
                }

                var report = GenerateReportWithSummaryAtTop(includeHeading: false);

                // Dont want to assume locality here
                string logRunTime = _reportDate.ToString().Replace('/', '-').Replace(":", "-").Replace(" ", "-");
                string logFileName = $"Page-Transformation-Report-{logRunTime}{_reportFileName}";

                logFileName = logFileName + ".aspx";
                var targetFolder = this.EnsureDestination();

                var pageName = $"{targetFolder.Name}/{logFileName}";

                var reportPage = this._clientContext.Web.AddClientSidePage(pageName);
                reportPage.PageTitle = base._includeVerbose ? LogStrings.Report_ModernisationReport : LogStrings.Report_ModernisationSummaryReport;

                var componentsToAdd = CacheManager.Instance.GetClientSideComponents(_clientContext, reportPage);

                PnPCore.IPageComponent baseControl = null;
                var webPartName = reportPage.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.MarkDown);

                baseControl = componentsToAdd.FirstOrDefault(p => p.Name.Equals(webPartName, StringComparison.InvariantCultureIgnoreCase));

                var jsonRpt = JsonConvert.SerializeObject(report, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml });

                var jsonDecoded = GetMarkdownJsonProperties(jsonRpt);

                //PnP.Framework.Pages.ClientSideWebPart mdWebPart = new PnP.Framework.Pages.ClientSideWebPart(baseControl)
                //{
                //    PropertiesJson = jsonDecoded
                //};
                var mdWebPart = reportPage.NewWebPart(baseControl);
                mdWebPart.PropertiesJson = jsonDecoded;

                // This should only have one web part on the page
                reportPage.AddControl(mdWebPart);
                reportPage.Save(pageName);
                reportPage.DisableComments();

                // Cleardown all logs
                Logs.RemoveRange(0, Logs.Count);

                Console.WriteLine($"Report saved as: {pageName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to log file: {0} {1}", ex.Message, ex.StackTrace);
            }
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