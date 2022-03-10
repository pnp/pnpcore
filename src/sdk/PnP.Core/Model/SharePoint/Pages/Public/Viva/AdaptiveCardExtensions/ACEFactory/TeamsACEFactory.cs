using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions.ACEFactory
{
    /// <summary>
    /// Creates new instance of TeamsACE based on provided WebPart
    /// </summary>
    public  class TeamsACEFactory : ACEFactory
    {
        /// <summary>
        /// Id of Teams AdaptiveCardExtension
        /// </summary>
        public override string ACEId { get => "3f2506d3-390c-426e-b272-4b4ec0ee4d2d"; }
        /// <summary>
        /// Returns TeamsACE
        /// </summary>
        /// <param name="control">WebPart used to store ACE configuration</param>
        /// <returns></returns>
        public override AdaptiveCardExtension BuildACEFromWebPart(IPageWebPart control)
        {
            return new TeamsACE()
            {
                Id = control.WebPartId,
                Description = control.Description,
                InstanceId = control.InstanceId,
                Title = control.Title,
                Properties = JsonSerializer.Deserialize<TeamsACEProperties>(control.PropertiesJson)
            };
        }
    }
}
