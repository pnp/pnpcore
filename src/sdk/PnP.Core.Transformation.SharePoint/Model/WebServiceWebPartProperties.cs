using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.SharePoint.Model
{
    /// <summary>
    /// Entity to describe a web part on a wiki or webpart page called from web services
    /// </summary>
    internal class WebServiceWebPartProperties
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WebServiceWebPartProperties()
        {
            this.Properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Type of the web part
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Id of the web part
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Control id of the web part
        /// </summary>
        public string ControlId { get; set; }

        /// <summary>
        /// Dictionary with web part properties
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }

        /// <summary>
        /// Zone id of the web part
        /// </summary>
        public string ZoneId { get; set; }

        /// <summary>
        /// Returns the shortened web part type name
        /// </summary>
        /// <returns>Shortened web part type name</returns>
        public string TypeShort()
        {
            string name = Type;
            var typeSplit = Type.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (typeSplit.Length > 0)
            {
                name = typeSplit[0];
            }

            return $"{name}";
        }

        /// <summary>
        /// Returns Dictionary with web part properties as string,object 
        /// </summary>
        /// <returns>Dictionary with web part properties as string,object</returns>
        public Dictionary<string, object> PropertiesAsStringObjectDictionary()
        {
            Dictionary<string, object> castedCollection = new Dictionary<string, object>();

            foreach (var item in this.Properties)
            {
                castedCollection.Add(item.Key, (object)item.Value);
            }

            return castedCollection;
        }
    }
}
