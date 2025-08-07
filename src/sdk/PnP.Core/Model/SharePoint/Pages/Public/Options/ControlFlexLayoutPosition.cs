using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents the position and dimensions of a control within a flexible layout on a canvas.
    /// </summary>
    /// <remarks>This class provides properties to define the X and Y coordinates, width, and height of a
    /// control. Additionally, it includes an optional identifier for grouping controls within a layout.</remarks>
    public class ControlFlexLayoutPosition
    {
        /// <summary>
        /// Gets or sets the X-coordinate position of the object.
        /// </summary>
        public int XPos { get; set; }
        
        /// <summary>
        /// Gets or sets the vertical position of an object.
        /// </summary>
        public int YPos { get; set; }
        
        /// <summary>
        /// Gets or sets the width of the object, typically measured in pixels.
        /// </summary>
        public int Width { get; set; }
        
        /// <summary>
        /// Gets or sets the height of the object.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the web part group associated with the entity.
        /// </summary>
        public Guid? WpGroupId { get; set; }

    }
}
