using ExcelDataReader;
using RfcBuddy.App.Objects;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace RfcBuddy.App.Core;

public static class ExcelHelper
{
    //Column numbers in the Excel sheet with useful values
    private const int colRfcNo = 0;
    private const int colApproval = 1;
    private const int colPlatform = 2;
    private const int colAssetTag = 3;
    private const int colStartDate = 4;
    private const int colEndDate = 5;
    private const int colDescription = 6;
    private const int colRisk = 7;

    /// <summary>
    /// Processes the OCIO changes
    /// </summary>
    /// <param name="excelFile">The 365-day Excel input file</param>
    /// <param name="ministryKeywords">A list of ministry-relevant keywords</param>
    /// <param name="generalKeywords">A list of keywords that affect Government systems used by the minisry</param>
    /// <param name="ignoreKeywords">A list of keywords to ignore (e.g. systems from other ministries)</param>
    public static void ProcessExcelSheet(string excelFile, List<string> ministryKeywords, List<string> generalKeywords, List<string> ignoreKeywords, out List<Rfc> ministryRfcs, out List<Rfc> generalRfcs, out List<Rfc> otherRfcs, out int totalRfcs)
    {
        //Required for the ExcelDataReader to understand the encoding of the 365-day change Excel file.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ministryRfcs = [];
        generalRfcs = [];
        otherRfcs = [];
        totalRfcs = 0;
        //Read Excel file
        DataSet? excelData = null;
        using (var excelFileStream = File.Open(excelFile, FileMode.Open, FileAccess.Read))
        {
            using var excelDataReader = ExcelReaderFactory.CreateReader(excelFileStream);
            excelData = excelDataReader.AsDataSet();
        }
        if (null != excelData && excelData.Tables.Count > 0)
        {
            //Parse and filter RFCs from Excel data
            DataTable Rfcs = excelData.Tables[0];
            int currentRow = 1;
            while (currentRow < Rfcs.Rows.Count)
            {
                Rfc currentRfc = ReadRfc(ref Rfcs, currentRow);
                if (!string.IsNullOrEmpty(currentRfc.RfcNumber))
                {
                    totalRfcs++;
                    //Ignore RFCs that contain keywords that should be filtered out.
                    if (!RfcKeywordMatches(ref currentRfc, ignoreKeywords))
                    {
                        if (RfcKeywordMatches(ref currentRfc, ministryKeywords))
                        {
                            ministryRfcs.Add(currentRfc);
                        }
                        else if (RfcKeywordMatches(ref currentRfc, generalKeywords))
                        {
                            generalRfcs.Add(currentRfc);
                        }
                        else
                        {
                            otherRfcs.Add(currentRfc);
                        }
                    }
                }
                currentRow++;
            }
        }
    }

    /// <summary>
    /// Reads the RFC starting at the given row
    /// </summary>
    /// <param name="changes">The source data with all RFCs</param>
    /// <param name="currentRow">The row where the current RFC starts</param>
    /// <returns></returns>
    internal static Rfc ReadRfc(ref DataTable changes, int currentRow)
    {
        Rfc result = new(string.Empty);
        if (currentRow < changes.Rows.Count && null != changes.Rows[currentRow].ItemArray[colRfcNo])
        {
            result = new Rfc(changes.Rows[currentRow].ItemArray[colRfcNo]?.ToString() ?? string.Empty)
            {
                ApprovalStatus = changes.Rows[currentRow].ItemArray[colApproval]?.ToString() ?? string.Empty,
                Platform = changes.Rows[currentRow].ItemArray[colPlatform]?.ToString() ?? string.Empty,
                AssetTags = changes.Rows[currentRow].ItemArray[colAssetTag]?.ToString() ?? string.Empty,
                Description = changes.Rows[currentRow].ItemArray[colDescription]?.ToString() ?? string.Empty,
                RiskAssessment = changes.Rows[currentRow].ItemArray[colRisk]?.ToString() ?? string.Empty
            };
            if (DateTime.TryParse(changes.Rows[currentRow].ItemArray[colStartDate]?.ToString(), out DateTime startDate))
            {
                result.StartDate = startDate;
            }
            if (DateTime.TryParse(changes.Rows[currentRow].ItemArray[colEndDate]?.ToString(), out DateTime endDate))
            {
                result.EndDate = endDate;
            }
        }
        return result;
    }

    /// <summary>
    /// Checks whether a list of keywords matches the given RFC.
    /// Matching is case-insensitive, and only matches whole words.
    /// </summary>
    /// <param name="rfc">The RFC to check</param>
    /// <param name="keywords">The list of keywords</param>
    /// <returns></returns>
    internal static bool RfcKeywordMatches(ref Rfc rfc, List<string> keywords)
    {
        bool match = false;
        foreach (string keyword in keywords)
        {
            string pattern = @"\b" + keyword + @"\b";
            if (Regex.IsMatch(rfc.AssetTags, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(1000))
                || Regex.IsMatch(rfc.Description, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(1000))
                || Regex.IsMatch(rfc.RiskAssessment, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(1000)))
            {
                match = true;
                rfc.Keywords.Add(keyword);
            }
        }
        return match;
    }
}
