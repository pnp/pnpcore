using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// This class is used to instantiate controls of type 1 (= emptySection).
    /// </summary>
    public interface IEmptySection : ICanvasControl
    {

        /// <summary>
        /// Type of the control (= <see cref="IEmptySection"/>)
        /// </summary>
        public new Type Type { get; }
    }
}
