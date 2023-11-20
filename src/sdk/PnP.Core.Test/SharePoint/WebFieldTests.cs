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
    public class WebFieldTests
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
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IWeb web = await context.Web.GetAsync(p => p.Fields);
                Assert.IsTrue(web.Fields.Length > 0);

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
        public async Task GetWebFieldByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
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
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddTextAsync("ADDED FIELD1", new FieldTextOptions());

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD1", addedField.Title);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebFieldWithOptionsDefaultFormulaTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddTextAsync("ADDED FIELD2", new FieldTextOptions()
                {
                    Description = "TEST DESCRIPTION",
                    Group = "TEST GROUP",
                    DefaultFormula = @"=""DEFAULT""",
                    Hidden = true,
                    Indexed = true,
                    EnforceUniqueValues = true,
                    Required = true,
                    ValidationFormula = @"=ISNUMBER(5)",
                    ValidationMessage = "Invalid Value"
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD2", addedField.Title);
                Assert.AreEqual("TEST DESCRIPTION", addedField.Description);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(@"=""DEFAULT""", addedField.DefaultFormula);
                Assert.IsTrue(addedField.Hidden);
                Assert.IsTrue(addedField.Indexed);
                Assert.IsTrue(addedField.EnforceUniqueValues);
                Assert.IsTrue(addedField.Required);
                Assert.AreEqual("Invalid Value", addedField.ValidationMessage);
                Assert.AreEqual(@"=ISNUMBER(5)", addedField.ValidationFormula);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebFieldAsXmlTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddFieldAsXmlAsync(@"<Field Type=""Text"" Name=""ADDED FIELD3"" DisplayName=""ADDED FIELD3""/>");

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD3", addedField.Title);
                Assert.AreEqual("ADDED FIELD3", addedField.InternalName);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);

                await addedField.DeleteAsync();
            }
        }


        [TestMethod]
        public async Task AddNewWebGenericFieldTextTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddTextAsync("ADDED FIELD4", new FieldTextOptions()
                {
                    Group = "TEST GROUP",
                    MaxLength = 100
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD4", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                Assert.AreEqual(100, addedField.MaxLength);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddNewWebFieldTextSpecificTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddTextAsync("ADDED FIELD5", new FieldTextOptions()
                {
                    Group = "TEST GROUP",
                    MaxLength = 100
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD5", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                Assert.AreEqual(100, addedField.MaxLength);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddNewWebFieldTextSpecificInternalNameTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddTextAsync("ADDED FIELD INTERNAL 5", new FieldTextOptions()
                {
                    InternalName = "AddFldInternal5",
                    Group = "TEST GROUP",
                    MaxLength = 100
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD INTERNAL 5", addedField.Title);
                Assert.AreEqual("AddFldInternal5", addedField.InternalName);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Text, addedField.FieldTypeKind);
                Assert.AreEqual(100, addedField.MaxLength);

                await addedField.DeleteAsync();
            }
        }


        [TestMethod]
        public async Task AddFieldWithFieldCustomizerTest()
        {
            //TestCommon.Instance.Mocking = false;
            string fieldName = TestCommon.GetPnPSdkTestAssetName("FieldWithCustomizer");
            string groupName = TestCommon.GetPnPSdkTestAssetName("FieldGroup");
            Guid clientSideComponentId = new Guid(TestAssets.TestFieldCustomizerClientSideComponentId);
            string clientSideComponentProperties = $@"{{""message"":""Added from AddFieldWithFieldCustomizerTest"" }}";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 0))
            {
                IField field = await context.Web.Fields.AddTextAsync(fieldName, new FieldTextOptions()
                {
                    Group = groupName,
                    MaxLength = 100
                });

                // Test the created object
                Assert.IsNotNull(field);
                Assert.AreEqual(fieldName, field.Title);
                Assert.AreEqual(groupName, field.Group);

                field.ClientSideComponentId = clientSideComponentId;
                field.ClientSideComponentProperties = clientSideComponentProperties;
                await field.UpdateAsync();
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                IField field = (from f in context.Web.Fields
                                where f.Title == fieldName
                                select f).FirstOrDefault();

                Assert.IsNotNull(field);
                Assert.AreEqual(clientSideComponentId, field.ClientSideComponentId);
                Assert.AreEqual(clientSideComponentProperties, field.ClientSideComponentProperties);

                await field.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebFieldMultilineTextSpecificTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddMultilineTextAsync("ADDED FIELD6", new FieldMultilineTextOptions()
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
                Assert.AreEqual("ADDED FIELD6", addedField.Title);
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
        public async Task AddWebFieldDateTimeSpecificTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddDateTimeAsync("ADDED FIELD7", new FieldDateTimeOptions()
                {
                    Group = "TEST GROUP",
                    DateTimeCalendarType = CalendarType.GregorianXLITFrench,
                    DisplayFormat = DateTimeFieldFormatType.DateTime,
                    FriendlyDisplayFormat = DateTimeFieldFriendlyFormatType.Relative
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD7", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.DateTime, addedField.FieldTypeKind);
                Assert.AreEqual(CalendarType.GregorianXLITFrench, addedField.DateTimeCalendarType);
                Assert.AreEqual((int)DateTimeFieldFormatType.DateTime, addedField.DisplayFormat);
                Assert.AreEqual(DateTimeFieldFriendlyFormatType.Relative, addedField.FriendlyDisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebFieldMultiChoiceSpecificTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddChoiceMultiAsync("ADDED FIELD8", new FieldChoiceMultiOptions()
                {
                    Group = "TEST GROUP",
                    FillInChoice = true,
                    Choices = new string[] { "CHOICE 1", "CHOICE 2" }
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD8", addedField.Title);
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
        public async Task AddWebFieldChoiceSpecificTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddChoiceAsync("ADDED FIELD9", new FieldChoiceOptions()
                {
                    Group = "TEST GROUP",
                    FillInChoice = true,
                    Choices = new string[] { "CHOICE 1", "CHOICE 2" },
                    EditFormat = ChoiceFormatType.RadioButtons
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD9", addedField.Title);
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
        public async Task AddWebFieldNumberSpecificTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddNumberAsync("ADDED FIELD10", new FieldNumberOptions()
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
                Assert.AreEqual("ADDED FIELD10", addedField.Title);
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
        public async Task AddWebFieldCurrencySpecificTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddCurrencyAsync("ADDED FIELD11", new FieldCurrencyOptions()
                {
                    Group = "TEST GROUP",
                    CurrencyLocaleId = 1033
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD11", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Currency, addedField.FieldTypeKind);
                Assert.AreEqual(1033, addedField.CurrencyLocaleId);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebFieldUrlSpecificTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddUrlAsync("ADDED FIELD12", new FieldUrlOptions()
                {
                    Group = "TEST GROUP",
                    DisplayFormat = UrlFieldFormatType.Image
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD12", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.URL, addedField.FieldTypeKind);
                Assert.AreEqual((int)UrlFieldFormatType.Image, addedField.DisplayFormat);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebFieldCalculatedAsDateTimeSpecificTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddCalculatedAsync("ADDED FIELD13", new FieldCalculatedOptions()
                {
                    Group = "TEST GROUP",
                    // The test site does not always have 1033 as locale, hence this might fail
                    //CurrencyLocaleId = 1033,
                    DateFormat = DateTimeFieldFormatType.DateTime,
                    Formula = @"=TODAY()",
                    OutputType = FieldType.DateTime
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD13", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Calculated, addedField.FieldTypeKind);
                // The test site does not always have 1033 as locale, hence this might fail
                //Assert.AreEqual(1033, addedField.CurrencyLocaleId);
                Assert.AreEqual(DateTimeFieldFormatType.DateTime, addedField.DateFormat);
                Assert.AreEqual(@"=TODAY()", addedField.Formula);
                Assert.AreEqual(FieldType.DateTime, addedField.OutputType);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebFieldCalculatedSpecificAsNumberTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddCalculatedAsync("ADDED FIELD14", new FieldCalculatedOptions()
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
                Assert.AreEqual("ADDED FIELD14", addedField.Title);
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
        public async Task AddWebFieldCalculatedSpecificAsTextTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddCalculatedAsync("ADDED FIELD15", new FieldCalculatedOptions()
                {
                    Group = "TEST GROUP",
                    Formula = @"=""HELLO""",
                    OutputType = FieldType.Text
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD15", addedField.Title);
                Assert.AreEqual("TEST GROUP", addedField.Group);
                Assert.AreEqual(FieldType.Calculated, addedField.FieldTypeKind);
                Assert.AreEqual(@"=""HELLO""", addedField.Formula);
                Assert.AreEqual(FieldType.Text, addedField.OutputType);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebFieldLookupSpecificTest()
        {
            // CAUTION : Add Lookup field DOES NOT SUPPORT specifying some options at creation (e.g. Group, Hidden, ...)
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IWeb currentWeb = await context.Web.GetAsync();
                IList documents = currentWeb.Lists.GetByTitle("Documents", l => l.Id);
                IField addedField = await currentWeb.Fields.AddLookupAsync("ADDED FIELD16", new FieldLookupOptions()
                {
                    Required = true,
                    LookupFieldName = "Title",
                    LookupListId = documents.Id,
                    LookupWebId = currentWeb.Id
                });

                // Test the created object
                Assert.IsNotNull(addedField);
                Assert.AreEqual("ADDED FIELD16", addedField.Title);
                Assert.AreEqual(FieldType.Lookup, addedField.FieldTypeKind);
                Assert.AreEqual("Title", addedField.LookupField);
                Assert.AreEqual(documents.Id, Guid.Parse(addedField.LookupList));
                Assert.AreEqual(currentWeb.Id, addedField.LookupWebId);

                await addedField.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWebFieldUserSpecificTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = await context.Web.Fields.AddUserAsync("ADDED FIELD17", new FieldUserOptions()
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
                Assert.AreEqual("ADDED FIELD17", addedField.Title);
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
        public async Task UpdateWebFieldTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField field = await context.Web.Fields.AddTextAsync("TO UPDATE FIELD", new FieldTextOptions());

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
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField field = await context.Web.Fields.AddTextAsync("TO DELETE FIELD");

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

        [TestMethod]
        public async Task TaxonomyFieldPropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var termStore = await context.TermStore.GetAsync(t => t.Groups);
                var group = termStore.Groups.AsRequested().FirstOrDefault(g => g.Name == "System");
                await group.LoadAsync(g => g.Sets);
                var termSet = group.Sets.AsRequested().FirstOrDefault();

                var fieldTitle = "tax_test_" + DateTime.UtcNow.Ticks;
                await context.Web.Fields.AddTaxonomyAsync(fieldTitle, new FieldTaxonomyOptions
                {
                    TermSetId = new Guid(termSet.Id),
                    TermStoreId = new Guid(termStore.Id),
                });

                // request it again, since not all properties are mapped on the initail creation request
                var newField = await context.Web.Fields.FirstOrDefaultAsync(f => f.Title == fieldTitle);

                Assert.IsTrue(newField.TermSetId.Equals(new Guid(termSet.Id)));
                Assert.IsTrue(newField.SspId.Equals(new Guid(termStore.Id)));
                Assert.IsTrue(newField.IsTermSetValid);

                var fieldsToDelete = await context.Web.Fields.Where(f => f.Title.StartsWith("tax_test")).ToListAsync();

                foreach (var field in fieldsToDelete)
                {
                    field.Delete();
                }
            }
        }

        [TestMethod]
        public async Task AddTaxonomyFieldWithDefaultsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {                

                try
                {
                    var termStore = await context.TermStore.GetAsync(t => t.Groups);
                    var group = termStore.Groups.AsRequested().FirstOrDefault(g => g.Name == "System");
                    await group.LoadAsync(g => g.Sets);
                    var termSet = group.Sets.AsRequested().FirstOrDefault();

                    await termSet.LoadAsync(p => p.Terms);

                    ITerm term1 = termSet.Terms.AsRequested().First();
                    ITerm term2 = termSet.Terms.AsRequested().Last();

                    var fieldTitle = "tax_test_" + DateTime.UtcNow.Ticks;
                    await context.Web.Fields.AddTaxonomyAsync(fieldTitle, new FieldTaxonomyOptions
                    {
                        TermSetId = new Guid(termSet.Id),
                        TermStoreId = new Guid(termStore.Id),  
                        DefaultValue = term1
                    });

                    // request it again, since not all properties are mapped on the initail creation request
                    var newField = await context.Web.Fields.FirstOrDefaultAsync(f => f.Title == fieldTitle);

                    Assert.IsTrue(newField.TermSetId.Equals(new Guid(termSet.Id)));
                    Assert.IsTrue(newField.SspId.Equals(new Guid(termStore.Id)));
                    Assert.IsTrue(newField.IsTermSetValid);
                    Assert.IsTrue(newField.DefaultValue.ToString().Contains($"{term1.Id}"));

                    // Update the default value
                    newField.DefaultValue = $"-1;#{term2.Labels.First(p => p.IsDefault == true).Name}|{term2.Id}";
                    await newField.UpdateAsync();

                    newField = await context.Web.Fields.FirstOrDefaultAsync(f => f.Title == fieldTitle);

                    Assert.IsTrue(newField.TermSetId.Equals(new Guid(termSet.Id)));
                    Assert.IsTrue(newField.SspId.Equals(new Guid(termStore.Id)));
                    Assert.IsTrue(newField.IsTermSetValid);
                    Assert.IsTrue(newField.DefaultValue.ToString().Contains($"{term2.Id}"));

                    fieldTitle = "tax_test_" + DateTime.UtcNow.Ticks;
                    await context.Web.Fields.AddTaxonomyMultiAsync(fieldTitle, new FieldTaxonomyOptions
                    {
                        TermSetId = new Guid(termSet.Id),
                        TermStoreId = new Guid(termStore.Id),
                        DefaultValues = new System.Collections.Generic.List<ITerm>() { term1, term2 }
                    });

                    newField = await context.Web.Fields.FirstOrDefaultAsync(f => f.Title == fieldTitle);

                    Assert.IsTrue(newField.TermSetId.Equals(new Guid(termSet.Id)));
                    Assert.IsTrue(newField.SspId.Equals(new Guid(termStore.Id)));
                    Assert.IsTrue(newField.IsTermSetValid);
                    Assert.IsTrue(newField.DefaultValue.ToString().Contains($"{term1.Id}"));
                    Assert.IsTrue(newField.DefaultValue.ToString().Contains($"{term2.Id}"));

                }
                finally
                {
                    var fieldsToDelete = await context.Web.Fields.Where(f => f.Title.StartsWith("tax_test")).ToListAsync();

                    foreach (var field in fieldsToDelete)
                    {
                        field.Delete();
                    }
                }
            }
        }

        [TestMethod]
        public async Task AddOpenTaxonomyFieldTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                try
                {
                    var termStore = await context.TermStore.GetAsync(t => t.Groups);
                    var group = termStore.Groups.AsRequested().FirstOrDefault(g => g.Name == "System");
                    await group.LoadAsync(g => g.Sets);
                    var termSet = group.Sets.AsRequested().FirstOrDefault();

                    await termSet.LoadAsync(p => p.Terms);

                    ITerm term1 = termSet.Terms.AsRequested().First();
                    ITerm term2 = termSet.Terms.AsRequested().Last();

                    var fieldTitle = "tax_test_" + DateTime.UtcNow.Ticks;
                    await context.Web.Fields.AddTaxonomyAsync(fieldTitle, new FieldTaxonomyOptions
                    {
                        TermSetId = new Guid(termSet.Id),
                        TermStoreId = new Guid(termStore.Id),
                        OpenTermSet = true
                    });

                    // request it again, since not all properties are mapped on the initail creation request
                    var newField = await context.Web.Fields.FirstOrDefaultAsync(f => f.Title == fieldTitle);

                    Assert.IsTrue(newField.TermSetId.Equals(new Guid(termSet.Id)));
                    Assert.IsTrue(newField.SspId.Equals(new Guid(termStore.Id)));
                    Assert.IsTrue(newField.IsTermSetValid);
                    Assert.IsTrue(newField.Open);
                }
                finally
                {
                    var fieldsToDelete = await context.Web.Fields.Where(f => f.Title.StartsWith("tax_test")).ToListAsync();

                    foreach (var field in fieldsToDelete)
                    {
                        field.Delete();
                    }
                }
            }
        }

        [TestMethod]
        public async Task AddWebFieldAndPropagateChanges()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = null;
                IList addedList = null;

                try
                {
                    // Create field
                    addedField = await context.Web.Fields.AddTextAsync("PropagateFieldChanges", new FieldTextOptions());

                    // Create list and add field
                    string listTitle = TestCommon.GetPnPSdkTestAssetName("AddWebFieldAndPropagateChanges");
                    addedList = context.Web.Lists.GetByTitle(listTitle);

                    if (TestCommon.Instance.Mocking && addedList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (addedList == null)
                    {
                        addedList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    }

                    // Add field
                    var listField = addedList.Fields.AddFieldAsXml(addedField.SchemaXml);

                    // Set default value
                    addedField.DefaultValue = "B";

                    // Push update of added field, will also trigger update of the list field
                    addedField.UpdateAndPushChanges();

                    // Load the list field again
                    var listFieldReloaded = addedList.Fields.QueryProperties(p => p.DefaultValue).FirstOrDefault(p => p.Id == addedField.Id);

                    // Check if the default value has been updated on the field in the list
                    Assert.IsTrue(listFieldReloaded.DefaultValue.ToString() == "B");

                }
                finally
                {
                    if (addedList != null)
                    {
                        await addedList.DeleteAsync();
                    }

                    await addedField.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task AddWebFieldAndPropagateChangesBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IField addedField = null;
                IList addedList = null;

                try
                {
                    // Create field
                    addedField = await context.Web.Fields.AddTextAsync("PropagateChangesBatch", new FieldTextOptions());

                    // Create list and add field
                    string listTitle = TestCommon.GetPnPSdkTestAssetName("AddWebFieldAndPropagateChangesBatch");
                    addedList = context.Web.Lists.GetByTitle(listTitle);

                    if (TestCommon.Instance.Mocking && addedList != null)
                    {
                        Assert.Inconclusive("Test data set should be setup to not have the list available.");
                    }

                    if (addedList == null)
                    {
                        addedList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                    }

                    // Add field
                    var listField = addedList.Fields.AddFieldAsXml(addedField.SchemaXml);

                    // Set default value
                    addedField.DefaultValue = "B";

                    // Push update of added field, will also trigger update of the list field
                    addedField.UpdateAndPushChangesBatch();

                    // Execute the batch
                    context.Execute();

                    // Load the list field again
                    var listFieldReloaded = addedList.Fields.QueryProperties(p => p.DefaultValue).FirstOrDefault(p => p.Id == addedField.Id);

                    // Check if the default value has been updated on the field in the list
                    Assert.IsTrue(listFieldReloaded.DefaultValue.ToString() == "B");

                }
                finally
                {
                    if (addedList != null)
                    {
                        await addedList.DeleteAsync();
                    }

                    await addedField.DeleteAsync();
                }
            }
        }

    }
}
