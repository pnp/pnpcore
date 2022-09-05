using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System;
using System.Collections.Generic;

namespace PnP.Core.Test.Services.Core.CSOM.Utils
{
    [TestClass]
    public class CSOMItemFieldTests
    {
        [TestMethod]
        public void CSOMItemField_Test_GetRequestParameters_Text()
        {
            CSOMItemField field = new CSOMItemField()
            {
                FieldName = "SomeTestField",
                FieldType = "String",
                FieldValue = "Test field value"
            };
            var parameters = field.GetRequestParameters();
            string parameterDeclaration = parameters[0].SerializeParameter();
            string parameterValue = parameters[1].SerializeParameter();

            Assert.AreEqual("<Parameter Type=\"String\">SomeTestField</Parameter>", parameterDeclaration);
            Assert.AreEqual("<Parameter Type=\"String\">Test field value</Parameter>", parameterValue);
        }
        [TestMethod]
        public void CSOMItemField_Test_GetRequestParameters_Text_NoTypePassed()
        {
            CSOMItemField field = new CSOMItemField()
            {
                FieldName = "SomeTestField",
                FieldValue = "Test field value"
            };
            var parameters = field.GetRequestParameters();
            string parameterDeclaration = parameters[0].SerializeParameter();
            string parameterValue = parameters[1].SerializeParameter();

            Assert.AreEqual("<Parameter Type=\"String\">SomeTestField</Parameter>", parameterDeclaration);
            Assert.AreEqual("<Parameter Type=\"String\">Test field value</Parameter>", parameterValue);
        }
        [TestMethod]
        public void CSOMItemField_Test_GetRequestParameters_Int()
        {
            CSOMItemField field = new CSOMItemField()
            {
                FieldName = "SomeTestField",
                FieldType = "Int",
                FieldValue = 123
            };
            var parameters = field.GetRequestParameters();
            string parameterDeclaration = parameters[0].SerializeParameter();
            string parameterValue = parameters[1].SerializeParameter();

            Assert.AreEqual("<Parameter Type=\"String\">SomeTestField</Parameter>", parameterDeclaration);
            Assert.AreEqual("<Parameter Type=\"Int\">123</Parameter>", parameterValue);
        }
        [TestMethod]
        public void CSOMItemField_Test_GetRequestParameters_Int_NoTypePassed()
        {
            CSOMItemField field = new CSOMItemField()
            {
                FieldName = "SomeTestField",
                FieldValue = 123
            };
            var parameters = field.GetRequestParameters();
            string parameterDeclaration = parameters[0].SerializeParameter();
            string parameterValue = parameters[1].SerializeParameter();

            Assert.AreEqual("<Parameter Type=\"String\">SomeTestField</Parameter>", parameterDeclaration);
            Assert.AreEqual("<Parameter Type=\"Int32\">123</Parameter>", parameterValue);
        }
        [TestMethod]
        public void CSOMItemField_Test_GetRequestParameters_Boolean()
        {
            CSOMItemField field = new CSOMItemField()
            {
                FieldName = "SomeTestField",
                FieldValue = true
            };
            var parameters = field.GetRequestParameters();
            string parameterDeclaration = parameters[0].SerializeParameter();
            string parameterValue = parameters[1].SerializeParameter();

            Assert.AreEqual("<Parameter Type=\"String\">SomeTestField</Parameter>", parameterDeclaration);
            Assert.AreEqual("<Parameter Type=\"Boolean\">true</Parameter>", parameterValue);
        }
        [TestMethod]
        public void CSOMItemField_Test_GetRequestParameters_Choice()
        {
            CSOMItemField field = new CSOMItemField()
            {
                FieldName = "SomeTestField",
                FieldValue = "Test Choice 1"
            };
            var parameters = field.GetRequestParameters();
            string parameterDeclaration = parameters[0].SerializeParameter();
            string parameterValue = parameters[1].SerializeParameter();

            Assert.AreEqual("<Parameter Type=\"String\">SomeTestField</Parameter>", parameterDeclaration);
            Assert.AreEqual("<Parameter Type=\"String\">Test Choice 1</Parameter>", parameterValue);
        }
        [TestMethod]
        public void CSOMItemField_Test_GetRequestParameters_MultiChoice()
        {
            CSOMItemField field = new CSOMItemField()
            {
                FieldName = "SomeTestField",
                FieldValue = new List<string>() { "Choice 1", "Choice 2" }
            };
            var parameters = field.GetRequestParameters();
            string parameterDeclaration = parameters[0].SerializeParameter();
            string parameterValue = parameters[1].SerializeParameter();

            Assert.AreEqual("<Parameter Type=\"String\">SomeTestField</Parameter>", parameterDeclaration);
            Assert.AreEqual("<Parameter Type=\"Array\"><Object Type=\"String\">Choice 1</Object><Object Type=\"String\">Choice 2</Object></Parameter>", parameterValue);
        }
        [TestMethod]
        public void CSOMItemField_Test_GetRequestParameters_Lookup()
        {
            CSOMItemField field = new CSOMItemField()
            {
                FieldName = "SomeTestField",
                FieldValue = new FieldLookupValue()
                {
                    LookupId = 2
                }
            };
            var parameters = field.GetRequestParameters();
            string parameterDeclaration = parameters[0].SerializeParameter();
            string parameterValue = parameters[1].SerializeParameter();

            Assert.AreEqual("<Parameter Type=\"String\">SomeTestField</Parameter>", parameterDeclaration);
            Assert.AreEqual("<Parameter TypeId=\"{f1d34cc0-9b50-4a78-be78-d5facfcccfb7}\"><Property Name=\"LookupId\" Type=\"Int32\">2</Property><Property Name=\"LookupValue\" Type=\"Null\" /></Parameter>", parameterValue);
        }

