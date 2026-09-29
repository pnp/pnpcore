using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.ObjectHandlers;
using System.Xml.Linq;

namespace PnP.Core.Provisioning.Test.Offline.ObjectHandlers
{
    /// <summary>
    /// Pins when applying a site column to a site that already has it is recognised as changing nothing,
    /// so the engine does not write it back.
    /// </summary>
    [TestClass]
    [TestCategory("Offline")]
    public class FieldSchemaMergeTests
    {
        /// <summary>
        /// Merges the template into the site's schema the way an update does and reports whether that changed anything.
        /// </summary>
        private static bool MergeChangesSomething(string existingXml, string templateXml)
        {
            XElement existing = XElement.Parse(existingXml);
            XElement before = new XElement(existing);

            ObjectField.MergeInto(existing, XElement.Parse(templateXml));
            existing.Attributes("Version").Remove();

            return !ObjectField.SchemasMatch(before, existing);
        }

        /// <summary>
        /// SharePoint's own TemplateId column as a site holds it after being saved, against the definition a
        /// template extracted from another site carries.
        /// </summary>
        [TestMethod]
        public void ATemplateSayingLessThanTheSiteChangesNothing()
        {
            const string onTheSite = "<Field ID=\"{467e811f-0c12-4a93-bb04-42ff0c1c597b}\" DisplayName=\"Template Id\" Type=\"Text\" " +
                "ShowInEditForm=\"FALSE\" Required=\"TRUE\" Name=\"TemplateId\" RowOrdinal=\"0\" Group=\"_Hidden\" " +
                "SourceID=\"{74902c5a-e792-43ed-94c6-eaa1c85ee80c}\" StaticName=\"TemplateId\" Version=\"54\" />";

            const string inTheTemplate = "<Field ID=\"{467E811F-0C12-4A93-BB04-42FF0C1C597B}\" DisplayName=\"Template Id\" Type=\"Text\" " +
                "ShowInEditForm=\"FALSE\" Required=\"TRUE\" Name=\"TemplateId\" RowOrdinal=\"0\" Group=\"_Hidden\" />";

            Assert.IsFalse(MergeChangesSomething(onTheSite, inTheTemplate));
        }

        [TestMethod]
        public void ElementsTheMergeMovesToTheEndChangeNothing()
        {
            const string onTheSite = "<Field ID=\"{4e1a0f01-1111-4a11-9c11-000000000001}\" Type=\"Choice\" Name=\"PnPProjectStatus\" " +
                "DisplayName=\"Status\" Version=\"3\"><Default>Active</Default><CHOICES><CHOICE>Active</CHOICE><CHOICE>Closed</CHOICE></CHOICES></Field>";

            const string inTheTemplate = "<Field ID=\"{4E1A0F01-1111-4A11-9C11-000000000001}\" Type=\"Choice\" Name=\"PnPProjectStatus\" " +
                "DisplayName=\"Status\"><CHOICES><CHOICE>Active</CHOICE><CHOICE>Closed</CHOICE></CHOICES><Default>Active</Default></Field>";

            Assert.IsFalse(MergeChangesSomething(onTheSite, inTheTemplate));
        }

        [TestMethod]
        public void AChangedAttributeIsAChange()
        {
            const string onTheSite = "<Field ID=\"{4e1a0f01-1111-4a11-9c11-000000000002}\" Type=\"Text\" Name=\"PnPProjectCode\" " +
                "DisplayName=\"Project code\" Version=\"1\" />";

            Assert.IsTrue(MergeChangesSomething(onTheSite,
                "<Field ID=\"{4e1a0f01-1111-4a11-9c11-000000000002}\" Type=\"Text\" Name=\"PnPProjectCode\" DisplayName=\"Code\" />"));

            Assert.IsTrue(MergeChangesSomething(onTheSite,
                "<Field ID=\"{4e1a0f01-1111-4a11-9c11-000000000002}\" Type=\"Text\" Name=\"PnPProjectCode\" Indexed=\"TRUE\" />"),
                "An attribute the site does not have yet is a change.");
        }

        [TestMethod]
        public void AChangedChildElementIsAChange()
        {
            const string onTheSite = "<Field ID=\"{4e1a0f01-1111-4a11-9c11-000000000001}\" Type=\"Choice\" Name=\"PnPProjectStatus\">" +
                "<CHOICES><CHOICE>Active</CHOICE><CHOICE>Closed</CHOICE></CHOICES></Field>";

            Assert.IsTrue(MergeChangesSomething(onTheSite,
                "<Field ID=\"{4e1a0f01-1111-4a11-9c11-000000000001}\" Type=\"Choice\" Name=\"PnPProjectStatus\">" +
                "<CHOICES><CHOICE>Closed</CHOICE><CHOICE>Active</CHOICE></CHOICES></Field>"),
                "The order of the choices is part of the column, so reordering them is a change.");

            Assert.IsTrue(MergeChangesSomething(onTheSite,
                "<Field ID=\"{4e1a0f01-1111-4a11-9c11-000000000001}\" Type=\"Choice\" Name=\"PnPProjectStatus\">" +
                "<CHOICES><CHOICE>Active</CHOICE><CHOICE>Closed</CHOICE><CHOICE>On hold</CHOICE></CHOICES></Field>"));
        }
    }
}
