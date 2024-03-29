using RfcBuddy.App.Objects;
using System.Drawing;
using System.Text.RegularExpressions;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace RfcBuddy.App.Core;

public class WordHelper
{
    //Color definitions in the Word document
    private readonly Color ministryHighlight = Color.Red;
    private readonly Color generalHighlight = Color.Purple;
    private readonly Color sundayHighlight = Color.Green;
    private readonly Color changeHighlight = Color.Blue;

    //The date format to use for start and end dates in the Word document
    private const string wordDateFormat = "ddd yyyy-MM-dd HH:mm";

    /// <summary>
    /// Creates a Word file from the given RFCs.
    /// </summary>
    /// <param name="wordFilePath">The word file stream</param>
    /// <param name="ministryRFCs">A list of ministry-related RFCs</param>
    /// <param name="generalRFCs">A list of general Government RFCs</param>
    /// <param name="otherRFCs">A list of other RFCs</param>
    public void CreateWordFile(ref Stream wordFile, List<Rfc> ministryRFCs, List<Rfc> generalRFCs, List<Rfc> otherRFCs, List<PreviousRfc> previousRFCs)
    {
        using DocX document = DocX.Create(wordFile);
        //Header and legend
        Paragraph title = document.InsertParagraph("RFCs for " + DateTime.Now.ToShortDateString());
        title.FontSize(24).Bold().UnderlineStyle(UnderlineStyle.singleLine).Color(Color.DarkBlue);
        title.Alignment = Alignment.center;
        document.InsertParagraph();
        Paragraph p1 = document.InsertParagraph("Ministry terms: ").Bold();
        p1.Append("Ministry example").Color(ministryHighlight);
        Paragraph p2 = document.InsertParagraph("General terms: ").Bold();
        p2.Append("General example").Color(generalHighlight);
        Paragraph p3 = document.InsertParagraph("Sunday change window: ").Bold();
        p3.Append("2020-01-05 06:00:00").Color(sundayHighlight);
        Paragraph p4 = document.InsertParagraph("Changed item: ").Bold();
        p4.Append("Updated Date or Description").Color(changeHighlight);

        //The following 3 categories are all divided in:
        //- In Progress
        //- New
        //- Previously Reviewed

        //Ministry RFCs
        Paragraph ministry = document.InsertParagraph("Ministry: " + ministryRFCs.Count + " RFCs");
        ministry.StyleId = "Heading1";
        ministry.Color(ministryHighlight);
        AddRfcSection(document, ministryRFCs, previousRFCs, ministryHighlight);

        //General RFCs
        Paragraph general = document.InsertParagraph("General: " + generalRFCs.Count + " RFCs");
        general.StyleId = "Heading1";
        general.Color(generalHighlight);
        AddRfcSection(document, generalRFCs, previousRFCs, generalHighlight);

        //Other RFCs
        Paragraph other = document.InsertParagraph("Other / unclassified: " + otherRFCs.Count + " RFCs");
        other.StyleId = "Heading1";
        AddRfcSection(document, otherRFCs, previousRFCs, Color.Black);

        document.Save();
    }

    /// <summary>
    /// Adds an RFC section to the document, based on a list of given RFCs.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="rfcs"></param>
    /// <param name="keywordHighlight"></param>
    private void AddRfcSection(DocX document, List<Rfc> rfcs, List<PreviousRfc> previousRFCs, Color keywordHighlight)
    {
        DateTime now = DateTime.Now;
        List<string> unchangedRFCs = [];
        //Changes that are currently in progress
        Paragraph pProgress = document.InsertParagraph("In Progress");
        pProgress.StyleId = "Heading2";
        int pProgressCount = 0;
        foreach (Rfc currentRFC in rfcs.Where(x => x.StartDate <= now && x.EndDate >= now))
        {
            //Don't worry about previous RFCs - we want to see all changes that are currently in progress.
            AddRfc(document, currentRFC, null, keywordHighlight);
            document.InsertParagraph();
            document.InsertParagraph();
            pProgressCount++;
        }
        if (0 == pProgressCount)
        {
            document.InsertParagraph("No RFCs found.");
        }

        //Changes that are new or changed since the last script run
        Paragraph pNew = document.InsertParagraph("New or Changed");
        pNew.StyleId = "Heading2";
        int pNewCount = 0;
        foreach (Rfc currentRFC in rfcs.Where(x => x.StartDate > now || x.EndDate < now))
        {
            PreviousRfc? previousRFC = previousRFCs.FirstOrDefault(x => x.RfcNumber == currentRFC.RfcNumber);
            if (null != previousRFC
                && currentRFC.StartDate == previousRFC.StartDate
                && currentRFC.EndDate == previousRFC.EndDate
                && Cryptography.VerifySha256Hash(currentRFC.AssetTags, previousRFC.AssetTagsHash)
                && Cryptography.VerifySha256Hash(currentRFC.Description, previousRFC.DescriptionHash)
                && Cryptography.VerifySha256Hash(currentRFC.RiskAssessment, previousRFC.RiskAssessmentHash)
                )
            {
                unchangedRFCs.Add(currentRFC.RfcNumber);
            }
            else
            {
                AddRfc(document, currentRFC, previousRFC, keywordHighlight);
                document.InsertParagraph();
                document.InsertParagraph();
                pNewCount++;
            }
        }
        if (0 == pNewCount)
        {
            document.InsertParagraph("No RFCs found.");
        }

        //Changes that were identical in the previous script run
        Paragraph pPrevious = document.InsertParagraph("Previously Reviewed");
        pPrevious.StyleId = "Heading2";
        int pPreviousCount = 0;
        foreach (Rfc currentRFC in rfcs.Where(x => unchangedRFCs.Contains(x.RfcNumber)))
        {
            AddRfc(document, currentRFC, null, keywordHighlight);
            document.InsertParagraph();
            document.InsertParagraph();
            pPreviousCount++;
        }
        if (0 == pPreviousCount)
        {
            document.InsertParagraph("No RFCs found.");
        }
    }

