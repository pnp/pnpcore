using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions.ACEFactory
{
    /// <summary>
    /// Creates new instance of CardDesigner ACE based on provided WebPart
    /// </summary>
    public class CardDesignerACEFactory : ACEFactory
    {
        /// <summary>
        /// Id of CardDesigner AdaptiveCardExtension
        /// </summary>
        public override string ACEId { get => "9593e615-7320-4b8b-be98-09b97112b12f"; }
        /// <summary>
        /// Returns CardDesigner
        /// </summary>
        /// <param name="control">WebPart used to store ACE configuration</param>
        /// <returns></returns>
        public override AdaptiveCardExtension BuildACEFromWebPart(IPageWebPart control)
        {
            return new CardDesignerACE()
            {
                Id = control.WebPartId,
                Description = control.Description,
                InstanceId = control.InstanceId,
                Title = control.Title,
                Properties = JsonSerializer.Deserialize<CardDesignerProps>(control.PropertiesJson)
            };
        }
    }
}
