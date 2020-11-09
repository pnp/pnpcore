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

        /// <summary>
        /// Removes the control from the page
        /// </summary>
        void Delete();

        /// <summary>
        /// Moves the control to another section and column
        /// </summary>
        /// <param name="newSection">New section that will host the control</param>
        void Move(ICanvasSection newSection);

        /// <summary>
        /// Moves the control to another section and column
        /// </summary>
        /// <param name="newSection">New section that will host the control</param>
        /// <param name="order">New order for the control in the new section</param>
        void Move(ICanvasSection newSection, int order);

        /// <summary>
        /// Moves the control to another section and column
        /// </summary>
        /// <param name="newColumn">New column that will host the control</param>
        void Move(ICanvasColumn newColumn);

        /// <summary>
        /// Moves the control to another section and column
        /// </summary>
        /// <param name="newColumn">New column that will host the control</param>
        /// <param name="order">New order for the control in the new column</param>
        void Move(ICanvasColumn newColumn, int order);

        /// <summary>
        /// Moves the control to another section and column while keeping it's current position
        /// </summary>
        /// <param name="newSection">New section that will host the control</param>
        void MovePosition(ICanvasSection newSection);

        /// <summary>
        /// Moves the control to another section and column in the given position
        /// </summary>
        /// <param name="newSection">New section that will host the control</param>
        /// <param name="position">New position for the control in the new section</param>
        void MovePosition(ICanvasSection newSection, int position);

        /// <summary>
        /// Moves the control to another section and column while keeping it's current position
        /// </summary>
        /// <param name="newColumn">New column that will host the control</param>
        void MovePosition(ICanvasColumn newColumn);

        /// <summary>
        /// Moves the control to another section and column in the given position
        /// </summary>
        /// <param name="newColumn">New column that will host the control</param>
        /// <param name="position">New position for the control in the new column</param>
        void MovePosition(ICanvasColumn newColumn, int position);

        /// <summary>
        /// Moves the control inside the current column to a new position
        /// </summary>
        /// <param name="position">New position for this control</param>
        void MovePosition(int position);
    }
}
