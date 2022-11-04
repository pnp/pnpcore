using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class ListFieldTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetListFieldsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.GetByTitle("Documents", l => l.Fields);
                Assert.IsTrue(documents.Fields.Length > 0);

                IField field = documents.Fields.AsRequested().FirstOrDefault(p => p.InternalName == "Title");
                // Test a string property
                Assert.AreEqual("Title", field.InternalName);
                // Test a boolean property
                Assert.IsFalse(field.Hidden);
                // Test special types
                Assert.AreEqual(FieldType.Text, field.FieldTypeKind);
            }
        }

        [TestMethod]
        public async Task GetListFieldByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                //await context.Web.GetAsync(p => p.Lists);
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                Guid titleFieldId = new Guid("fa564e0f-0c70-4ab9-b863-0177e6ddd247");
                IField field = (from f in documents.Fields
                                where f.Id == titleFieldId
                                select f).FirstOrDefault();

                Assert.IsNotNull(field);
                Assert.AreEqual("Title", field.Title);
            }
        }

        [TestMethod]
        public async Task AddListFieldNoOptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddTextAsync("ADDED FIELD", new FieldTextOptions());

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldWithOptionsDefaultFormulaTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddTextAsync("ADDED FIELD", new FieldTextOptions()
                {
                    Description = "TEST DESCRIPTION",
                    Group = "TEST GROUP",
                    DefaultFormula = @"=""DEFAULT""",
                    Indexed = true,
                    EnforceUniqueValues = true,
                    Required = true,
                    ValidationFormula = @"=ISNUMBER(5)",
                    ValidationMessage = "Invalid Value",
                    // TODO Check why hidden list fields cannot be deleted the regular way
                    //{"error":{"code":"-2130575214, Microsoft.SharePoint.SPException","message":{"lang":"en-US","value":"You cannot delete a hidden column."}}}
                    //Hidden = true,
                }); ;

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST DESCRIPTION", addedField.Description);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(@"=""DEFAULT""", addedField.DefaultFormula);
                Assert.IsTrue(addedField.Indexed);
                Assert.IsTrue(addedField.EnforceUniqueValues);
                Assert.IsTrue(addedField.Required);
                Assert.AreEqual("Invalid Value", addedField.ValidationMessage);
                Assert.AreEqual(@"=ISNUMBER(5)", addedField.ValidationFormula);

                await addedField.DeleteAsync();
            }
        }

        #region AddListFieldAsXml Tests

        [TestMethod]
        public async Task AddListFieldAsXmlAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddFieldAsXmlAsync(@"<Field Type=""Text"" Name=""ADDEDFIELD"" DisplayName=""ADDED FIELD""/>", true);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                // Internal name is not set with AddFieldAsXml on list fields
                //Assert.AreEqual("ADDEDFIELD", addedField.InternalName);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldAsXmlBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {

                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddFieldAsXmlBatchAsync(@"<Field Type=""Text"" Name=""ADDEDFIELD"" DisplayName=""ADDED FIELD""/>", true);
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                // Internal name is not set with AddFieldAsXml on list fields
                //Assert.AreEqual("ADDEDFIELD", addedField.InternalName);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldAsXmlSpecifiedBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Batch newBatch = context.NewBatch();

                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddFieldAsXmlBatchAsync(newBatch, @"<Field Type=""Text"" Name=""ADDEDFIELD"" DisplayName=""ADDED FIELD""/>", true);
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                // Internal name is not set with AddFieldAsXml on list fields
                //Assert.AreEqual("ADDEDFIELD", addedField.InternalName);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldAsXmlBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {

                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddFieldAsXmlBatch(@"<Field Type=""Text"" Name=""ADDEDFIELD"" DisplayName=""ADDED FIELD""/>", true);
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                // Internal name is not set with AddFieldAsXml on list fields
                //Assert.AreEqual("ADDEDFIELD", addedField.InternalName);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldAsXmlSpecifiedBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Batch newBatch = context.NewBatch();

                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddFieldAsXmlBatch(newBatch, @"<Field Type=""Text"" Name=""ADDEDFIELD"" DisplayName=""ADDED FIELD""/>", true);
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                // Internal name is not set with AddFieldAsXml on list fields
                //Assert.AreEqual("ADDEDFIELD", addedField.InternalName);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldAsXmlTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddFieldAsXml(@"<Field Type=""Text"" Name=""ADDEDFIELD"" DisplayName=""ADDED FIELD""/>", true);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                // Internal name is not set with AddFieldAsXml on list fields
                //Assert.AreEqual("ADDEDFIELD", addedField.InternalName);

                await addedField.DeleteAsync();
            }
        }

        #endregion

        [TestMethod]
        public async Task AddNewWebGenericFieldTextTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddTextAsync("ADDED FIELD", new FieldTextOptions()
                {
                    Group = "TEST GROUP",
                    MaxLength = 100
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                Assert.AreEqual(100, addedField.MaxLength);

                await addedField.DeleteAsync();
            }
        }

        // NOTE Specific methods MUST ALWAYS CALL the generic AddAsync with appropriate arguments, only the specific methods are test covered for each type of field

        #region AddNewListFieldText Tests

        [TestMethod]
        public async Task AddNewListFieldTextSpecificAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddTextAsync("ADDED FIELD", new FieldTextOptions()
                {
                    Group = "TEST GROUP",
                    MaxLength = 100
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                Assert.AreEqual(100, addedField.MaxLength);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddNewListFieldTextSpecificTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddText("ADDED FIELD", new FieldTextOptions()
                {
                    Group = "TEST GROUP",
                    MaxLength = 100
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                Assert.AreEqual(100, addedField.MaxLength);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddNewListFieldTextSpecificBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddTextBatch("ADDED FIELD", new FieldTextOptions()
                {
                    Group = "TEST GROUP",
                    MaxLength = 100
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                Assert.AreEqual(100, addedField.MaxLength);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddNewListFieldTextSpecificSpecificBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddTextBatch(newBatch, "ADDED FIELD", new FieldTextOptions()
                {
                    Group = "TEST GROUP",
                    MaxLength = 100
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                Assert.AreEqual(100, addedField.MaxLength);

                await addedField.DeleteAsync();
            }
        }

        #endregion 

        #region Add Method Testings Combinations

        [TestMethod]
        public async Task AddNewListFieldAddTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddText("ADDED FIELD", new FieldTextOptions()
                {
                    Group = "TEST GROUP",
                    MaxLength = 100
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                Assert.AreEqual(100, addedField.MaxLength);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddNewListFieldAddSpecificBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddTextBatch(newBatch, "ADDED FIELD", new FieldTextOptions()
                {
                    Group = "TEST GROUP",
                    MaxLength = 100
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                Assert.AreEqual(100, addedField.MaxLength);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddNewListFieldAddBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddTextBatch("ADDED FIELD", new FieldTextOptions()
                {
                    Group = "TEST GROUP",
                    MaxLength = 100
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                Assert.AreEqual(100, addedField.MaxLength);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddNewListFieldAddBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddTextBatchAsync("ADDED FIELD", new FieldTextOptions()
                {
                    Group = "TEST GROUP",
                    MaxLength = 100
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                Assert.AreEqual(100, addedField.MaxLength);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddNewListFieldExceptionsBatchAsyncTests()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");

                // Empty Title Test
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IField addedField = await documents.Fields.AddTextBatchAsync(string.Empty, new FieldTextOptions()
                    {
                        Group = "TEST GROUP",
                        MaxLength = 100
                    });
                    await context.ExecuteAsync();
                });

            }
        }

        [TestMethod]
        public async Task AddNewListFieldExceptionsAsyncTests()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");

                // Empty Title Test
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IField addedField = await documents.Fields.AddTextAsync(string.Empty, new FieldTextOptions()
                    {
                        Group = "TEST GROUP",
                        MaxLength = 100
                    });
                });
            }
        }

        #endregion

        #region AddListFieldMultilineText Tests

        [TestMethod]
        public async Task AddListFieldMultilineTextAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddMultilineTextAsync("ADDED FIELD", new FieldMultilineTextOptions()
                {
                    Group = "TEST GROUP",
                    AllowHyperlink = true,
                    AppendOnly = true,
                    NumberOfLines = 6,
                    RestrictedMode = true,
                    RichText = true,
                    UnlimitedLengthInDocumentLibrary = true
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Note, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.AllowHyperlink);
                Assert.IsTrue(addedField.AppendOnly);
                Assert.AreEqual(6, addedField.NumberOfLines);
                Assert.IsTrue(addedField.RestrictedMode);
                Assert.IsTrue(addedField.RichText);
                Assert.IsTrue(addedField.UnlimitedLengthInDocumentLibrary);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldMultilineTextTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddMultilineText("ADDED FIELD", new FieldMultilineTextOptions()
                {
                    Group = "TEST GROUP",
                    AllowHyperlink = true,
                    AppendOnly = true,
                    NumberOfLines = 6,
                    RestrictedMode = true,
                    RichText = true,
                    UnlimitedLengthInDocumentLibrary = true
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Note, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.AllowHyperlink);
                Assert.IsTrue(addedField.AppendOnly);
                Assert.AreEqual(6, addedField.NumberOfLines);
                Assert.IsTrue(addedField.RestrictedMode);
                Assert.IsTrue(addedField.RichText);
                Assert.IsTrue(addedField.UnlimitedLengthInDocumentLibrary);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldMultilineTextBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddMultilineTextBatchAsync("ADDED FIELD", new FieldMultilineTextOptions()
                {
                    Group = "TEST GROUP",
                    AllowHyperlink = true,
                    AppendOnly = true,
                    NumberOfLines = 6,
                    RestrictedMode = true,
                    RichText = true,
                    UnlimitedLengthInDocumentLibrary = true
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Note, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.AllowHyperlink);
                Assert.IsTrue(addedField.AppendOnly);
                Assert.AreEqual(6, addedField.NumberOfLines);
                Assert.IsTrue(addedField.RestrictedMode);
                Assert.IsTrue(addedField.RichText);
                Assert.IsTrue(addedField.UnlimitedLengthInDocumentLibrary);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldMultilineTextBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddMultilineTextBatch("ADDED FIELD", new FieldMultilineTextOptions()
                {
                    Group = "TEST GROUP",
                    AllowHyperlink = true,
                    AppendOnly = true,
                    NumberOfLines = 6,
                    RestrictedMode = true,
                    RichText = true,
                    UnlimitedLengthInDocumentLibrary = true
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Note, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.AllowHyperlink);
                Assert.IsTrue(addedField.AppendOnly);
                Assert.AreEqual(6, addedField.NumberOfLines);
                Assert.IsTrue(addedField.RestrictedMode);
                Assert.IsTrue(addedField.RichText);
                Assert.IsTrue(addedField.UnlimitedLengthInDocumentLibrary);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldMultilineTextSpecificBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddMultilineTextBatchAsync(newBatch, "ADDED FIELD", new FieldMultilineTextOptions()
                {
                    Group = "TEST GROUP",
                    AllowHyperlink = true,
                    AppendOnly = true,
                    NumberOfLines = 6,
                    RestrictedMode = true,
                    RichText = true,
                    UnlimitedLengthInDocumentLibrary = true
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Note, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.AllowHyperlink);
                Assert.IsTrue(addedField.AppendOnly);
                Assert.AreEqual(6, addedField.NumberOfLines);
                Assert.IsTrue(addedField.RestrictedMode);
                Assert.IsTrue(addedField.RichText);
                Assert.IsTrue(addedField.UnlimitedLengthInDocumentLibrary);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldMultilineTextSpecificBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddMultilineTextBatch(newBatch, "ADDED FIELD", new FieldMultilineTextOptions()
                {
                    Group = "TEST GROUP",
                    AllowHyperlink = true,
                    AppendOnly = true,
                    NumberOfLines = 6,
                    RestrictedMode = true,
                    RichText = true,
                    UnlimitedLengthInDocumentLibrary = true
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Note, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.AllowHyperlink);
                Assert.IsTrue(addedField.AppendOnly);
                Assert.AreEqual(6, addedField.NumberOfLines);
                Assert.IsTrue(addedField.RestrictedMode);
                Assert.IsTrue(addedField.RichText);
                Assert.IsTrue(addedField.UnlimitedLengthInDocumentLibrary);

                await addedField.DeleteAsync();
            }
        }

        #endregion

        #region AddListFieldDateTime Tests

        [TestMethod]
        public async Task AddListFieldDateTimeAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddDateTimeAsync("ADDED FIELD", new FieldDateTimeOptions()
                {
                    Group = "TEST GROUP",
                    DateTimeCalendarType = CalendarType.GregorianXLITFrench,
                    DisplayFormat = DateTimeFieldFormatType.DateTime,
                    FriendlyDisplayFormat = DateTimeFieldFriendlyFormatType.Relative
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.DateTime, addedField.FieldTypeKind);
                Assert.AreEqual(CalendarType.GregorianXLITFrench, addedField.DateTimeCalendarType);
                Assert.AreEqual((int)DateTimeFieldFormatType.DateTime, addedField.DisplayFormat);
                Assert.AreEqual(DateTimeFieldFriendlyFormatType.Relative, addedField.FriendlyDisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldDateTimeTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddDateTime("ADDED FIELD", new FieldDateTimeOptions()
                {
                    Group = "TEST GROUP",
                    DateTimeCalendarType = CalendarType.GregorianXLITFrench,
                    DisplayFormat = DateTimeFieldFormatType.DateTime,
                    FriendlyDisplayFormat = DateTimeFieldFriendlyFormatType.Relative
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.DateTime, addedField.FieldTypeKind);
                Assert.AreEqual(CalendarType.GregorianXLITFrench, addedField.DateTimeCalendarType);
                Assert.AreEqual((int)DateTimeFieldFormatType.DateTime, addedField.DisplayFormat);
                Assert.AreEqual(DateTimeFieldFriendlyFormatType.Relative, addedField.FriendlyDisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldDateTimeBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddDateTimeBatchAsync("ADDED FIELD", new FieldDateTimeOptions()
                {
                    Group = "TEST GROUP",
                    DateTimeCalendarType = CalendarType.GregorianXLITFrench,
                    DisplayFormat = DateTimeFieldFormatType.DateTime,
                    FriendlyDisplayFormat = DateTimeFieldFriendlyFormatType.Relative
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.DateTime, addedField.FieldTypeKind);
                Assert.AreEqual(CalendarType.GregorianXLITFrench, addedField.DateTimeCalendarType);
                Assert.AreEqual((int)DateTimeFieldFormatType.DateTime, addedField.DisplayFormat);
                Assert.AreEqual(DateTimeFieldFriendlyFormatType.Relative, addedField.FriendlyDisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldDateTimeBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddDateTimeBatch("ADDED FIELD", new FieldDateTimeOptions()
                {
                    Group = "TEST GROUP",
                    DateTimeCalendarType = CalendarType.GregorianXLITFrench,
                    DisplayFormat = DateTimeFieldFormatType.DateTime,
                    FriendlyDisplayFormat = DateTimeFieldFriendlyFormatType.Relative
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.DateTime, addedField.FieldTypeKind);
                Assert.AreEqual(CalendarType.GregorianXLITFrench, addedField.DateTimeCalendarType);
                Assert.AreEqual((int)DateTimeFieldFormatType.DateTime, addedField.DisplayFormat);
                Assert.AreEqual(DateTimeFieldFriendlyFormatType.Relative, addedField.FriendlyDisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldDateTimeSpecificBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddDateTimeBatchAsync(newBatch, "ADDED FIELD", new FieldDateTimeOptions()
                {
                    Group = "TEST GROUP",
                    DateTimeCalendarType = CalendarType.GregorianXLITFrench,
                    DisplayFormat = DateTimeFieldFormatType.DateTime,
                    FriendlyDisplayFormat = DateTimeFieldFriendlyFormatType.Relative
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.DateTime, addedField.FieldTypeKind);
                Assert.AreEqual(CalendarType.GregorianXLITFrench, addedField.DateTimeCalendarType);
                Assert.AreEqual((int)DateTimeFieldFormatType.DateTime, addedField.DisplayFormat);
                Assert.AreEqual(DateTimeFieldFriendlyFormatType.Relative, addedField.FriendlyDisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldDateTimeSpecificBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddDateTimeBatch(newBatch, "ADDED FIELD", new FieldDateTimeOptions()
                {
                    Group = "TEST GROUP",
                    DateTimeCalendarType = CalendarType.GregorianXLITFrench,
                    DisplayFormat = DateTimeFieldFormatType.DateTime,
                    FriendlyDisplayFormat = DateTimeFieldFriendlyFormatType.Relative
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.DateTime, addedField.FieldTypeKind);
                Assert.AreEqual(CalendarType.GregorianXLITFrench, addedField.DateTimeCalendarType);
                Assert.AreEqual((int)DateTimeFieldFormatType.DateTime, addedField.DisplayFormat);
                Assert.AreEqual(DateTimeFieldFriendlyFormatType.Relative, addedField.FriendlyDisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        #endregion

        #region AddListFieldMultiChoice Tests

        [TestMethod]
        public async Task AddListFieldMultiChoiceAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddChoiceMultiAsync("ADDED FIELD", new FieldChoiceMultiOptions()
                {
                    Group = "TEST GROUP",
                    FillInChoice = true,
                    Choices = new string[] { "CHOICE 1", "CHOICE 2" }
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.MultiChoice, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.FillInChoice);
                // Currently supports collections only for model types (JsonMappingHelper Ln 120)
                Assert.IsNotNull(addedField.Choices);
                Assert.AreEqual(2, addedField.Choices.Length);
                Assert.AreEqual("CHOICE 1", addedField.Choices[0]);
                Assert.AreEqual("CHOICE 2", addedField.Choices[1]);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldMultiChoiceTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddChoiceMulti("ADDED FIELD", new FieldChoiceMultiOptions()
                {
                    Group = "TEST GROUP",
                    FillInChoice = true,
                    Choices = new string[] { "CHOICE 1", "CHOICE 2" }
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.MultiChoice, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.FillInChoice);
                // Currently supports collections only for model types (JsonMappingHelper Ln 120)
                Assert.IsNotNull(addedField.Choices);
                Assert.AreEqual(2, addedField.Choices.Length);
                Assert.AreEqual("CHOICE 1", addedField.Choices[0]);
                Assert.AreEqual("CHOICE 2", addedField.Choices[1]);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldMultiChoiceBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddChoiceMultiBatchAsync("ADDED FIELD", new FieldChoiceMultiOptions()
                {
                    Group = "TEST GROUP",
                    FillInChoice = true,
                    Choices = new string[] { "CHOICE 1", "CHOICE 2" }
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.MultiChoice, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.FillInChoice);
                // Currently supports collections only for model types (JsonMappingHelper Ln 120)
                Assert.IsNotNull(addedField.Choices);
                Assert.AreEqual(2, addedField.Choices.Length);
                Assert.AreEqual("CHOICE 1", addedField.Choices[0]);
                Assert.AreEqual("CHOICE 2", addedField.Choices[1]);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldMultiChoiceBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddChoiceMultiBatch("ADDED FIELD", new FieldChoiceMultiOptions()
                {
                    Group = "TEST GROUP",
                    FillInChoice = true,
                    Choices = new string[] { "CHOICE 1", "CHOICE 2" }
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.MultiChoice, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.FillInChoice);
                // Currently supports collections only for model types (JsonMappingHelper Ln 120)
                Assert.IsNotNull(addedField.Choices);
                Assert.AreEqual(2, addedField.Choices.Length);
                Assert.AreEqual("CHOICE 1", addedField.Choices[0]);
                Assert.AreEqual("CHOICE 2", addedField.Choices[1]);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldMultiChoiceSpecifiedBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddChoiceMultiBatchAsync(newBatch, "ADDED FIELD", new FieldChoiceMultiOptions()
                {
                    Group = "TEST GROUP",
                    FillInChoice = true,
                    Choices = new string[] { "CHOICE 1", "CHOICE 2" }
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.MultiChoice, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.FillInChoice);
                // Currently supports collections only for model types (JsonMappingHelper Ln 120)
                Assert.IsNotNull(addedField.Choices);
                Assert.AreEqual(2, addedField.Choices.Length);
                Assert.AreEqual("CHOICE 1", addedField.Choices[0]);
                Assert.AreEqual("CHOICE 2", addedField.Choices[1]);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldMultiChoiceSpecifiedBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddChoiceMultiBatch(newBatch, "ADDED FIELD", new FieldChoiceMultiOptions()
                {
                    Group = "TEST GROUP",
                    FillInChoice = true,
                    Choices = new string[] { "CHOICE 1", "CHOICE 2" }
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.MultiChoice, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.FillInChoice);
                // Currently supports collections only for model types (JsonMappingHelper Ln 120)
                Assert.IsNotNull(addedField.Choices);
                Assert.AreEqual(2, addedField.Choices.Length);
                Assert.AreEqual("CHOICE 1", addedField.Choices[0]);
                Assert.AreEqual("CHOICE 2", addedField.Choices[1]);

                await addedField.DeleteAsync();
            }
        }

        #endregion

        #region AddListFieldChoice Tests

        [TestMethod]
        public async Task AddListFieldChoiceAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddChoiceAsync("ADDED FIELD", new FieldChoiceOptions()
                {
                    Group = "TEST GROUP",
                    FillInChoice = true,
                    Choices = new string[] { "CHOICE 1", "CHOICE 2" },
                    EditFormat = ChoiceFormatType.RadioButtons
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Choice, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.FillInChoice);
                Assert.AreEqual(ChoiceFormatType.RadioButtons, addedField.EditFormat);
                // Currently supports collections only for model types (JsonMappingHelper Ln 120)
                Assert.IsNotNull(addedField.Choices);
                Assert.AreEqual(2, addedField.Choices.Length);
                Assert.AreEqual("CHOICE 1", addedField.Choices[0]);
                Assert.AreEqual("CHOICE 2", addedField.Choices[1]);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldChoiceTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddChoice("ADDED FIELD", new FieldChoiceOptions()
                {
                    Group = "TEST GROUP",
                    FillInChoice = true,
                    Choices = new string[] { "CHOICE 1", "CHOICE 2" },
                    EditFormat = ChoiceFormatType.RadioButtons
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Choice, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.FillInChoice);
                Assert.AreEqual(ChoiceFormatType.RadioButtons, addedField.EditFormat);
                // Currently supports collections only for model types (JsonMappingHelper Ln 120)
                Assert.IsNotNull(addedField.Choices);
                Assert.AreEqual(2, addedField.Choices.Length);
                Assert.AreEqual("CHOICE 1", addedField.Choices[0]);
                Assert.AreEqual("CHOICE 2", addedField.Choices[1]);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldChoiceBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddChoiceBatchAsync("ADDED FIELD", new FieldChoiceOptions()
                {
                    Group = "TEST GROUP",
                    FillInChoice = true,
                    Choices = new string[] { "CHOICE 1", "CHOICE 2" },
                    EditFormat = ChoiceFormatType.RadioButtons
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Choice, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.FillInChoice);
                Assert.AreEqual(ChoiceFormatType.RadioButtons, addedField.EditFormat);
                // Currently supports collections only for model types (JsonMappingHelper Ln 120)
                Assert.IsNotNull(addedField.Choices);
                Assert.AreEqual(2, addedField.Choices.Length);
                Assert.AreEqual("CHOICE 1", addedField.Choices[0]);
                Assert.AreEqual("CHOICE 2", addedField.Choices[1]);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldChoiceBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddChoiceBatch("ADDED FIELD", new FieldChoiceOptions()
                {
                    Group = "TEST GROUP",
                    FillInChoice = true,
                    Choices = new string[] { "CHOICE 1", "CHOICE 2" },
                    EditFormat = ChoiceFormatType.RadioButtons
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Choice, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.FillInChoice);
                Assert.AreEqual(ChoiceFormatType.RadioButtons, addedField.EditFormat);
                // Currently supports collections only for model types (JsonMappingHelper Ln 120)
                Assert.IsNotNull(addedField.Choices);
                Assert.AreEqual(2, addedField.Choices.Length);
                Assert.AreEqual("CHOICE 1", addedField.Choices[0]);
                Assert.AreEqual("CHOICE 2", addedField.Choices[1]);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldChoiceSpecificBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddChoiceBatchAsync(newBatch, "ADDED FIELD", new FieldChoiceOptions()
                {
                    Group = "TEST GROUP",
                    FillInChoice = true,
                    Choices = new string[] { "CHOICE 1", "CHOICE 2" },
                    EditFormat = ChoiceFormatType.RadioButtons
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Choice, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.FillInChoice);
                Assert.AreEqual(ChoiceFormatType.RadioButtons, addedField.EditFormat);
                // Currently supports collections only for model types (JsonMappingHelper Ln 120)
                Assert.IsNotNull(addedField.Choices);
                Assert.AreEqual(2, addedField.Choices.Length);
                Assert.AreEqual("CHOICE 1", addedField.Choices[0]);
                Assert.AreEqual("CHOICE 2", addedField.Choices[1]);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldChoiceSpecificBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddChoiceBatch(newBatch, "ADDED FIELD", new FieldChoiceOptions()
                {
                    Group = "TEST GROUP",
                    FillInChoice = true,
                    Choices = new string[] { "CHOICE 1", "CHOICE 2" },
                    EditFormat = ChoiceFormatType.RadioButtons
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Choice, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.FillInChoice);
                Assert.AreEqual(ChoiceFormatType.RadioButtons, addedField.EditFormat);
                // Currently supports collections only for model types (JsonMappingHelper Ln 120)
                Assert.IsNotNull(addedField.Choices);
                Assert.AreEqual(2, addedField.Choices.Length);
                Assert.AreEqual("CHOICE 1", addedField.Choices[0]);
                Assert.AreEqual("CHOICE 2", addedField.Choices[1]);

                await addedField.DeleteAsync();
            }
        }

        #endregion

        #region AddListFieldNumber Tests

        [TestMethod]
        public async Task AddListFieldNumberAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddNumberAsync("ADDED FIELD", new FieldNumberOptions()
                {
                    Group = "TEST GROUP",
                    MaximumValue = 100,
                    MinimumValue = 0,
                    ShowAsPercentage = true
                    // Althought in the docs as usable parameters, DisplayFormat is always -1 in response for number fields
                    //DisplayFormat = 0,
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Number, addedField.FieldTypeKind);
                Assert.AreEqual(0, addedField.MinimumValue);
                Assert.IsTrue(addedField.ShowAsPercentage);
                // TODO Solve issue for double type deserializing
                Assert.AreEqual(100, addedField.MaximumValue);
                // Althought in the docs as usable parameters, DisplayFormat is always -1 in response for number fields
                Assert.AreEqual(-1, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldNumberTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddNumber("ADDED FIELD", new FieldNumberOptions()
                {
                    Group = "TEST GROUP",
                    MaximumValue = 100,
                    MinimumValue = 0,
                    ShowAsPercentage = true
                    // Althought in the docs as usable parameters, DisplayFormat is always -1 in response for number fields
                    //DisplayFormat = 0,
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Number, addedField.FieldTypeKind);
                Assert.AreEqual(0, addedField.MinimumValue);
                Assert.IsTrue(addedField.ShowAsPercentage);
                Assert.AreEqual(100, addedField.MaximumValue);
                Assert.AreEqual(-1, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldNumberBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddNumberBatchAsync("ADDED FIELD", new FieldNumberOptions()
                {
                    Group = "TEST GROUP",
                    MaximumValue = 100,
                    MinimumValue = 0,
                    ShowAsPercentage = true
                    // Althought in the docs as usable parameters, DisplayFormat is always -1 in response for number fields
                    //DisplayFormat = 0,
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Number, addedField.FieldTypeKind);
                Assert.AreEqual(0, addedField.MinimumValue);
                Assert.IsTrue(addedField.ShowAsPercentage);
                Assert.AreEqual(100, addedField.MaximumValue);
                Assert.AreEqual(-1, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldNumberBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddNumberBatch("ADDED FIELD", new FieldNumberOptions()
                {
                    Group = "TEST GROUP",
                    MaximumValue = 100,
                    MinimumValue = 0,
                    ShowAsPercentage = true
                    // Althought in the docs as usable parameters, DisplayFormat is always -1 in response for number fields
                    //DisplayFormat = 0,
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Number, addedField.FieldTypeKind);
                Assert.AreEqual(0, addedField.MinimumValue);
                Assert.IsTrue(addedField.ShowAsPercentage);
                Assert.AreEqual(100, addedField.MaximumValue);
                Assert.AreEqual(-1, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldNumberSpecificBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddNumberBatchAsync(newBatch, "ADDED FIELD", new FieldNumberOptions()
                {
                    Group = "TEST GROUP",
                    MaximumValue = 100,
                    MinimumValue = 0,
                    ShowAsPercentage = true
                    // Althought in the docs as usable parameters, DisplayFormat is always -1 in response for number fields
                    //DisplayFormat = 0,
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Number, addedField.FieldTypeKind);
                Assert.AreEqual(0, addedField.MinimumValue);
                Assert.IsTrue(addedField.ShowAsPercentage);
                Assert.AreEqual(100, addedField.MaximumValue);
                Assert.AreEqual(-1, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldNumberSpecificBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddNumberBatch(newBatch, "ADDED FIELD", new FieldNumberOptions()
                {
                    Group = "TEST GROUP",
                    MaximumValue = 100,
                    MinimumValue = 0,
                    ShowAsPercentage = true
                    // Althought in the docs as usable parameters, DisplayFormat is always -1 in response for number fields
                    //DisplayFormat = 0,
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Number, addedField.FieldTypeKind);
                Assert.AreEqual(0, addedField.MinimumValue);
                Assert.IsTrue(addedField.ShowAsPercentage);
                Assert.AreEqual(100, addedField.MaximumValue);
                Assert.AreEqual(-1, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        #endregion

        #region AddListFieldCurrency Tests

        [TestMethod]
        public async Task AddListFieldCurrencyAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddCurrencyAsync("ADDED FIELD", new FieldCurrencyOptions()
                {
                    Group = "TEST GROUP",
                    CurrencyLocaleId = 1033
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Currency, addedField.FieldTypeKind);
                Assert.AreEqual(1033, addedField.CurrencyLocaleId);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldCurrencyTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddCurrency("ADDED FIELD", new FieldCurrencyOptions()
                {
                    Group = "TEST GROUP",
                    CurrencyLocaleId = 1033
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Currency, addedField.FieldTypeKind);
                Assert.AreEqual(1033, addedField.CurrencyLocaleId);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldCurrencyBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddCurrencyBatchAsync("ADDED FIELD", new FieldCurrencyOptions()
                {
                    Group = "TEST GROUP",
                    CurrencyLocaleId = 1033
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Currency, addedField.FieldTypeKind);
                Assert.AreEqual(1033, addedField.CurrencyLocaleId);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldCurrencyBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddCurrencyBatch("ADDED FIELD", new FieldCurrencyOptions()
                {
                    Group = "TEST GROUP",
                    CurrencyLocaleId = 1033
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Currency, addedField.FieldTypeKind);
                Assert.AreEqual(1033, addedField.CurrencyLocaleId);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldCurrencySpecifiedBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddCurrencyBatchAsync(newBatch, "ADDED FIELD", new FieldCurrencyOptions()
                {
                    Group = "TEST GROUP",
                    CurrencyLocaleId = 1033
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Currency, addedField.FieldTypeKind);
                Assert.AreEqual(1033, addedField.CurrencyLocaleId);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldCurrencySpecifiedBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddCurrencyBatch(newBatch, "ADDED FIELD", new FieldCurrencyOptions()
                {
                    Group = "TEST GROUP",
                    CurrencyLocaleId = 1033
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Currency, addedField.FieldTypeKind);
                Assert.AreEqual(1033, addedField.CurrencyLocaleId);

                await addedField.DeleteAsync();
            }
        }

        #endregion

        #region AddListFieldUrl Tests

        [TestMethod]
        public async Task AddListFieldUrlAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddUrlAsync("ADDED FIELD", new FieldUrlOptions()
                {
                    Group = "TEST GROUP",
                    DisplayFormat = UrlFieldFormatType.Image
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.URL, addedField.FieldTypeKind);
                Assert.AreEqual((int)UrlFieldFormatType.Image, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldUrlTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddUrl("ADDED FIELD", new FieldUrlOptions()
                {
                    Group = "TEST GROUP",
                    DisplayFormat = UrlFieldFormatType.Image
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.URL, addedField.FieldTypeKind);
                Assert.AreEqual((int)UrlFieldFormatType.Image, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldUrlBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddUrlBatchAsync("ADDED FIELD", new FieldUrlOptions()
                {
                    Group = "TEST GROUP",
                    DisplayFormat = UrlFieldFormatType.Image
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.URL, addedField.FieldTypeKind);
                Assert.AreEqual((int)UrlFieldFormatType.Image, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }


        [TestMethod]
        public async Task AddListFieldUrlBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddUrlBatch("ADDED FIELD", new FieldUrlOptions()
                {
                    Group = "TEST GROUP",
                    DisplayFormat = UrlFieldFormatType.Image
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.URL, addedField.FieldTypeKind);
                Assert.AreEqual((int)UrlFieldFormatType.Image, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }


        [TestMethod]
        public async Task AddListFieldUrlSpecifiedBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddUrlBatchAsync(newBatch, "ADDED FIELD", new FieldUrlOptions()
                {
                    Group = "TEST GROUP",
                    DisplayFormat = UrlFieldFormatType.Image
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.URL, addedField.FieldTypeKind);
                Assert.AreEqual((int)UrlFieldFormatType.Image, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }


        [TestMethod]
        public async Task AddListFieldUrlSpecifiedBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddUrlBatch(newBatch, "ADDED FIELD", new FieldUrlOptions()
                {
                    Group = "TEST GROUP",
                    DisplayFormat = UrlFieldFormatType.Image
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.URL, addedField.FieldTypeKind);
                Assert.AreEqual((int)UrlFieldFormatType.Image, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }


        #endregion

        #region AddListFieldCalculated

        [TestMethod]
        public async Task AddListFieldCalculatedAsDateTimeSpecificTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddCalculatedAsync("ADDED FIELD", new FieldCalculatedOptions()
                {
                    Group = "TEST GROUP",
                    // Don't test locale as it's dependent on the locale settings of the test site
                    //CurrencyLocaleId = 1033,
                    DateFormat = DateTimeFieldFormatType.DateTime,
                    Formula = @"=TODAY()",
                    OutputType = FieldType.DateTime
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Calculated, addedField.FieldTypeKind);
                // Don't test locale as it's dependent on the locale settings of the test site
                //Assert.AreEqual(1033, addedField.CurrencyLocaleId);
                Assert.AreEqual(DateTimeFieldFormatType.DateTime, addedField.DateFormat);
                Assert.AreEqual(@"=TODAY()", addedField.Formula);
                Assert.AreEqual(FieldType.DateTime, addedField.OutputType);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldCalculatedSpecificAsNumberTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddCalculatedAsync("ADDED FIELD", new FieldCalculatedOptions()
                {
                    Group = "TEST GROUP",
                    // Don't use decimals in the formula to remove the test dependency on the site's locale
                    Formula = @"=3-2",
                    OutputType = FieldType.Number,
                    ShowAsPercentage = true,
                    // Althought in the docs as usable parameters, DisplayFormat is always -1 in response for number fields
                    //DisplayFormat = 0,
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Calculated, addedField.FieldTypeKind);
                // Don't use decimals in the formula to remove the test dependency on the site's locale
                Assert.AreEqual(@"=3-2", addedField.Formula);
                Assert.AreEqual(FieldType.Number, addedField.OutputType);
                Assert.IsTrue(addedField.ShowAsPercentage);
                // Althought in the docs as usable parameters, DisplayFormat is always -1 in response for number fields
                Assert.AreEqual(-1, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldCalculatedSpecificAsTextAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddCalculatedAsync("ADDED FIELD", new FieldCalculatedOptions()
                {
                    Group = "TEST GROUP",
                    Formula = @"=""HELLO""",
                    OutputType = FieldType.Text
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Calculated, addedField.FieldTypeKind);
                Assert.AreEqual(@"=""HELLO""", addedField.Formula);
                Assert.AreEqual(FieldType.Text, addedField.OutputType);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldCalculatedAsTextTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddCalculated("ADDED FIELD", new FieldCalculatedOptions()
                {
                    Group = "TEST GROUP",
                    Formula = @"=""HELLO""",
                    OutputType = FieldType.Text
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Calculated, addedField.FieldTypeKind);
                Assert.AreEqual(@"=""HELLO""", addedField.Formula);
                Assert.AreEqual(FieldType.Text, addedField.OutputType);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldCalculatedSpecificAsTextBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddCalculatedBatchAsync("ADDED FIELD", new FieldCalculatedOptions()
                {
                    Group = "TEST GROUP",
                    Formula = @"=""HELLO""",
                    OutputType = FieldType.Text
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Calculated, addedField.FieldTypeKind);
                Assert.AreEqual(@"=""HELLO""", addedField.Formula);
                Assert.AreEqual(FieldType.Text, addedField.OutputType);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldCalculatedSpecificAsTextBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddCalculatedBatch("ADDED FIELD", new FieldCalculatedOptions()
                {
                    Group = "TEST GROUP",
                    Formula = @"=""HELLO""",
                    OutputType = FieldType.Text
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Calculated, addedField.FieldTypeKind);
                Assert.AreEqual(@"=""HELLO""", addedField.Formula);
                Assert.AreEqual(FieldType.Text, addedField.OutputType);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldCalculatedSpecificAsTextSpecifiedBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddCalculatedBatchAsync(newBatch, "ADDED FIELD", new FieldCalculatedOptions()
                {
                    Group = "TEST GROUP",
                    Formula = @"=""HELLO""",
                    OutputType = FieldType.Text
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Calculated, addedField.FieldTypeKind);
                Assert.AreEqual(@"=""HELLO""", addedField.Formula);
                Assert.AreEqual(FieldType.Text, addedField.OutputType);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldCalculatedSpecificAsTextSpecifiedBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddCalculatedBatch(newBatch, "ADDED FIELD", new FieldCalculatedOptions()
                {
                    Group = "TEST GROUP",
                    Formula = @"=""HELLO""",
                    OutputType = FieldType.Text
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Calculated, addedField.FieldTypeKind);
                Assert.AreEqual(@"=""HELLO""", addedField.Formula);
                Assert.AreEqual(FieldType.Text, addedField.OutputType);

                await addedField.DeleteAsync();
            }
        }

        #endregion

        #region AddListFieldLookup Tests

        [TestMethod]
        public async Task AddListFieldLookupAsyncTest()
        {
            // CAUTION : Add Lookup field DOES NOT SUPPORT specifying some options at creation (e.g. Group, Hidden, ...)
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;
                IWeb currentWeb = await context.Web.GetAsync(w => w.Id, w => w.Lists);
                IList sitePages = currentWeb.Lists.AsRequested().FirstOrDefault(p => p.Title == "Site Pages");
                IList documents = currentWeb.Lists.AsRequested().FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddLookupAsync("ADDED FIELD", new FieldLookupOptions()
                {
                    Required = true,
                    LookupFieldName = "Title",
                    LookupListId = sitePages.Id,
                    //LookupWebId = currentWeb.Id
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.Lookup, addedField.FieldTypeKind);
                Assert.AreEqual("Title", addedField.LookupField);
                Assert.AreEqual(sitePages.Id, Guid.Parse(addedField.LookupList));
                Assert.AreEqual(currentWeb.Id, addedField.LookupWebId);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldLookupTest()
        {
            // CAUTION : Add Lookup field DOES NOT SUPPORT specifying some options at creation (e.g. Group, Hidden, ...)
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;
                IWeb currentWeb = await context.Web.GetAsync(w => w.Id, w => w.Lists);
                IList sitePages = currentWeb.Lists.AsRequested().FirstOrDefault(p => p.Title == "Site Pages");
                IList documents = currentWeb.Lists.AsRequested().FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddLookup("ADDED FIELD", new FieldLookupOptions()
                {
                    Required = true,
                    LookupFieldName = "Title",
                    LookupListId = sitePages.Id,
                    //LookupWebId = currentWeb.Id
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.Lookup, addedField.FieldTypeKind);
                Assert.AreEqual("Title", addedField.LookupField);
                Assert.AreEqual(sitePages.Id, Guid.Parse(addedField.LookupList));
                Assert.AreEqual(currentWeb.Id, addedField.LookupWebId);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldLookupBatchAsyncTest()
        {
            // CAUTION : Add Lookup field DOES NOT SUPPORT specifying some options at creation (e.g. Group, Hidden, ...)
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;
                IWeb currentWeb = await context.Web.GetAsync(w => w.Id, w => w.Lists);
                IList sitePages = currentWeb.Lists.AsRequested().FirstOrDefault(p => p.Title == "Site Pages");
                IList documents = currentWeb.Lists.AsRequested().FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddLookupBatchAsync("ADDED FIELD", new FieldLookupOptions()
                {
                    Required = true,
                    LookupFieldName = "Title",
                    LookupListId = sitePages.Id,
                    //LookupWebId = currentWeb.Id
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.Lookup, addedField.FieldTypeKind);
                Assert.AreEqual("Title", addedField.LookupField);
                Assert.AreEqual(sitePages.Id, Guid.Parse(addedField.LookupList));
                Assert.AreEqual(currentWeb.Id, addedField.LookupWebId);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldLookupBatchTest()
        {
            // CAUTION : Add Lookup field DOES NOT SUPPORT specifying some options at creation (e.g. Group, Hidden, ...)
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;
                IWeb currentWeb = await context.Web.GetAsync(w => w.Id, w => w.Lists);
                IList sitePages = currentWeb.Lists.AsRequested().FirstOrDefault(p => p.Title == "Site Pages");
                IList documents = currentWeb.Lists.AsRequested().FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddLookupBatch("ADDED FIELD", new FieldLookupOptions()
                {
                    Required = true,
                    LookupFieldName = "Title",
                    LookupListId = sitePages.Id,
                    //LookupWebId = currentWeb.Id
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.Lookup, addedField.FieldTypeKind);
                Assert.AreEqual("Title", addedField.LookupField);
                Assert.AreEqual(sitePages.Id, Guid.Parse(addedField.LookupList));
                Assert.AreEqual(currentWeb.Id, addedField.LookupWebId);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldLookupSpecificBatchAsyncTest()
        {
            // CAUTION : Add Lookup field DOES NOT SUPPORT specifying some options at creation (e.g. Group, Hidden, ...)
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;
                var newBatch = context.NewBatch();
                IWeb currentWeb = await context.Web.GetAsync(w => w.Id, w => w.Lists);
                IList sitePages = currentWeb.Lists.AsRequested().FirstOrDefault(p => p.Title == "Site Pages");
                IList documents = currentWeb.Lists.AsRequested().FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddLookupBatchAsync(newBatch, "ADDED FIELD", new FieldLookupOptions()
                {
                    Required = true,
                    LookupFieldName = "Title",
                    LookupListId = sitePages.Id,
                    //LookupWebId = currentWeb.Id
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.Lookup, addedField.FieldTypeKind);
                Assert.AreEqual("Title", addedField.LookupField);
                Assert.AreEqual(sitePages.Id, Guid.Parse(addedField.LookupList));
                Assert.AreEqual(currentWeb.Id, addedField.LookupWebId);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldLookupSpecificBatchTest()
        {
            // CAUTION : Add Lookup field DOES NOT SUPPORT specifying some options at creation (e.g. Group, Hidden, ...)
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;
                var newBatch = context.NewBatch();
                IWeb currentWeb = await context.Web.GetAsync(w => w.Id, w => w.Lists);
                IList sitePages = currentWeb.Lists.AsRequested().FirstOrDefault(p => p.Title == "Site Pages");
                IList documents = currentWeb.Lists.AsRequested().FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddLookupBatch(newBatch, "ADDED FIELD", new FieldLookupOptions()
                {
                    Required = true,
                    LookupFieldName = "Title",
                    LookupListId = sitePages.Id,
                    //LookupWebId = currentWeb.Id
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.Lookup, addedField.FieldTypeKind);
                Assert.AreEqual("Title", addedField.LookupField);
                Assert.AreEqual(sitePages.Id, Guid.Parse(addedField.LookupList));
                Assert.AreEqual(currentWeb.Id, addedField.LookupWebId);

                await addedField.DeleteAsync();
            }
        }

        #endregion

        #region AddListFieldUser Tests

        [TestMethod]
        public async Task AddListFieldUserAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddUserAsync("ADDED FIELD", new FieldUserOptions()
                {
                    Group = "TEST GROUP",
                    Required = true,
                    AllowDisplay = true,
                    Presence = true,
                    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
                    // TODO Must be tested when support for SharePoint groups is implemented
                    //SelectionGroup = 1
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.User, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.Required);
                Assert.IsTrue(addedField.AllowDisplay);
                Assert.IsFalse(addedField.AllowMultipleValues);
                Assert.AreEqual(addedField.SelectionMode, FieldUserSelectionMode.PeopleAndGroups);
                // TODO Must be tested when support for SharePoint groups is implemented
                //Assert.AreEqual(addedField.SelectionGroup, 1);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldUserTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddUser("ADDED FIELD", new FieldUserOptions()
                {
                    Group = "TEST GROUP",
                    Required = true,
                    AllowDisplay = true,
                    Presence = true,
                    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.User, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.Required);
                Assert.IsTrue(addedField.AllowDisplay);
                Assert.IsFalse(addedField.AllowMultipleValues);
                Assert.AreEqual(addedField.SelectionMode, FieldUserSelectionMode.PeopleAndGroups);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldUserBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddUserBatchAsync("ADDED FIELD", new FieldUserOptions()
                {
                    Group = "TEST GROUP",
                    Required = true,
                    AllowDisplay = true,
                    Presence = true,
                    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
                });
                await context.ExecuteAsync();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.User, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.Required);
                Assert.IsTrue(addedField.AllowDisplay);
                Assert.IsFalse(addedField.AllowMultipleValues);
                Assert.AreEqual(addedField.SelectionMode, FieldUserSelectionMode.PeopleAndGroups);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldUserBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddUserBatch("ADDED FIELD", new FieldUserOptions()
                {
                    Group = "TEST GROUP",
                    Required = true,
                    AllowDisplay = true,
                    Presence = true,
                    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
                });
                context.Execute();

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.User, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.Required);
                Assert.IsTrue(addedField.AllowDisplay);
                Assert.IsFalse(addedField.AllowMultipleValues);
                Assert.AreEqual(addedField.SelectionMode, FieldUserSelectionMode.PeopleAndGroups);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldUserSpecifiedBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddUserBatchAsync(newBatch, "ADDED FIELD", new FieldUserOptions()
                {
                    Group = "TEST GROUP",
                    Required = true,
                    AllowDisplay = true,
                    Presence = true,
                    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
                });
                await context.ExecuteAsync(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.User, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.Required);
                Assert.IsTrue(addedField.AllowDisplay);
                Assert.IsFalse(addedField.AllowMultipleValues);
                Assert.AreEqual(addedField.SelectionMode, FieldUserSelectionMode.PeopleAndGroups);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldUserSpecifiedBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var newBatch = context.NewBatch();
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = documents.Fields.AddUserBatch(newBatch, "ADDED FIELD", new FieldUserOptions()
                {
                    Group = "TEST GROUP",
                    Required = true,
                    AllowDisplay = true,
                    Presence = true,
                    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
                });
                context.Execute(newBatch);

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual(FieldType.User, addedField.FieldTypeKind);
                Assert.IsTrue(addedField.Required);
                Assert.IsTrue(addedField.AllowDisplay);
                Assert.IsFalse(addedField.AllowMultipleValues);
                Assert.AreEqual(addedField.SelectionMode, FieldUserSelectionMode.PeopleAndGroups);

                await addedField.DeleteAsync();
            }
        }
        #endregion

        [TestMethod]
        public async Task UpdateListFieldTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField field = await documents.Fields.AddTextAsync("TO UPDATE FIELD", new FieldTextOptions());

                // Test if the content type is created
                Assert.IsNotNull(field);

                field.Title = "UPDATED";
                await field.UpdateAsync();

                // Test if the content type is still found
                IField fieldToFind = (from ct in documents.Fields
                                      where ct.Title == "UPDATED"
                                      select ct).FirstOrDefault();

                Assert.IsNotNull(fieldToFind);

                await fieldToFind.DeleteAsync();

            }
        }

        [TestMethod]
        public async Task DeleteListFieldTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField field = await documents.Fields.AddTextAsync("TO DELETE FIELD");

                // Test if the content type is created
                Assert.IsNotNull(field);

                await field.DeleteAsync();

                // Test if the content type is still found
                IField fieldToFind = (from ct in documents.Fields
                                      where ct.Title == "TO DELETE FIELD"
                                      select ct).FirstOrDefault();

                Assert.IsNull(fieldToFind);
            }
        }


        [TestMethod]
        public async Task ListFieldVisibilityInFormTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Add a new library
                string listTitle = TestCommon.GetPnPSdkTestAssetName("AddListFieldNotVisibleInDisplayFormTest");
                IList myList = null;
                try
                {
                    myList = context.Web.Lists.GetByTitle(listTitle);

                    if (TestCommon.Instance.Mocking && myList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (myList == null)
                    {
                        myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                    }

                    IField displayField = await myList.Fields.AddTextAsync("DISPLAYFORMFIELD", new FieldTextOptions());

                    // Hide the field from the display form
                    displayField.SetShowInDisplayForm(false);

                    // Load the field again
                    IField fieldToFind = (from f in myList.Fields
                                          where f.Title == "DISPLAYFORMFIELD"
                                          select f).FirstOrDefault();
                    Assert.IsTrue(fieldToFind != null);
                    Assert.IsTrue(fieldToFind.SchemaXml.Contains("ShowInDisplayForm=\"FALSE\""));
                    
                    IField editField = await myList.Fields.AddTextAsync("EDITFORMFIELD", new FieldTextOptions());

                    // Hide the field from the display form
                    editField.SetShowInEditForm(false);

                    // Load the field again
                    IField editFieldToFind = (from f in myList.Fields
                                          where f.Title == "EDITFORMFIELD"
                                              select f).FirstOrDefault();
                    Assert.IsTrue(editFieldToFind != null);
                    Assert.IsTrue(editFieldToFind.SchemaXml.Contains("ShowInEditForm=\"FALSE\""));

                    IField newField = await myList.Fields.AddTextAsync("NEWFORMFIELD", new FieldTextOptions());

                    // Hide the field from the display form
                    newField.SetShowInNewForm(false);

                    // Load the field again
                    IField newFieldToFind = (from f in myList.Fields
                                              where f.Title == "NEWFORMFIELD"
                                              select f).FirstOrDefault();
                    Assert.IsTrue(newFieldToFind != null);
                    Assert.IsTrue(newFieldToFind.SchemaXml.Contains("ShowInNewForm=\"FALSE\""));
                }
                finally
                {
                    myList.Delete();
                }
            }
        }
    }
}
