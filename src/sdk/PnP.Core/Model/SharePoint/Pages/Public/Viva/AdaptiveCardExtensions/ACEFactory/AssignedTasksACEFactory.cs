using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions.ACEFactory
{
    /// <summary>
    /// Creates new instance of Assigened Tasks ACE based on provided WebPart
    /// </summary>
    public class AssignedTasksACEFactory :ACEFactory
    {
        /// <summary>
        /// Id of Assigened Tasks AdaptiveCardExtension
        /// </summary>
        public override string ACEId { get => "749d8ca7-0821-4e96-be16-db7b0bcf1a9e"; }
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
                Properties = JsonSerializer.Deserialize<object>(control.PropertiesJson)
            };
        }
    }
}
