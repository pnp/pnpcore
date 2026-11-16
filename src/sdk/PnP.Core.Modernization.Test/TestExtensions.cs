using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PnP.Core.Modernization
{
    public static class TestExtensions
    {
        /// <summary>
        /// Fail test if collection has no items
        /// </summary>
        /// <param name="collection"></param>
        public static void FailTestIfZero(this ListItemCollection collection)
        {
            if (collection == null || collection.Count == 0)
            {
                Assert.Fail("No pages found");
            }
        }
    }
}
