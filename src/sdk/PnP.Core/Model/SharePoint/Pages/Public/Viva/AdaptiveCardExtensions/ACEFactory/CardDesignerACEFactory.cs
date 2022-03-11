using System;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Creates new instance of CardDesigner ACE based on provided WebPart
    /// </summary>
    public class CardDesignerACEFactory : ACEFactory
    {
        /// <summary>
        /// Id of CardDesigner AdaptiveCardExtension
        /// </summary>
        public override string ACEId { get => VivaDashboard.DefaultACEToId(DefaultACE.CardDesigner); }
        
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
                Order = control.Order,
                JsonProperties = control.Properties,
                Properties = JsonSerializer.Deserialize<CardDesignerProps>(control.PropertiesJson),
                CardSize = (CardSize)Enum.Parse(typeof(CardSize), (control as PageWebPart).ACECardSize),
                IconProperty = (control as PageWebPart).ACEIconProperty,
            };
        }
    }
}