        [TestMethod]
        public void CSOMItemField_Test_GetRequestParameters_LookupMulti()
        {
            FieldValueCollection lookupCollection = new FieldValueCollection(new Field()
            {
                TypeAsString = "LookupMulti"
            }, "SomeTestField");
            lookupCollection.Values.Add(new FieldLookupValue()
            {
                LookupId = 2
            });
            lookupCollection.Values.Add(new FieldLookupValue()
            {
                LookupId = 3
            });
            CSOMItemField field = new CSOMItemField()
            {
                FieldName = "SomeTestField",
                FieldValue = lookupCollection
            };
            var parameters = field.GetRequestParameters();
            string parameterDeclaration = parameters[0].SerializeParameter();
            string parameterValue = parameters[1].SerializeParameter();

            Assert.AreEqual("<Parameter Type=\"String\">SomeTestField</Parameter>", parameterDeclaration);
            Assert.AreEqual($"<Parameter Type=\"Array\">" +
               "<Object TypeId=\"{f1d34cc0-9b50-4a78-be78-d5facfcccfb7}\">" +
                  "<Property Name=\"LookupId\" Type=\"Int32\">2</Property>" +
                  "<Property Name=\"LookupValue\" Type=\"Null\" />" +
               "</Object>" +
               "<Object TypeId=\"{f1d34cc0-9b50-4a78-be78-d5facfcccfb7}\">" +
                  "<Property Name=\"LookupId\" Type=\"Int32\">3</Property>" +
                  "<Property Name=\"LookupValue\" Type=\"Null\" />" +
               "</Object>" +
            "</Parameter>", parameterValue);
        }
        [TestMethod]
        public void CSOMItemField_Test_GetRequestParameters_User()
        {
            CSOMItemField field = new CSOMItemField()
            {
                FieldName = "SomeTestField",
                FieldValue = new FieldUserValue()
                {
                    LookupId = 2
                }
            };
            var parameters = field.GetRequestParameters();
            string parameterDeclaration = parameters[0].SerializeParameter();
            string parameterValue = parameters[1].SerializeParameter();

            Assert.AreEqual("<Parameter Type=\"String\">SomeTestField</Parameter>", parameterDeclaration);
            Assert.AreEqual("<Parameter TypeId=\"{c956ab54-16bd-4c18-89d2-996f57282a6f}\"><Property Name=\"Email\" Type=\"Null\" /><Property Name=\"LookupId\" Type=\"Int32\">2</Property><Property Name=\"LookupValue\" Type=\"Null\" /></Parameter>", parameterValue);
        }
        [TestMethod]
        public void CSOMItemField_Test_GetRequestParameters_Url()
        {
            CSOMItemField field = new CSOMItemField()
            {
                FieldName = "SomeTestField",
                FieldValue = new FieldUrlValue()
                {
                    Url = "https://test.sharepoint.com/sites/test",
                    Description = "Test description"
                }
            };
            var parameters = field.GetRequestParameters();
            string parameterDeclaration = parameters[0].SerializeParameter();
            string parameterValue = parameters[1].SerializeParameter();

            Assert.AreEqual("<Parameter Type=\"String\">SomeTestField</Parameter>", parameterDeclaration);
            Assert.AreEqual("<Parameter TypeId=\"{fa8b44af-7b43-43f2-904a-bd319497011e}\"><Property Name=\"Url\" Type=\"String\">https://test.sharepoint.com/sites/test</Property><Property Name=\"Description\" Type=\"String\">Test description</Property></Parameter>", parameterValue);
        }
        [TestMethod]
        public void CSOMItemField_Test_GetRequestParameters_Taxonomy()
        {
            CSOMItemField field = new CSOMItemField()
            {
                FieldName = "SomeTestField",
                FieldValue = new FieldTaxonomyValue()
                {
                    Label = "Plant 1",
                    TermId = Guid.Parse("988c6f0b-89e6-4e26-b5e8-a5de1e7d09f1"),
                    WssId = -1
                }
            };
            var parameters = field.GetRequestParameters(2);
            string parameterDeclaration = parameters[0].SerializeParameter();
            string parameterValue = parameters[1].SerializeParameter();

            Assert.AreEqual("<Parameter ObjectPathId=\"2\" />", parameterDeclaration);
            Assert.AreEqual("<Parameter TypeId=\"{19e70ed0-4177-456b-8156-015e4d163ff8}\"><Property Name=\"Label\" Type=\"String\">Plant 1</Property><Property Name=\"TermGuid\" Type=\"String\">988c6f0b-89e6-4e26-b5e8-a5de1e7d09f1</Property><Property Name=\"WssId\" Type=\"Int32\">-1</Property></Parameter>", parameterValue);
        }
        [TestMethod]
        public void CSOMItemField_Test_GetRequestParameters_TaxonomyMulti()
        {
            FieldValueCollection taxFieldCollection = new FieldValueCollection(new Field()
            {
                TypeAsString = "TaxonomyFieldTypeMulti"
            }, "SomeTestField");
            taxFieldCollection.Values.Add(new FieldTaxonomyValue()
            {
                Label = "Legal",
                WssId = -1,
                TermId = Guid.Parse("4a699f99-8a47-40f3-8ae3-656e468d7861")
            });
            taxFieldCollection.Values.Add(new FieldTaxonomyValue()
            {
                Label = "People",
                WssId = -1,
                TermId = Guid.Parse("45193e31-5fed-4662-b51d-09ed6037ff37")
            });
            CSOMItemField field = new CSOMItemField()
            {
                FieldName = "SomeTestField",
                FieldValue = taxFieldCollection
            };
            var parameters = field.GetRequestParameters(1);
            string parameterReference = parameters[0].SerializeParameter();
            string parameterValue = parameters[1].SerializeParameter();

            Assert.AreEqual("<Parameter Type=\"String\">-1;#Legal|4a699f99-8a47-40f3-8ae3-656e468d7861;#-1;#People|45193e31-5fed-4662-b51d-09ed6037ff37</Parameter>", parameterValue);
            Assert.AreEqual("<Parameter ObjectPathId=\"1\" />", parameterReference);
        }
    }
}
