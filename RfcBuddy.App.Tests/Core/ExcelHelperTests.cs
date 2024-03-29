using RfcBuddy.App.Objects;
using System.Data;

namespace RfcBuddy.App.Core.Tests
{
    [TestClass()]
    public class ExcelHelperTests
    {
        [TestMethod()]
        public void ReadRfcTest()
        {
            DataTable dt = new();
            dt.Columns.Add("RfcNo");
            dt.Columns.Add("Approval");
            dt.Columns.Add("Platform");
            dt.Columns.Add("AssetTag");
            dt.Columns.Add("StartDate");
            dt.Columns.Add("EndDate");
            dt.Columns.Add("Description");
            dt.Columns.Add("Risk");
            object[] data = new[] { "CHG0072560", "Approved", "Windows", "starsky, hutch", "2024 - 03 - 18  1:00:00 PM", "2024-03-25  1:00:00 PM", "Standard Change 049", "Low risk, this is a routine process that is repeated many times." };
            dt.LoadDataRow(data, true);
            Rfc rfc = ExcelHelper.ReadRfc(ref dt, 0);
            Assert.AreEqual("CHG0072560", rfc.RfcNumber);
            Assert.AreEqual("Approved", rfc.ApprovalStatus);
        }
    }
}