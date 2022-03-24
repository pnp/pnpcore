using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{

    [SharePointType("SP.NavigationNode", Target = typeof(Web), Uri = "_api/web/navigation/GetNodeById({Id})", Get = "_api /Web/Navigation/QuickLaunch", LinqGet = "_api/Web/Navigation/QuickLaunch")]
    internal sealed class NavigationNode : BaseDataModel<INavigationNode>, INavigationNode
    {
        internal const string NavigationNodeOptionsAdditionalInformationKey = "NavigationNodeOptions";

        #region Construction
        public NavigationNode()
        {
            // Handler to construct the Add request for this list
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (additionalInformation) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var navigationNodeOptions = (NavigationNodeOptions)additionalInformation[NavigationNodeOptionsAdditionalInformationKey];

                // Build body
                var navigationNodeCreationInformation = new
                {
                    __metadata = new { type = "SP.NavigationNode" },
                    IsVisible = true,
                    Title = "Test title",
                    Url = "https://mathijsdev2.sharepoint.com"
                    
                }.AsExpando();

                string body = JsonSerializer.Serialize(navigationNodeCreationInformation, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues);

                return new ApiCall($"_api/web/navigation/quicklaunch", ApiType.SPORest, body);
            };
        }
        #endregion

        #region Properties
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public bool IsExternal { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsDocLib { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsVisible { get => GetValue<bool>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public int CurrentLCID { get => GetValue<int>(); set => SetValue(value); }

        public ListTemplateType ListTemplateType { get => GetValue<ListTemplateType>(); set => SetValue(value); }

        public List<Guid> AudienceIds { get => GetValue<List<Guid>>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Convert.ToInt32(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }

        #endregion
    }
}
