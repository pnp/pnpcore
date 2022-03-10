using PnP.Core.Model.SharePoint;
using PnP.Core.Model.SharePoint.Pages.Public.Viva;
using PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions;
using PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions.ACEFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint.Viva.MockData
{
    internal class CustomAsyncCard : AdaptiveCardExtension<CustomAsyncCardProps>
    {
        public CustomAsyncCard()
        {
            Id = "9e73ef29-1b62-4084-92b5-207bedea22b8";
            Title = "Async Card";
        }
    }
    internal class CustomAsyncCardProps : ACEProperties
    {
    }
    internal class CustomAsyncCardFactory : ACEFactory
    {
        public override string ACEId => "9e73ef29-1b62-4084-92b5-207bedea22b8";
        public override AdaptiveCardExtension BuildACEFromWebPart(IPageWebPart control)
        {
            return new CustomAsyncCard()
            {
                Id = control.WebPartId,
                Description = control.Description,
                InstanceId = control.InstanceId,
                Title = control.Title,
                Properties = JsonSerializer.Deserialize<CustomAsyncCardProps>(control.PropertiesJson)
            };
        }
    }
}
