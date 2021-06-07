using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.SharePoint.Model
{
    /// <summary>
    /// Represents a placeholder for a classic Web Part to be transformed
    /// </summary>
    internal class WebPartPlaceHolder
    {
        public string Id { get; set; }
        public string ControlId { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int Order { get; set; }
        public WebPartDefinition WebPartDefinition { get; set; }
        public string WebPartXmlOnPremises { get; set; }
        public ClientResult<string> WebPartXml { get; set; }

        public string WebPartType { get; set; }
    }
}
