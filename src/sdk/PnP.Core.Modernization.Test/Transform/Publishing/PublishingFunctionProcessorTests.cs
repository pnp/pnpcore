using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Publishing;

namespace PnP.Core.Modernization.Tests.Transform.Publishing
{
    [TestClass]
    [TestCategory("Offline")]
    public class PublishingFunctionProcessorTests
    {
        public string ExpectedPropertyName = "Title";

        [DataTestMethod]
        [DataRow("<img alt='Original image' src='/images/photo.png'>", "")]
        [DataRow("<div class='ms-rtestate-field'><img alt='Original image' src='/images/photo.png'></div>", "")]
        [DataRow("<div class='ms-rtestate-field'><a href='/guide'><img alt='Original image' src='/images/photo.png'></a></div>", "/guide")]
        public void PublishingImageFunctionsAcceptCoreRestStreamWrappers(string html, string anchor)
        {
            using var context = CoreTestContext.Create();
            var functions = new PublishingBuiltIn(new PublishingPageTransformationInformation(null), context, context);

            Assert.AreEqual("/images/photo.png", functions.ToImageUrl(html));
            Assert.AreEqual("Original image", functions.ToImageAltText(html));
            Assert.AreEqual(anchor, functions.ToImageAnchor(html));
            Assert.AreEqual("/images/plain.png", functions.ToImageUrl("/images/plain.png"));
        }

        [TestMethod]
        public void ResolveFunctionToken_SimpleReplacementTest()
        {
            // Arrange
            var funcProc = new PublishingFunctionProcessor(null, null, null, null, null, null);
            // use nothing other than the token as the input function
            string functionString = funcProc.NameAttributeToken;

            // Act
            string newFunction = funcProc.ResolveFunctionToken(functionString, ExpectedPropertyName);

            // Assert
            Assert.AreEqual(ExpectedPropertyName, newFunction);
        }

        [TestMethod]
        public void ResolveFunctionToken_CaseSensitivityTest()
        {
            // Arrange
            var funcProc = new PublishingFunctionProcessor(null, null, null, null, null, null);
            // use nothing other than the token as the input function
            string functionStringLower = funcProc.NameAttributeToken.ToLower();
            string functionStringUpper = funcProc.NameAttributeToken.ToUpper();

            // Act
            string newFunctionLower = funcProc.ResolveFunctionToken(functionStringLower, ExpectedPropertyName);
            string newFunctionUpper = funcProc.ResolveFunctionToken(functionStringUpper, ExpectedPropertyName);

            // Assert
            Assert.AreEqual(ExpectedPropertyName, newFunctionLower);
            Assert.AreEqual(ExpectedPropertyName, newFunctionUpper);
        }

        [TestMethod]
        public void ResolveFunctionToken_MultipleOccurencesTest()
        {
            // Arrange
            var funcProc = new PublishingFunctionProcessor(null, null, null, null, null, null);
            // use nothing other than the token as the input function

            var templateString = "A='{0}';B='{1}';C='{2}'";
            
            // enter the token value in a multitude of formats
            string inputFunctionString = string.Format(templateString,
                funcProc.NameAttributeToken,
                funcProc.NameAttributeToken.ToUpper(),
                funcProc.NameAttributeToken.ToLower());

            // get our expected output .. where all tokens are resolved to our "property name"
            string expectedOutput = string.Format(templateString,
                ExpectedPropertyName, ExpectedPropertyName, ExpectedPropertyName);


            // Act
            // do our token resolution
            string newFunction = funcProc.ResolveFunctionToken(inputFunctionString, ExpectedPropertyName);

            // Assert
            // check the processed string matches our expected output
            Assert.AreEqual(expectedOutput, newFunction);
        }


        [TestMethod]
        public void ResolveFunctionToken_WithoutTokens()
        {
            // Arrange
            var funcProc = new PublishingFunctionProcessor(null, null, null, null, null, null);
            // use nothing other than the token as the input function

            var functionString = "A='foo'";

            // Act
            // if this throws any exceptions the test will fail
            string newFunction = funcProc.ResolveFunctionToken(functionString, ExpectedPropertyName);

            // Assert
            // make sure it hasn't changed
            Assert.AreEqual(functionString, newFunction);
        }
    }

}