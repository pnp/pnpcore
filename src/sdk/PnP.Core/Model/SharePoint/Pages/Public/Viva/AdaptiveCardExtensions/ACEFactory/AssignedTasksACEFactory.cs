using System;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Creates new instance of Assigened Tasks ACE based on provided WebPart
    /// </summary>
    public class AssignedTasksACEFactory : ACEFactory
    {
        /// <summary>
        /// Id of Assigened Tasks AdaptiveCardExtension
        /// </summary>
        public override string ACEId { get => VivaDashboard.DefaultACEToId(DefaultACE.AssignedTasks); }
        
        /// <summary>
        /// Returns Assigened Tasks ACE
        /// </summary>
        /// <param name="control">WebPart used to store ACE configuration</param>
        /// <returns></returns>
        public override AdaptiveCardExtension BuildACEFromWebPart(IPageWebPart control)
        {
            return new AssignedTasksACE()
            {
                Id = control.WebPartId,
                Description = control.Description,
                InstanceId = control.InstanceId,
                Title = control.Title,
                Order = control.Order,
                JsonProperties = control.Properties,
                Properties = JsonSerializer.Deserialize<object>(control.PropertiesJson),
                CardSize = (CardSize)Enum.Parse(typeof(CardSize), (control as PageWebPart).ACECardSize),
                IconProperty = (control as PageWebPart).ACEIconProperty,
            };
        }
    }
}
