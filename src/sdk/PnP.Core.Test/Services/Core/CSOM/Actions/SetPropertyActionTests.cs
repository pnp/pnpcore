using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services.Core.CSOM.QueryAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.Services.Core.CSOM.Actions
{
    [TestClass]
    public class SetPropertyActionTests
    {
        [TestMethod]
        public void SetPropertyAction_Test_SetGuidProperty()
        {
            SetPropertyAction action = new SetPropertyAction()
            {
                Id = 1,
                ObjectPathId = "11",
                Name = "SspId",
                SetParameter = new Parameter()
                {
                    Value = Guid.Parse("1e1a939f-60b2-2000-98a6-d25d3d400a3a"),
                    Type = typeof(Guid).Name
                }
            };

            string expectedString = "<SetProperty Id=\"1\" ObjectPathId=\"11\" Name=\"SspId\"><Parameter Type=\"Guid\">{1e1a939f-60b2-2000-98a6-d25d3d400a3a}</Parameter></SetProperty>";
            string actualString = action.ToString();

            Assert.AreEqual(expectedString, actualString);
        }
    }
}
