using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Base interface for a canvas control 
    /// </summary>
    public interface ICanvasControl
    {
        /// <summary>
        /// The <see cref="ICanvasSection"/> hosting  this control
        /// </summary>
        ICanvasSection Section { get; }

        /// <summary>
        /// The <see cref="ICanvasColumn"/> hosting this control
        /// </summary>
        ICanvasColumn Column { get; }

        /// <summary>
        /// The internal storage version used for this control
        /// </summary>
        string DataVersion { get; }

        /// <summary>
        /// The internal canvas storage version used
        /// </summary>
        string CanvasDataVersion { get; }

        /// <summary>
        /// Value of the control's "data-sp-canvascontrol" attribute
        /// </summary>
        string CanvasControlData { get; }

        /// <summary>
        /// Type of the control: 4 is a text part, 3 is a client side web part
        /// </summary>
        int ControlType { get; }

        /// <summary>
        /// Value of the control's "data-sp-controldata" attribute
        /// </summary>
        string JsonControlData { get; }

        /// <summary>
        /// Instance ID of the control
        /// </summary>
        Guid InstanceId { get; }

        /// <summary>
        /// Order of the control in the control collection
        /// </summary>
        int Order { get; set; }

        /// <summary>
        /// Type if the control (<see cref="IClientSideText"/> or <see cref="IClientSideWebPart"/>)
        /// </summary>
        Type Type { get; }
    }
}
