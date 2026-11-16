using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Tests.Transform.Utility
{
    [TestClass]
    [TestCategory("Offline")]
    public class StringExtensionTests
    {
        [TestMethod]
        public void StringExtension_ContainsTest()
        {
            var original = "ThisIsAFolderInAPath/Testing";
            var partialText = "thisisafolderinapath/testing";

            var result = original.ContainsIgnoringCasing(partialText);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void StringExtension_ContainsPartialTest()
        {
            var original = "ThisIsAFolderInAPath/Testing";
            var partialText = "thisisafolderinapath";

            var result = original.ContainsIgnoringCasing(partialText);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void StringExtension_NotContainsTest()
        {
            var original = "ThisIsAFolderInAPath/Testing";
            var partialText = "somethingcompletelydifferent";

            var result = original.ContainsIgnoringCasing(partialText);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void StringExtension_EmptyCheckContainsTest()
        {
            var original = "ThisIsAFolderInAPath/Testing";
            var partialText = "";

            var result = original.ContainsIgnoringCasing(partialText);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void StringExtension_Emptyheck2ContainsTest()
        {
            var original = "";
            var partialText = "somethingcompletelydifferent";

            var result = original.ContainsIgnoringCasing(partialText);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void StringExtension_StripString()
        {
            var input = "/sites/pnptransformationsource/en/pages/search.aspx";
            var expectedResult = "/en/pages/search.aspx";

            var result = input.StripRelativeUrlSectionString();

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void StringExtension_StripNoSitesString()
        {
            var input = "pnptransformationsource/en/pages/search.aspx";
            var expectedResult = "pnptransformationsource/en/pages/search.aspx";

            var result = input.StripRelativeUrlSectionString();

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void StringExtension_GetBaseUrlValidHttpsString()
        {

            var input = "https://server/pnptransformationsource/en/pages/search.aspx";
            var expectedResult = "https://server";

            var result = input.GetBaseUrl();

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void StringExtension_GetBaseUrlValidHttpString()
        {

            var input = "http://server/pnptransformationsource/en/pages/search.aspx";
            var expectedResult = "http://server";

            var result = input.GetBaseUrl();

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void StringExtension_GetBaseUrlInvalidString()
        {

            var input = "/pnptransformationsource/en/pages/search.aspx";
            var expectedResult = string.Empty;

            var result = input.GetBaseUrl();

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void StringExtension_GetBaseUrEmptyString()
        {

            var input = string.Empty;
            var expectedResult = string.Empty;

            var result = input.GetBaseUrl();

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void StringExtension_GetClassNameFromType()
        {

            var input = "Microsoft.SharePoint.WebPartPages";
            var expectedResult = "WebPartPages";

            var result = input.InferClassNameFromNameSpace();

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void StringExtension_GetClassNameFromFullType()
        {

            var input = "Microsoft.SharePoint.Publishing.TemplateRedirectionPage,Microsoft.SharePoint.Publishing,Version=16.0.0.0,Culture=neutral,PublicKeyToken=71e9bce111e9429c";
            var expectedResult = "TemplateRedirectionPage";

            var result = input.InferClassNameFromNameSpace();

            Assert.AreEqual(expectedResult, result);
        }
    }
}
