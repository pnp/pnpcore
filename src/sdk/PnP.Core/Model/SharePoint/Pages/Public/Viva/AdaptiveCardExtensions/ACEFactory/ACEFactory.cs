using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Default ACE factory
    /// </summary>
    public class ACEFactory
    {
        /// <summary>
        /// Not used by default factory
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
                Order = control.Order,
                Properties = control.Properties,
                JsonProperties = control.Properties,
                CardSize = (CardSize)Enum.Parse(typeof(CardSize), (control as PageWebPart).ACECardSize),
                IconProperty = (control as PageWebPart).ACEIconProperty,
            };
        }
    }
}
