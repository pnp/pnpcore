using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions.ACEFactory
{
    /// <summary>
    /// Default ACE factory
    /// </summary>
    public class ACEFactory
    {
        /// <summary>
        /// Not used by defult factory
        /// </summary>
        public virtual string ACEId { get; }
        /// <summary>
        /// Returns AdaptiveCardExtension with custom properties deserialized to JsonElement
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public virtual AdaptiveCardExtension BuildACEFromWebPart(IPageWebPart control)
        {
            return new AdaptiveCardExtension()
            {
                Id = control.WebPartId,
                Description = control.Description,
                InstanceId = control.InstanceId,
                Title = control.Title,
                Properties = control.Properties
            };
        }
    }
}
