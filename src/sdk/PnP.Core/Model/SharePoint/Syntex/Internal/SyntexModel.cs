using PnP.Core.Services;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal class SyntexModel : ISyntexModel
    {
        #region Properties

        public IListItem ListItem { get; internal set; }

        public IFile File { get; internal set; }

        public int Id
        {
            get
            {
                if (ListItem != null)
                {
                    return ListItem.Id;
                }
                return 0;
            }
        }

        public Guid UniqueId 
        {
            get
            {
                if (File != null)
                {
                    return File.UniqueId;
                }

                return Guid.Empty;
            }
        }        

        public string Name
        {
            get
            {
                if (ListItem != null)
                {
                    return ListItem.Values[PageConstants.ModelMappedClassifierName].ToString();
                }
                return null;
            }
        }

        public DateTime ModelLastTrained
        {
            get
            {
                if (ListItem != null)
                {
                    if (ListItem.Values[PageConstants.ModelLastTrained] is DateTime dateTime)
                    {
                        return dateTime;
                    }

                    if (DateTime.TryParse(ListItem.Values[PageConstants.ModelLastTrained]?.ToString(), out DateTime modelLastTrained))
                    {
                        return modelLastTrained;
                    }
                }
                return DateTime.MinValue;
            }
        }

        public string Description
        {
            get
            {
                if (ListItem != null)
                {
                    return ListItem.Values[PageConstants.ModelDescription].ToString();
                }
                return null;
            }
        }

        #endregion

        #region Methods
        public async Task RegisterModelAsync(IList list)
        {
            // Ensure we have the needed data loaded
            await (list as List).EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
            await list.PnPContext.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

            /*
            {
                "__metadata": {
                    "type": "Microsoft.Office.Server.ContentCenter.SPMachineLearningPublicationsEntityData"
                },
                "Publications": {
                    "results": [
                        {
                            "ModelUniqueId": "bb25a5be-aeed-436d-90e7-add975ac766e",
                            "TargetSiteUrl": "https://m365x215748.sharepoint.com/sites/Mark8ProjectTeam",
                            "TargetWebServerRelativeUrl": "/sites/Mark8ProjectTeam",
                            "TargetLibraryServerRelativeUrl": "/sites/Mark8ProjectTeam/Shared Documents",
                            "ViewOption": "NewViewAsDefault"
                        }
                    ]
                },
                "Comment": ""
            }
            */
            var registerInfo = new
            {
                __metadata = new { type = "Microsoft.Office.Server.ContentCenter.SPMachineLearningPublicationsEntityData" },
                Publications = new 
                {
                    results = new System.Collections.Generic.List<dynamic>()
                    {
                        new
                        {
                            ModelUniqueId = UniqueId,
                            TargetSiteUrl = list.PnPContext.Uri.ToString(),
                            TargetWebServerRelativeUrl = list.PnPContext.Web.ServerRelativeUrl,
                            TargetLibraryServerRelativeUrl = list.RootFolder.ServerRelativeUrl,
                            ViewOption = "NewViewAsDefault"
                        }
                    }
                }
            }.AsExpando();

            string body = JsonSerializer.Serialize(registerInfo, new JsonSerializerOptions() { IgnoreNullValues = true });

            var apiCall = new ApiCall("/_api/machinelearning/publications", ApiType.SPORest, body);
            var publicationResponse = await (ListItem as ListItem).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

        }
        #endregion
    }
}
