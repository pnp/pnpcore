using AngleSharp.Dom;
using System;
using System.Linq;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Base class for a canvas control 
    /// </summary>
    internal abstract class CanvasControl : ICanvasControl
    {
        #region variables
        internal const string CanvasControlAttribute = "data-sp-canvascontrol";
        internal const string CanvasDataVersionAttribute = "data-sp-canvasdataversion";
        internal const string ControlDataAttribute = "data-sp-controldata";

        internal int order;
        internal int controlType;
        internal string jsonControlData;
        internal string dataVersion;
        internal string canvasDataVersion;
        internal string canvasControlData;
        internal Guid instanceId;
        internal ICanvasSection section;
        internal ICanvasColumn column;
        #endregion

        #region construction
        /// <summary>
        /// Constructs the canvas control
        /// </summary>
        internal CanvasControl()
        {
            dataVersion = "1.0";
            canvasDataVersion = "1.0";
            instanceId = Guid.NewGuid();
            canvasControlData = "";
            order = 0;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The <see cref="ICanvasSection"/> hosting  this control
        /// </summary>
        public ICanvasSection Section
        {
            get
            {
                return section;
            }
        }

        /// <summary>
        /// The <see cref="ICanvasColumn"/> hosting this control
        /// </summary>
        public ICanvasColumn Column
        {
            get
            {
                return column;
            }
        }

        /// <summary>
        /// The internal storage version used for this control
        /// </summary>
        public string DataVersion
        {
            get
            {
                return dataVersion;
            }
            set
            {
                dataVersion = value;
            }
        }

        /// <summary>
        /// The internal canvas storage version used
        /// </summary>
        public string CanvasDataVersion
        {
            get
            {
                return canvasDataVersion;
            }
        }

        /// <summary>
        /// Value of the control's "data-sp-canvascontrol" attribute
        /// </summary>
        public string CanvasControlData
        {
            get
            {
                return canvasControlData;
            }
        }

        /// <summary>
        /// Type of the control: 4 is a text part, 3 is a client side web part
        /// </summary>
        public int ControlType
        {
            get
            {
                return controlType;
            }
        }

        /// <summary>
        /// Value of the control's "data-sp-controldata" attribute
        /// </summary>
        public string JsonControlData
        {
            get
            {
                return jsonControlData;
            }
        }

        /// <summary>
        /// Instance ID of the control
        /// </summary>
        public Guid InstanceId
        {
            get
            {
                return instanceId;
            }
            set
            {
                instanceId = value;
            }
        }

        /// <summary>
        /// Order of the control in the control collection
        /// </summary>
        public int Order
        {
            get
            {
                return order;
            }
            set
            {
                order = value;
            }
        }

        /// <summary>
        /// Type if the control (<see cref="IPageText"/> or <see cref="IPageWebPart"/>)
        /// </summary>
        public abstract Type Type { get; }
        #endregion

        #region public methods
        /// <summary>
        /// Converts a control object to it's html representation
        /// </summary>
        /// <param name="controlIndex">The sequence of the control inside the section</param>
        /// <returns>Html representation of a control</returns>
        public abstract string ToHtml(float controlIndex);

        /// <summary>
        /// Removes the control from the page
        /// </summary>
        public void Delete()
        {
            Column.Section.Page.Controls.Remove(this);
        }

        /// <summary>
        /// Moves the control to another section and column
        /// </summary>
        /// <param name="newSection">New section that will host the control</param>
        public void Move(ICanvasSection newSection)
        {
            section = newSection;
            column = newSection.DefaultColumn;
        }

        /// <summary>
        /// Moves the control to another section and column
        /// </summary>
        /// <param name="newSection">New section that will host the control</param>
        /// <param name="order">New order for the control in the new section</param>
        public void Move(ICanvasSection newSection, int order)
        {
            Move(newSection);
            this.order = order;
        }

        /// <summary>
        /// Moves the control to another section and column
        /// </summary>
        /// <param name="newColumn">New column that will host the control</param>
        public void Move(ICanvasColumn newColumn)
        {
            section = newColumn.Section;
            column = newColumn;
        }

        /// <summary>
        /// Moves the control to another section and column
        /// </summary>
        /// <param name="newColumn">New column that will host the control</param>
        /// <param name="order">New order for the control in the new column</param>
        public void Move(ICanvasColumn newColumn, int order)
        {
            Move(newColumn);
            this.order = order;
        }

        /// <summary>
        /// Moves the control to another section and column while keeping it's current position
        /// </summary>
        /// <param name="newSection">New section that will host the control</param>
        public void MovePosition(ICanvasSection newSection)
        {
            var currentSection = Section;
            section = newSection;
            column = newSection.DefaultColumn;
            ReindexSection(currentSection);
            ReindexSection(Section);
        }

        /// <summary>
        /// Moves the control to another section and column in the given position
        /// </summary>
        /// <param name="newSection">New section that will host the control</param>
        /// <param name="position">New position for the control in the new section</param>
        public void MovePosition(ICanvasSection newSection, int position)
        {
            var currentSection = Section;
            MovePosition(newSection);
            ReindexSection(currentSection);
            MovePosition(position);
        }

        /// <summary>
        /// Moves the control to another section and column while keeping it's current position
        /// </summary>
        /// <param name="newColumn">New column that will host the control</param>
        public void MovePosition(ICanvasColumn newColumn)
        {
            var currentColumn = Column;
            section = newColumn.Section;
            column = newColumn;
            ReindexColumn(currentColumn);
            ReindexColumn(Column);
        }

        /// <summary>
        /// Moves the control to another section and column in the given position
        /// </summary>
        /// <param name="newColumn">New column that will host the control</param>
        /// <param name="position">New position for the control in the new column</param>
        public void MovePosition(ICanvasColumn newColumn, int position)
        {
            var currentColumn = Column;
            MovePosition(newColumn);
            ReindexColumn(currentColumn);
            MovePosition(position);
        }

        /// <summary>
        /// Moves the control inside the current column to a new position
        /// </summary>
        /// <param name="position">New position for this control</param>
        public void MovePosition(int position)
        {
            // Ensure we're having a clean sequence before starting
            ReindexColumn();

            if (position > Order)
            {
                position++;
            }

            foreach (var control in section.Page.Controls.Where(c => c.Section == section && c.Column == column && c.Order >= position).OrderBy(p => p.Order))
            {
                control.Order++;
            }
            Order = position;

            // Ensure we're having a clean sequence to return
            ReindexColumn();
        }

        private void ReindexColumn()
        {
            ReindexColumn(Column);
        }

        private void ReindexColumn(ICanvasColumn column)
        {
            var index = 0;
            foreach (var control in this.column.Section.Page.Controls.Where(c => c.Section == column.Section && c.Column == column).OrderBy(c => c.Order))
            {
                index++;
                (control as CanvasControl).order = index;
            }
        }

        private void ReindexSection(ICanvasSection section)
        {
            foreach (var column in section.Columns)
            {
                ReindexColumn(column);
            }
        }

        /// <summary>
        /// Receives "data-sp-controldata" content and detects the type of the control
        /// </summary>
        /// <param name="controlDataJson">data-sp-controldata json string</param>
        /// <returns>Type of the control represented by the json string</returns>
        internal static Type GetType(string controlDataJson)
        {
            if (controlDataJson == null)
            {
                throw new ArgumentNullException(nameof(controlDataJson));
            }

            // Deserialize the json string
            var controlData = JsonSerializer.Deserialize<CanvasControlData>(controlDataJson, PnPConstants.JsonSerializer_IgnoreNullValues);

            if (controlData.ControlType == 3)
            {
                return typeof(PageWebPart);
            }
            else if (controlData.ControlType == 4)
            {
                return typeof(PageText);
            }
            else if (controlData.ControlType == 0)
            {
                return typeof(CanvasColumn);
            }
            else if (controlData.ControlType == 14) // Special Control Type used for section background image
            {
                return typeof(SectionBackgroundControl);
            }

            return null;
        }
        #endregion

        #region Internal and private methods
        internal virtual void FromHtml(IElement element, bool isHeader)
        {
            var controlData = JsonSerializer.Deserialize<CanvasControlData>(element.GetAttribute(ControlDataAttribute), PnPConstants.JsonSerializer_IgnoreNullValues);

            // populate base object
            canvasDataVersion = element.GetAttribute(CanvasDataVersionAttribute);
            canvasControlData = element.GetAttribute(CanvasControlAttribute);
            controlType = controlData.ControlType;
            instanceId = new Guid(controlData.Id ?? Guid.NewGuid().ToString());
        }

        internal void MoveTo(ICanvasSection newSection, ICanvasColumn newColumn)
        {
            section = newSection;
            column = newColumn;
        }

        #endregion
    }
}
