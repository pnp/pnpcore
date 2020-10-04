using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.Utilities
{
    internal static class TestAssets
    {
        private static bool IsDefaultSharePointLibraryName(string libraryName)
        {
            return new string[] { "Documents", "Site Assets" }.Contains(libraryName);
        }

        /// <summary>
        /// Create a test list item in the specified list of the site specified by the context configuration
        /// </summary>
        /// <param name="contextId">The number of the context. Default is 0</param>
        /// <param name="parentListName">The name of the parent list. A dedicated list name will be created. Default is the name of the calling test</param>
        /// <param name="itemTitle">The name of the document to create. Default is the name of the calling test</param>
        /// <param name="testName">The name of the current test. Default is the name of the calling test</param>
        /// <param name="parentListEnableVersions">Enable versioning on the parent list</param>
        /// <param name="parentListEnableMinorVersions">Enable minor versions on the parent list</param>
        /// <param name="fieldValues">The field values of the list item</param>
        /// <param name="contextConfig">The name of the context config. Default is the value of TestCommon.TestSite</param>
        /// <returns>A tuple containing the name of the list and the id and the title of the created list item</returns>
        internal static async Task<Tuple<string, int, string>> CreateTestListItemAsync(int contextId = default,
              [System.Runtime.CompilerServices.CallerMemberName] string parentListName = null,
              [System.Runtime.CompilerServices.CallerMemberName] string itemTitle = null,
              [System.Runtime.CompilerServices.CallerMemberName] string testName = null,
              bool parentListEnableVersions = false,
              bool parentListEnableMinorVersions = false,
              Dictionary<string, object> fieldValues = null,
              string contextConfig = null,
              [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null)
        {
            contextConfig ??= TestCommon.TestSite;

            itemTitle = TestCommon.GetPnPSdkTestAssetName(itemTitle);

            using (var context = await TestCommon.Instance.GetContextAsync(contextConfig, contextId, testName, sourceFilePath))
            {

                parentListName = TestCommon.GetPnPSdkTestAssetName(parentListName);

                IList list = await context.Web.Lists.AddAsync(parentListName, ListTemplateType.GenericList);
                list.EnableVersioning = parentListEnableVersions;
                list.EnableMinorVersions = parentListEnableMinorVersions;
                await list.UpdateAsync();
                if (fieldValues != null)
                {
                    if (!fieldValues.ContainsKey("Title"))
                    {
                        fieldValues.Add("Title", itemTitle);
                    }
                }
                else
                {
                    fieldValues = new Dictionary<string, object>() { { "Title", itemTitle } };
                }
                IListItem listItem = await list.Items.AddAsync(fieldValues);

                return new Tuple<string, int, string>(parentListName, listItem.Id, (string)listItem.Values["Title"]);
            }
        }

        /// <summary>
        /// Create a test document in the specified library of the site specified by the context configuration
        /// </summary>
        /// <param name="contextId">The number of the context. Default is 0</param>
        /// <param name="parentLibraryName">The name of the parent library. Default is "Documents", will try to create a library if not a default SharePoint one</param>
        /// <param name="fileName">The name of the document to create. Default is the name of the calling test</param>
        /// <param name="testName">The name of the current test. Default is the name of the calling test</param>
        /// <param name="parentLibraryEnableVersioning">Enable versioning on the parent library if a parentLibraryName is not a default SharePoint library</param>
        /// <param name="parentLibraryEnableMinorVersions">Enable minor versions on the parent library if a parentLibraryName is not a default SharePoint library</param>
        /// <param name="documentMetadata">The metadata of the document</param>
        /// <param name="contextConfig">The name of the context config. Default is the value of TestCommon.TestSite</param>
        /// <returns>A tuple containing the name of the library and the name and the server relative URL of the created document</returns>
        internal static async Task<Tuple<string, string, string>> CreateTestDocumentAsync(int contextId = default,
            string parentLibraryName = "Documents",
              [System.Runtime.CompilerServices.CallerMemberName] string fileName = null,
              [System.Runtime.CompilerServices.CallerMemberName] string testName = null,
              bool parentLibraryEnableVersioning = false,
              bool parentLibraryEnableMinorVersions = false,
              Dictionary<string, object> documentMetadata = null,
              string contextConfig = null,
              [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null)
        {
            contextConfig ??= TestCommon.TestSite;

            if (!fileName.EndsWith(".docx"))
            {
                fileName += ".docx";
            }

            fileName = TestCommon.GetPnPSdkTestAssetName(fileName);

            using (var context = await TestCommon.Instance.GetContextAsync(contextConfig, contextId, testName, sourceFilePath))
            {
                IFolder folder;
                if (!IsDefaultSharePointLibraryName(parentLibraryName))
                {
                    parentLibraryName = TestCommon.GetPnPSdkTestAssetName(parentLibraryName);

                    IList documentLibrary = await context.Web.Lists.AddAsync(parentLibraryName, ListTemplateType.DocumentLibrary);
                    documentLibrary.EnableVersioning = parentLibraryEnableVersioning;
                    documentLibrary.EnableMinorVersions = parentLibraryEnableMinorVersions;
                    await documentLibrary.UpdateAsync();
                    folder = await documentLibrary.RootFolder.GetAsync();
                }
                else
                {
                    folder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
                }

                IFile testDocument = await folder.Files.AddAsync(fileName, System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"), true);
                // TODO: Update ListItem from file is not working yet
                //if (documentMetadata != null)
                //{
                //    foreach (var metadataItem in documentMetadata)
                //    {
                //        testDocument.ListItemAllFields.Values[metadataItem.Key] = metadataItem.Value;
                //    }
                //    await testDocument.ListItemAllFields.UpdateAsync();
                //}
                return new Tuple<string, string, string>(parentLibraryName, testDocument.Name, testDocument.ServerRelativeUrl);
            }
        }

        /// <summary>
        /// Create a test document in the specified library of the site specified by the context configuration
        /// </summary>
        /// <param name="contextId">The number of the context. Default is 0</param>
        /// <param name="parentLibraryName">The name of the parent library. Default is the name of the calling test</param>
        /// <param name="fileName">The name of the document to create. Default is the name of the calling test</param>
        /// <param name="testName">The name of the current test. Default is the name of the calling test</param>
        /// <param name="parentLibraryEnableVersioning">Enable versioning on the parent library if a parentLibraryName is not a default SharePoint library</param>
        /// <param name="parentLibraryEnableMinorVersions">Enable minor versions on the parent library if a parentLibraryName is not a default SharePoint library</param>
        /// <param name="documentMetadata">The metadata of the document</param>
        /// <param name="contextConfig">The name of the context config. Default is the value of TestCommon.TestSite</param>
        /// <returns>A tuple containing the name of the library and the name and the server relative URL of the created document</returns>
        internal static async Task<Tuple<string, string, string>> CreateTestDocumentInDedicatedLibraryAsync(int contextId = default,
              [System.Runtime.CompilerServices.CallerMemberName] string parentLibraryName = null,
              [System.Runtime.CompilerServices.CallerMemberName] string fileName = null,
              [System.Runtime.CompilerServices.CallerMemberName] string testName = null,
              bool parentLibraryEnableVersioning = false,
              bool parentLibraryEnableMinorVersions = false,
              Dictionary<string, object> documentMetadata = null,
              string contextConfig = null,
              [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null)
        {
            return await CreateTestDocumentAsync(contextId, parentLibraryName, fileName, testName,
                parentLibraryEnableVersioning, parentLibraryEnableMinorVersions,
                documentMetadata, contextConfig, sourceFilePath).ConfigureAwait(false);
        }

        /// <summary>
        /// Cleanup a test document from the specified parent library
        /// </summary>
        /// <param name="contextId">The number of the context. Default is 0</param>
        /// <param name="parentLibraryServerRelativeUrl">The server relative URL of the parent library. Default is "{siteUrl}/Shared Documents"</param>
        /// <param name="fileName">The name of the document to cleanup. Default is the name of the calling test</param>
        /// <param name="testName">The name of the current test. Default is the name of the calling test</param>
        /// <param name="contextConfig">The name of the context config. Default is the value of TestCommon.TestSite</param>
        internal static async Task CleanupTestDocumentAsync(int contextId = default,
            string parentLibraryServerRelativeUrl = null,
            [System.Runtime.CompilerServices.CallerMemberName] string fileName = null,
            [System.Runtime.CompilerServices.CallerMemberName] string testName = null,
            string contextConfig = null,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null)
        {
            contextConfig ??= TestCommon.TestSite;
            fileName = TestCommon.GetPnPSdkTestAssetName(fileName);
            using (var context = await TestCommon.Instance.GetContextAsync(contextConfig, contextId, testName, sourceFilePath))
            {
                parentLibraryServerRelativeUrl ??= $"{context.Uri.PathAndQuery}/Shared Documents";
                if (!fileName.EndsWith(".docx"))
                {
                    fileName += ".docx";
                }

                string testDocumentServerRelativeUrl = $"{parentLibraryServerRelativeUrl}/{fileName}";
                IFile mockDocument = await context.Web.GetFileByServerRelativeUrlAsync(testDocumentServerRelativeUrl);
                await mockDocument.DeleteAsync();
            }
        }

        /// <summary>
        /// Cleanup a test document from the specified parent library
        /// </summary>
        /// <param name="contextId">The number of the context. Default is 0</param>
        /// <param name="listName">The name of the test dedicated list to cleanup. Default is the name of the calling test</param>
        /// <param name="testName">The name of the current test. Default is the name of the calling test</param>
        /// <param name="contextConfig">The name of the context config. Default is the value of TestCommon.TestSite</param>
        internal static async Task CleanupTestDedicatedListAsync(int contextId = default,
            [System.Runtime.CompilerServices.CallerMemberName] string listName = null,
            [System.Runtime.CompilerServices.CallerMemberName] string testName = null,
            string contextConfig = null,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null)
        {
            contextConfig ??= TestCommon.TestSite;

            listName = TestCommon.GetPnPSdkTestAssetName(listName);

            using (var context = await TestCommon.Instance.GetContextAsync(contextConfig, contextId, testName, sourceFilePath))
            {
                IList documentLibrary = await context.Web.Lists.GetByTitleAsync(listName);
                await documentLibrary.DeleteAsync();
            }
        }
    }
}
