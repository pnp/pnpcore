using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Utilities;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class UtilitiesTests
    {
        [TestMethod()]
        [DataRow("dsfsd", "dsfsd")]
        [DataRow("ds&,!@;:#¤`´~¨=%<>fsd", "dsfsd")]
        [DataRow("dsfsdáâĕėìíõøùúďđ", "dsfsdaaeeiioouudd")]
        public void NormalizeInputTest(string input, string alias)
        {
            input = NormalizeInput.RemoveUnallowedCharacters(input);
            input = NormalizeInput.ReplaceAccentedCharactersWithLatin(input);
            Assert.AreEqual(alias, input);
        }

    }
}
