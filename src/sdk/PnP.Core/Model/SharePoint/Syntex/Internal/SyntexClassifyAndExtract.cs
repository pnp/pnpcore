using PnP.Core.Services;
using System;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal static class SyntexClassifyAndExtract
    {
        internal static ISyntexClassifyAndExtractResult ProcessClassifyAndExtractResponse(string json)
        {
            var root = JsonSerializer.Deserialize<JsonElement>(json);
            return new SyntexClassifyAndExtractResult
            {
                Created = root.GetProperty("Created").GetDateTime(),
                DeliverDate = root.GetProperty("DeliverDate").GetDateTime(),
                ErrorMessage = root.GetProperty("ErrorMessage").GetString(),
                StatusCode = root.GetProperty("StatusCode").GetInt32(),
                Id = root.GetProperty("ID").GetGuid(),
                Status = root.GetProperty("Status").GetString(),
                WorkItemType = root.GetProperty("Type").GetGuid(),
                TargetServerRelativeUrl = root.GetProperty("TargetServerRelativeUrl").GetString(),
                TargetSiteUrl = root.GetProperty("TargetSiteUrl").GetString(),
                TargetWebServerRelativeUrl = root.GetProperty("TargetWebServerRelativeUrl").GetString()
            };
        }

        internal static ApiCall CreateClassifyAndExtractApiCall(PnPContext context, Guid fileUniqueId, bool isFolder)
        {
            var classifyAndExtractFile = new
            {
                __metadata = new { type = "Microsoft.Office.Server.ContentCenter.SPMachineLearningWorkItemEntityData" },
                Type = "AE9D4F24-EE84-4C0C-A972-A74CFFE939A1",
                TargetSiteId = context.Site.Id,
                TargetWebId = context.Web.Id,
                TargetUniqueId = fileUniqueId,
                IsFolder = isFolder
            }.AsExpando();

            string body = JsonSerializer.Serialize(classifyAndExtractFile, PnPConstants.JsonSerializer_IgnoreNullValues);

            var apiCall = new ApiCall("_api/machinelearning/workitems", ApiType.SPORest, body);
            return apiCall;
        }

    }
}
