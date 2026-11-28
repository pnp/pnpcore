using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Publishing;
using Moq;


namespace PnP.Core.Modernization.Tests.Transform.Publishing
{
    [TestClass]
    [TestCategory("Offline")]
    public class PublishingPageTransformationInformationTests
    {
        string EmptyField = "EmptyField";
        string FieldWithValue = "FieldWithValue";


        [TestMethod]
        public void GetGetFirstNonEmptyFieldName_EmptyArrayTest()
        {
            // Arrange
            var ppti = new PublishingPageTransformationInformation(null);
            string[] fieldNames = new string[0];

            // Act
            string fieldName = ppti.GetFirstNonEmptyFieldName(fieldNames);

            // Assert
            Assert.AreEqual(string.Empty, fieldName);
        }

        [TestMethod]
        public void GetGetFirstNonEmptyFieldName_TrimTest()
        {
            // Arrange
            var ppti = new PublishingPageTransformationInformation(null);
            string[] fieldNames = new string[] {"   ","" };

            // Act
            string fieldName = ppti.GetFirstNonEmptyFieldName(fieldNames);

            // Assert
            Assert.AreEqual(string.Empty, fieldName);
        }

        [TestMethod]
        public void GetGetFirstNonEmptyFieldName_RetrieveCorrectFieldTest()
        {
            // Arrange
            #region Configure Mock
            var mock = new Mock<PublishingPageTransformationInformation>
            {
                CallBase = true // <-- let mock call base members, except when "setup"
            };

            // hard coded results
            mock.Setup(p => p.IsFieldUsed(EmptyField)).Returns(false);
            mock.Setup(p => p.IsFieldUsed(FieldWithValue)).Returns(true);
            #endregion
            var ppti = mock.Object as PublishingPageTransformationInformation;
            string[] fieldNames = new string[] { EmptyField, FieldWithValue };

            // Act
            string fieldName = ppti.GetFirstNonEmptyFieldName(fieldNames);

            // Assert
            Assert.AreEqual(FieldWithValue, fieldName);
        }

        [TestMethod]
        public void GetGetFirstNonEmptyFieldName_FullTest()
        {
            // Arrange
            #region Configure Mock
            var mock = new Mock<PublishingPageTransformationInformation>
            {
                CallBase = true // <-- let mock call base members, except when "setup"
            };

            // hard coded results
            mock.Setup(p => p.IsFieldUsed(EmptyField)).Returns(false);
            mock.Setup(p => p.IsFieldUsed(FieldWithValue)).Returns(true);
            #endregion
            var ppti = mock.Object as PublishingPageTransformationInformation;
            string[] fieldNames = new string[] { "", "   ", EmptyField, " " + FieldWithValue + "   " };

            // Act
            string fieldName = ppti.GetFirstNonEmptyFieldName(fieldNames);

            // Assert
            Assert.AreEqual(FieldWithValue, fieldName);
        }
    }
}
