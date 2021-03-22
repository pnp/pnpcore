using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.Services.Core.CSOM.Actions
{
    [TestClass]
    public class ContentTypeCreationParameterTests
    {
        [TestMethod]
        public void ContentTypeCreationParameter_Test_Serialization()
        {
            ContentTypeCreationParameter parameter = new ContentTypeCreationParameter()
            {
                Value = new ContentTypeCreationInfo()
                {
                    Id = "0x10023213123123",
                    Description = "Test Description",
                    Group = "Test Group",
                    Name = "Test Name",
                }
            };
            string expectedParameter = "<Parameter TypeId=\"{168f3091-4554-4f14-8866-b20d48e45b54}\"><Property Name=\"Description\" Type=\"String\">Test Description</Property><Property Name=\"Group\" Type=\"String\">Test Group</Property><Property Name=\"Id\" Type=\"String\">0x10023213123123</Property><Property Name=\"Name\" Type=\"String\">Test Name</Property><Property Name=\"ParentContentType\" Type=\"Null\" /></Parameter>";

            Assert.AreEqual(expectedParameter, parameter.SerializeParameter());
        }
        [TestMethod]
        public void ContentTypeCreationParameter_Test_EmptyDescription()
        {
            ContentTypeCreationParameter parameter = new ContentTypeCreationParameter()
            {
                Value = new ContentTypeCreationInfo()
                {
                    Id = "0x10023213123123",
                    Group = "Test Group",
                    Name = "Test Name",
                }
            };
            string expectedParameter = "<Parameter TypeId=\"{168f3091-4554-4f14-8866-b20d48e45b54}\"><Property Name=\"Description\" Type=\"Null\" /><Property Name=\"Group\" Type=\"String\">Test Group</Property><Property Name=\"Id\" Type=\"String\">0x10023213123123</Property><Property Name=\"Name\" Type=\"String\">Test Name</Property><Property Name=\"ParentContentType\" Type=\"Null\" /></Parameter>";

            Assert.AreEqual(expectedParameter, parameter.SerializeParameter());
        }
        [TestMethod]
        public void ContentTypeCreationParameter_Test_EmptyGroup()
        {
            ContentTypeCreationParameter parameter = new ContentTypeCreationParameter()
            {
                Value = new ContentTypeCreationInfo()
                {
                    Id = "0x10023213123123",
                    Description = "Test Description",
                    Name = "Test Name",
                }
            };
            string expectedParameter = "<Parameter TypeId=\"{168f3091-4554-4f14-8866-b20d48e45b54}\"><Property Name=\"Description\" Type=\"String\">Test Description</Property><Property Name=\"Group\" Type=\"Null\" /><Property Name=\"Id\" Type=\"String\">0x10023213123123</Property><Property Name=\"Name\" Type=\"String\">Test Name</Property><Property Name=\"ParentContentType\" Type=\"Null\" /></Parameter>";

            Assert.AreEqual(expectedParameter, parameter.SerializeParameter());
        }
    }
}
