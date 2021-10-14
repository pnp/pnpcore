using System.Collections.Generic;

namespace PnP.Core.Transformation.Model
{
    /// <summary>
    /// Defines an abstraction for a Canvas Control in a Column of a Section of a target modern page
    /// </summary>
    public class CanvasControl
    {
        private Dictionary<string, object> properties = new Dictionary<string, object>();

        /// <summary>
        /// Type of the canvas control
        /// </summary>
        public CanvasControlType ControlType { get; set; }

        /// <summary>
        /// Ordinal position of the canvas control
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Properties of the canvas control
        /// </summary>
        public object this[string index]
        {
            get
            {
                if (this.properties.ContainsKey(index))
                {
                    return this.properties[index];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                this.properties[index] = value;
            }
        }
    }

    /// <summary>
    /// Defines the flavors of canvas controls
    /// </summary>
    public enum CanvasControlType
    {
        /// <summary>
        /// Client-side Text
        /// </summary>
        ClientSideText,
        /// <summary>
        /// Custom client-side web part
        /// </summary>
        CustomClientSideWebPart,
        /// <summary>
        /// Native default web part
        /// </summary>
        DefaultWebPart
    }
}
