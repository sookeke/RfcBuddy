using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RfcBuddy.App.Objects;

namespace RfcBuddy.App.Services;

public interface IAppSettingsService
{
    public AppSettings AppSettings { get; }
}

public class AppSettingsService : IAppSettingsService
{
    //App setting variables
    private const string keyDataFolder = "DataFolder";
    private const string sectionSourceInfo = "SourceInfo";
    private const string keySourceUrl365 = "SourceUrl365";
    private const string keySourceUrlUser = "SourceUrlUser";
    private const string keySourceUrlPassword = "SourceUrlPassword";
    private const string keySourceRefreshMinutes = "SourceRefreshMinutes";

    private readonly AppSettings _appSettings;

    AppSettings IAppSettingsService.AppSettings => _appSettings;

    public AppSettingsService(ILogger<AppSettingsService> logger, IConfiguration config)
    {
        _appSettings = new()
        {
            DataFolder = config[keyDataFolder] ?? "./data",
        };
        if (string.IsNullOrWhiteSpace(config[sectionSourceInfo + ":" + keySourceUrl365]))
        {
            logger.LogCritical("Source URL for the 365-day change schedule not set in the appSettings.json file. This should be in under " + sectionSourceInfo + "/" + keySourceUrl365 + ".");
        }
        else
        {
            _appSettings.SourceUrl = config[sectionSourceInfo + ":" + keySourceUrl365]!;
            _appSettings.SourceUser = config[sectionSourceInfo + ":" + keySourceUrlUser] ?? string.Empty;
            _appSettings.SourcePassword = config[sectionSourceInfo + ":" + keySourceUrlPassword] ?? string.Empty;
        }
        if (int.TryParse(config[sectionSourceInfo + ":" + keySourceRefreshMinutes], out var refreshMinutes))
        {
            _appSettings.SourceRefreshMinutes = refreshMinutes;
        }
        else
        {
            logger.LogError("Refresh interval minutes not set in appSettings.json file or not an integer. This should be in under " + sectionSourceInfo + "/" + keySourceRefreshMinutes + ".");
        }
    }

}
