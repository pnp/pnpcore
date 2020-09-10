using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.ASPNetCore.Models
{
    public class ListInfoViewModel
    {
        public string SiteTitle { get; set; }

        public List<IList> Lists { get; set; }
    }
}
