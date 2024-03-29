using System.ComponentModel.DataAnnotations;

namespace RfcBuddy.Web.Models;

public class HomeViewModel
{
    [Required(ErrorMessage =@"Please enter the ministry-specific keywords, or ""N/A"" to confirm that this field should be ignored.")]
    [Display(Name = "Ministry-specific keywords, e.g. server names or file shares")]
    public string MinistryKeywords { get; set; } = string.Empty;

    [Required(ErrorMessage = @"Please enter the generic keywords, or ""N/A"" to confirm that this field should be ignored.")]
    [Display(Name = "Generic keywords that affetc all ministries, e.g. Siteminder")]
    public string GeneralKeywords { get; set; } = string.Empty;

    [Required(ErrorMessage = @"Please enter the keywords to filter out, or ""N/A"" to confirm that this field should be ignored.")]
    [Display(Name = "Keywords for RFCs that should be filtered out")]
    public string IgnoreKeywords { get; set; } = string.Empty;
}
