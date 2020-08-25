using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
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
                Assert.IsTrue(documents.Fields.Count() > 0);

                IField field = documents.Fields.FirstOrDefault(p => p.InternalName == "Title");
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
                IField addedField = await documents.Fields.AddAsync("ADDED FIELD", FieldType.Text, new FieldTextOptions());

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
                IField addedField = await documents.Fields.AddAsync("ADDED FIELD", FieldType.Text, new FieldTextOptions()
                {
                    Description = "TEST DESCRIPTION",
                    Group = "TEST GROUP",
                    DefaultFormula = @"=""DEFAULT""",
                    Indexed = true,
                    EnforceUniqueValues = true,
                    Required = true,
                    // TODO Check validation formula format
                    //ValidationFormula = 
                    ValidationMessage = "Invalid Value",
                    // TODO Check why hidden list fields cannot be deleted the regular way
                    //{"error":{"code":"-2130575214, Microsoft.SharePoint.SPException","message":{"lang":"en-US","value":"You cannot delete a hidden column."}}}
                    //Hidden = true,
                });

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
                IField addedField = await documents.Fields.AddFieldAsXmlAsync(@"<Field Type=""Text"" Name=""ADDEDFIELD"" DisplayName=""ADDED FIELD""/>");

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
        public async Task AddNewWebGenericFieldTextTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddAsync("ADDED FIELD", FieldType.Text, new FieldTextOptions()
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
        [TestMethod]
        public async Task AddNewListFieldTextSpecificTest()
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
        public async Task AddListFieldMultilineTextSpecificTest()
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
        public async Task AddListFieldDateTimeSpecificTest()
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
        public async Task AddListFieldMultiChoiceSpecificTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddMultiChoiceAsync("ADDED FIELD", new FieldMultiChoiceOptions()
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
        public async Task AddListFieldChoiceSpecificTest()
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
                Assert.AreEqual((int)ChoiceFormatType.RadioButtons, addedField.EditFormat);
                // Currently supports collections only for model types (JsonMappingHelper Ln 120)
                Assert.IsNotNull(addedField.Choices);
                Assert.AreEqual(2, addedField.Choices.Length);
                Assert.AreEqual("CHOICE 1", addedField.Choices[0]);
                Assert.AreEqual("CHOICE 2", addedField.Choices[1]);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldNumberSpecificTest()
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
        public async Task AddListFieldCurrencySpecificTest()
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
        public async Task AddListFieldUrlSpecificTest()
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
        public async Task AddListFieldCalculatedAsDateTimeSpecificTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddCalculatedAsync("ADDED FIELD", new FieldCalculatedOptions()
                {
                    Group = "TEST GROUP",
                    CurrencyLocaleId = 1033,
                    DateFormat = DateTimeFieldFormatType.DateTime,
                    Formula = @"=TODAY()",
                    OutputType = FieldType.DateTime
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Calculated, addedField.FieldTypeKind);
                Assert.AreEqual(1033, addedField.CurrencyLocaleId);
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
                    Formula = @"=1-0.5",
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
                Assert.AreEqual(@"=1-0.5", addedField.Formula);
                Assert.AreEqual(FieldType.Number, addedField.OutputType);
                Assert.IsTrue(addedField.ShowAsPercentage);
                // Althought in the docs as usable parameters, DisplayFormat is always -1 in response for number fields
                Assert.AreEqual(-1, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListFieldCalculatedSpecificAsTextTest()
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
        public async Task AddListFieldLookupSpecificTest()
        {
            // CAUTION : Add Lookup field DOES NOT SUPPORT specifying some options at creation (e.g. Group, Hidden, ...)
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;
                IWeb currentWeb = await context.Web.GetAsync(w => w.Id, w => w.Lists);
                IList sitePages = context.Web.Lists.FirstOrDefault(p => p.Title == "Site Pages");
                IList documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField addedField = await documents.Fields.AddLookupAsync("ADDED FIELD", new FieldLookupOptions()
                {
                    Required = true,
                    LookupFieldName = "Title",
                    LookupListId = sitePages.Id,
                    LookupWebId = currentWeb.Id
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
        public async Task AddListFieldUserSpecificTest()
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
                    AllowMultipleValues = true,
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
                Assert.IsTrue(addedField.AllowMultipleValues);
                Assert.AreEqual(addedField.SelectionMode, FieldUserSelectionMode.PeopleAndGroups);
                // TODO Must be tested when support for SharePoint groups is implemented
                //Assert.AreEqual(addedField.SelectionGroup, 1);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdateListFieldTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var documents = context.Web.Lists.FirstOrDefault(p => p.Title == "Documents");
                IField field = await documents.Fields.AddAsync("TO UPDATE FIELD", FieldType.Text, new FieldTextOptions());

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
                IField field = await documents.Fields.AddAsync("TO DELETE FIELD", FieldType.Text);

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
    }
}
