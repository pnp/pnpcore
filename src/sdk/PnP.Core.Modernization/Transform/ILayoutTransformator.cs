using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Pages;
using System;
using System.Collections.Generic;

namespace PnP.Core.Modernization.Transform
{
    /// <summary>
    /// Interface implemented by all layout transformators
    /// </summary>
    public interface ILayoutTransformator
    {
        /// <summary>
        /// Transforms a classic wiki/webpart page layout into a modern client side page layout
        /// </summary>
        /// <param name="pageData">Information about the analyed page</param>
        void Transform(Tuple<PageLayout, List<WebPartEntity>> pageData);
    }
}
