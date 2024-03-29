using RfcBuddy.App.Objects;
using System.Text.RegularExpressions;

namespace RfcBuddy.App.Core;

public static class RfcHelper
{
    /// <summary>
    /// A separator used to delimit fields in the previousRfcs file.
    /// </summary>
    private const char previousRfcSeparator = '#';
    
    /// <summary>
    /// The date format to keep track of previous RFCs
    /// </summary>
    private const string previousRfcDateFormat = "yyyy-MM-dd-HHmmss";

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

    /// <summary>
    /// Loads the previously reviewed RFCs into a list.
    /// </summary>
    /// <param name="previousFilePath">The file with the previously reviewed RFCs.</param>
    /// <returns>A list of the previously reveiwed RFCs. If the file is not found, an empty list is returned.</returns>
    public static List<PreviousRfc> GetPreviousRfcs(string previousFilePath)
    {
        List<PreviousRfc> result = [];
        if (File.Exists(previousFilePath))
        {
            using StreamReader previousRfcs = File.OpenText(previousFilePath);
            while (!previousRfcs.EndOfStream)
            {
                string? previousRfc = previousRfcs.ReadLine();
                if (null != previousRfc)
                {
                    string[] rfcElements = previousRfc.Split(previousRfcSeparator);
                    if (6 == rfcElements.Length)  //Very basic check whether the line is valid - it contains 6 elements.
                    {
                        result.Add(new PreviousRfc(rfcElements[0])
                        {
                            StartDate = DateTime.ParseExact(rfcElements[1], previousRfcDateFormat, System.Globalization.CultureInfo.InvariantCulture),
                            EndDate = DateTime.ParseExact(rfcElements[2], previousRfcDateFormat, System.Globalization.CultureInfo.InvariantCulture),
                            AssetTagsHash = rfcElements[3],
                            DescriptionHash = rfcElements[4],
                            RiskAssessmentHash = rfcElements[5]
                        });
                    }
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Saves the given RFCs to a file, with a hash of the dates, asset tags, description, and risk assessment for later comparison.
    /// Note: Any file at the given location will be replaced.
    /// </summary>
    /// <param name="previousFilePath">The file to write to.</param>
    /// <param name="rfcs">The RFCs to write to the file.</param>
    public static void SavePreviousRfcs(string previousFilePath, IEnumerable<Rfc> rfcs)
    {
        if (!File.Exists(previousFilePath))
        {
            using FileStream tmp = File.Create(previousFilePath);
            tmp.Close();
        }
        using FileStream previousRFCFile = File.Open(previousFilePath, FileMode.Truncate);
        using (StreamWriter previousRFCs = new(previousRFCFile))
        {
            foreach (Rfc rfc in rfcs)
            {
                previousRFCs.WriteLine(rfc.RfcNumber.ToString()
                    + previousRfcSeparator + rfc.StartDate.ToString(previousRfcDateFormat)
                    + previousRfcSeparator + rfc.EndDate.ToString(previousRfcDateFormat)
                    + previousRfcSeparator + Cryptography.GetSha256Hash(rfc.AssetTags)
                    + previousRfcSeparator + Cryptography.GetSha256Hash(rfc.Description)
                    + previousRfcSeparator + Cryptography.GetSha256Hash(rfc.RiskAssessment));
            }
            previousRFCs.Close();
        }
        previousRFCFile.Close();
    }
}
