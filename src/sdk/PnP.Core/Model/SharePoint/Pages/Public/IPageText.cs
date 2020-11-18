using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Text web part
    /// </summary>
    public interface IPageText : ICanvasControl
    {
        /// <summary>
        /// Text value of the client side text control
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Text used in page preview in news web part
        /// </summary>
        public string PreviewText { get; }

        /// <summary>
        /// Type of the control (= <see cref="IPageText"/>)
        /// </summary>
        public new Type Type { get; }

    }
}