    /// <summary>
    /// Adds the given RFC to the document
    /// </summary>
    /// <param name="document">The document to add the RFC to</param>
    /// <param name="rfc">The RFC to add</param>
    /// <param name="keywordHighlight">The colour to use for any keyword highlighting</param>
    private void AddRfc(DocX document, Rfc rfc, PreviousRfc? previousRFC, Color keywordHighlight)
    {
        Paragraph newRFC = document.InsertParagraph();
        newRFC.Append("RFC: ").Bold();
        newRFC.Append(rfc.RfcNumber.ToString());
        newRFC.Append("\tStart: ").Bold();
        if (null != previousRFC && rfc.StartDate != previousRFC.StartDate)
        {
            newRFC.Append(rfc.StartDate.ToString(wordDateFormat)).Color(changeHighlight);
        }
        else if (DayOfWeek.Sunday == rfc.StartDate.DayOfWeek && "06:00:00" == rfc.StartDate.ToString("HH:mm:ss"))
        {
            newRFC.Append(rfc.StartDate.ToString(wordDateFormat)).Color(sundayHighlight);
        }
        else
        {
            newRFC.Append(rfc.StartDate.ToString(wordDateFormat));
        }
        newRFC.Append("\t\tEnd: ").Bold();
        if (null != previousRFC && rfc.EndDate != previousRFC.EndDate)
        {
            newRFC.Append(rfc.EndDate.ToString(wordDateFormat)).Color(changeHighlight);
        }
        else if (DayOfWeek.Sunday == rfc.EndDate.DayOfWeek && "09:00:00" == rfc.EndDate.ToString("HH:mm:ss"))
        {
            newRFC.Append(rfc.EndDate.ToString(wordDateFormat)).Color(sundayHighlight);
        }
        else
        {
            newRFC.Append(rfc.EndDate.ToString(wordDateFormat));
        }
        newRFC.Append(Environment.NewLine);
        newRFC.Append("Status: ").Bold();
        newRFC.Append(rfc.ApprovalStatus);
        newRFC.Append("\tPlatform: ").Bold();
        newRFC.Append(rfc.Platform);
        newRFC.Append("\tAsset Tags: ").Bold();
        if (null == previousRFC || Cryptography.VerifySha256Hash(rfc.AssetTags, previousRFC.AssetTagsHash))
        {
            newRFC.Append(rfc.AssetTags);
        }
        else
        {
            newRFC.Append(rfc.AssetTags).Color(changeHighlight);
        }
        newRFC.Append(Environment.NewLine);
        newRFC.Append("Description: ").Bold();
        if (null == previousRFC || Cryptography.VerifySha256Hash(rfc.Description, previousRFC.DescriptionHash))
        {
            newRFC.Append(rfc.Description);
        }
        else
        {
            newRFC.Append(rfc.Description).Color(changeHighlight);
        }
        newRFC.Append(Environment.NewLine);
        newRFC.Append("Risk Assessment: ").Bold();
        if (null == previousRFC || Cryptography.VerifySha256Hash(rfc.RiskAssessment, previousRFC.RiskAssessmentHash))
        {
            newRFC.Append(rfc.RiskAssessment);
        }
        else
        {
            newRFC.Append(rfc.RiskAssessment).Color(changeHighlight);
        }
        //Highlight keywords

        foreach (string keyword in rfc.Keywords)
        {
            StringReplaceTextOptions replaceTextOptions = new()
            {
                SearchValue = @"\b" + keyword + @"\b",
                NewValue = keyword,
                TrackChanges = false,
                EscapeRegEx = false,
                RegExOptions = RegexOptions.IgnoreCase,
                NewFormatting = new Formatting() { FontColor = keywordHighlight }
            };
            newRFC.ReplaceText(replaceTextOptions);
        }
    }
}
