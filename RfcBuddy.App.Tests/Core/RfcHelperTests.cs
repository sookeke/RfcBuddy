using RfcBuddy.App.Objects;

namespace RfcBuddy.App.Core.Tests
{
    [TestClass()]
    public class RfcHelperTests
    {
        [TestMethod()]
        [DataRow(true, "Tag2", "Foo")]
        [DataRow(true, "tag2", "Foo")]  //Matches should be case-insensitive
        [DataRow(true, "Bla", "Description")]
        [DataRow(true, "No risk")]  //Spaces allowed within keywords
        [DataRow(false, "Foo", "Bar")]
        [DataRow(false, "Tag2 ")]  //Should not match with a trailing space
        [DataRow(false, " RFC")]  //Should not match with a leading space either
        public void RfcKeywordMatchesTest(bool expected, params string[] keywords)
        {
            Rfc rfc = new("123456")
            {
                AssetTags = "Tag1 ,Tag2,Tag3",
                Description = "RFC description",
                RiskAssessment = "No risk to assets",
            };
            var actual = RfcHelper.RfcKeywordMatches(ref rfc, [.. keywords]);
            Assert.AreEqual(expected, actual);
        }
    }
}