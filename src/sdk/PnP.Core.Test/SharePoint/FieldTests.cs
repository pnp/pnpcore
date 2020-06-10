using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class FieldTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetWebFieldsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                IWeb web = await context.Web.GetAsync(p => p.Fields);
                Assert.IsTrue(web.Fields.Count() > 0);

                IField field = web.Fields.FirstOrDefault(p => p.InternalName == "Title");
                // Test a string property
                Assert.AreEqual("Title", field.InternalName);
                // Test a boolean property
                Assert.IsFalse(field.Hidden);
                // Test special types
                Assert.AreEqual(FieldType.Text, field.FieldTypeKind);
            }
        }

        [TestMethod]
        public void GetWebFieldByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                Guid titleFieldId = new Guid("fa564e0f-0c70-4ab9-b863-0177e6ddd247");
                IField field = (from f in context.Web.Fields
                                where f.Id == titleFieldId
                                select f).FirstOrDefault();

                Assert.IsNotNull(field);
                Assert.AreEqual("Title", field.Title);
            }
        }

        [TestMethod]
        public async Task AddWebFieldNoOptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddAsync("ADDED FIELD", FieldType.Text);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebFieldWithOptionsDefaultFormulaTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddAsync("ADDED FIELD", FieldType.Text, new BaseFieldAddOptions()
                {
                    Description = "TEST DESCRIPTION",
                    Group = "TEST GROUP",
                    DefaultFormula = @"=""DEFAULT""",
                    Hidden = true,
                    Indexed = true,
                    EnforceUniqueValues = true,
                    Required = true,
                    // TODO Check validation formula format
                    //ValidationFormula = 
                    ValidationMessage = "Invalid Value"
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST DESCRIPTION", addedField.Description);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(@"=""DEFAULT""", addedField.DefaultFormula);
                Assert.IsTrue(addedField.Hidden);
                Assert.IsTrue(addedField.Indexed);
                Assert.IsTrue(addedField.EnforceUniqueValues);
                Assert.IsTrue(addedField.Required);
                Assert.AreEqual("Invalid Value", addedField.ValidationMessage);


                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdateWebFieldTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                IField field = await context.Web.Fields.AddAsync("TO UPDATE FIELD", FieldType.Text);

                // Test if the content type is created
                Assert.IsNotNull(field);

                field.Title = "UPDATED";
                await field.UpdateAsync();

                // Test if the content type is still found
                IField fieldToFind = (from ct in context.Web.Fields
                                      where ct.Title == "UPDATED"
                                      select ct).FirstOrDefault();

                Assert.IsNotNull(fieldToFind);

                await fieldToFind.DeleteAsync();

            }
        }

        [TestMethod]
        public async Task DeleteWebFieldTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                IField field = await context.Web.Fields.AddAsync("TO DELETE FIELD", FieldType.Text);

                // Test if the content type is created
                Assert.IsNotNull(field);

                await field.DeleteAsync();

                // Test if the content type is still found
                IField fieldToFind = (from ct in context.Web.Fields
                                      where ct.Title == "TO DELETE FIELD"
                                      select ct).FirstOrDefault();

                Assert.IsNull(fieldToFind);
            }
        }
    }
}
