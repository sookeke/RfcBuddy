using System.Net;

namespace RfcBuddy.App.Core;

public static class WebHelper
{
    /// <summary>
    /// Downloads the latest changes from the given URL and stores them in the given file.
    /// </summary>
    /// <param name="outputFile">The file name for the download.</param>
    /// <param name="refreshInterval">The file refresh interval in minutes. If the last write time for the output file is younger that this, no new download will be attempted.</param>
    /// <param name="sourceUrl">The file to download.</param>
    /// <param name="sourceUrlUser">The user name to use for Basic Authentication for the download. If no user name is given, the download request will be sent without authentication headers.</param>
    /// <param name="sourceUrlPassword">The password to use for Basic Authentication for the download. If no password is given, the download request will be sent without authentication headers.</param>
    public static async Task GetLatestChanges(string outputFile, string sourceUrl, string sourceUrlUser, string sourceUrlPassword, int refreshInterval)
    {
        if (!File.Exists(outputFile) || File.GetLastWriteTimeUtc(outputFile) < DateTime.UtcNow.AddMinutes(0 - refreshInterval))
        {
            using HttpClientHandler handler = new();
            if (!string.IsNullOrEmpty(sourceUrlUser) && !string.IsNullOrEmpty(sourceUrlPassword))
            {
                handler.Credentials = new NetworkCredential(sourceUrlUser, sourceUrlPassword);
            }
            using HttpClient client = new(handler);
            using var responseStream = await client.GetStreamAsync(sourceUrl).ConfigureAwait(true);
            using var fileStream = new FileStream(outputFile, FileMode.Create);
            await responseStream.CopyToAsync(fileStream).ConfigureAwait(true);
            responseStream.Close();
        }
    }
}
