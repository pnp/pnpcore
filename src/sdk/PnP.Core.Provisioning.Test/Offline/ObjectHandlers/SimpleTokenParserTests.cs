using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;

namespace PnP.Core.Provisioning.Test.Offline.ObjectHandlers
{
    /// <summary>
    /// Covers the parsing rules shared by <see cref="SimpleTokenParser"/> and the full
    /// <see cref="TokenParser"/>.
    /// </summary>
    [TestClass]
    public class SimpleTokenParserTests
    {
        private static SimpleTokenParser ParserWith(params (string Key, string Value)[] parameters)
        {
            var parser = new SimpleTokenParser();
            foreach ((string key, string value) in parameters)
            {
                parser.AddToken(new WebhookParameter(key, value));
            }
            return parser;
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ParseString_ReplacesAKnownToken()
        {
            SimpleTokenParser parser = ParserWith(("SiteUrl", "https://contoso.sharepoint.com/sites/team"));

            Assert.AreEqual("https://contoso.sharepoint.com/sites/team",
                parser.ParseString("{webhookparam:SiteUrl}"));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ParseString_IsCaseInsensitive()
        {
            SimpleTokenParser parser = ParserWith(("SiteUrl", "resolved"));

            Assert.AreEqual("resolved", parser.ParseString("{WEBHOOKPARAM:SITEURL}"));
            Assert.AreEqual("resolved", parser.ParseString("{webhookparam:siteurl}"));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ParseString_ReplacesEveryOccurrenceInALongerString()
        {
            SimpleTokenParser parser = ParserWith(("Name", "Contoso"));

            Assert.AreEqual("Contoso and Contoso again",
                parser.ParseString("{webhookparam:Name} and {webhookparam:Name} again"));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ParseString_LeavesUnknownTokensInPlace()
        {
            SimpleTokenParser parser = ParserWith(("Known", "value"));

            Assert.AreEqual("{webhookparam:Unknown}", parser.ParseString("{webhookparam:Unknown}"));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ParseString_PassesThroughInputWithNoTokenCharacters()
        {
            SimpleTokenParser parser = ParserWith(("Known", "value"));

            Assert.AreEqual("nothing to do here", parser.ParseString("nothing to do here"));
            Assert.AreEqual(string.Empty, parser.ParseString(string.Empty));
            Assert.IsNull(parser.ParseString(null));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ParseString_LeavesBareGuidsAlone()
        {
            SimpleTokenParser parser = ParserWith(("Known", "value"));

            const string guid = "{f2cd6d5b-1391-480e-a3dc-7f7f96137382}";
            Assert.AreEqual(guid, parser.ParseString(guid));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ParseString_ResolvesATokenEmbeddedInXml()
        {
            SimpleTokenParser parser = ParserWith(("ListTitle", "Documents"));

            Assert.AreEqual("<List Title=\"Documents\" />",
                parser.ParseString("<List Title=\"{webhookparam:ListTitle}\" />"));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ParseString_ResolvesAValueThatItselfContainsBraces()
        {
            SimpleTokenParser parser = ParserWith(("Json", "{\"key\":\"value\"}"));

            Assert.AreEqual("{\"key\":\"value\"}", parser.ParseString("{webhookparam:Json}"));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void AddToken_RegistersBothAliases()
        {
            SimpleTokenParser parser = ParserWith(("Name", "Contoso"));

            Assert.AreEqual("Contoso", parser.ParseString("{webhookparam:Name}"));
            Assert.AreEqual("Contoso", parser.ParseString("{webhookparameter:Name}"));
        }

        [TestMethod]
        [TestCategory("Offline")]
        public void ParseString_HandlesTokenNamesContainingRegexMetacharacters()
        {
            SimpleTokenParser parser = ParserWith(("My Param (2024)", "resolved"));

            Assert.AreEqual("resolved", parser.ParseString("{webhookparam:My Param (2024)}"));
        }
    }
}
