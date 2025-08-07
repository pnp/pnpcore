using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// This class is used to instantiate controls of type 1 (= emptySection).
    /// </summary>
    public interface IEmptySection : ICanvasControl
    {
        /// <summary>
        /// Builds Control attributes based on current settings, also used by pnpframework
        /// </summary>
        public void BuildControlData(float controlIndex);
    }
}
