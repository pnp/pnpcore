using System;

namespace PnP.Core.Model.SharePoint
{
    internal class FilePreview : IFilePreview
    {
        public string GetUrl { get; set; }

        public string PostUrl { get; set; }

        public string PostParameters { get; set; }
    }
}
