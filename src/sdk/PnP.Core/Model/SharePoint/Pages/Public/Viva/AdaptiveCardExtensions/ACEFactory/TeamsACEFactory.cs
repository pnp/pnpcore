using System;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Creates new instance of TeamsACE based on provided WebPart
    /// </summary>
    public class TeamsACEFactory : ACEFactory
    {
        /// <summary>
        /// Id of Teams AdaptiveCardExtension
        /// </summary>
        public override string ACEId { get => VivaDashboard.DefaultACEToId(DefaultACE.TeamsApp); }
        
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
                Order = control.Order,
                JsonProperties = control.Properties,
                Properties = JsonSerializer.Deserialize<TeamsACEProperties>(control.PropertiesJson),
                CardSize = (CardSize)Enum.Parse(typeof(CardSize), (control as PageWebPart).ACECardSize),
                IconProperty = (control as PageWebPart).ACEIconProperty,
            };
        }
    }
}
