using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RfcBuddy.Web.Services;

namespace RfcBuddy.Web.Controllers.Tests;

public class TestBase
{
    #region Initialization

    protected TestBase()
    {
    }
    #endregion

    #region Common Methods
    protected static ILogger<T> InitializeLogger<T>()
    {
        var serviceProvider = new ServiceCollection().AddLogging(builder => builder.AddDebug()).BuildServiceProvider();
        var factory = serviceProvider.GetService<ILoggerFactory>();
        return factory!.CreateLogger<T>();
    }

    protected static IAppSettingsService MockAppSettingsService()
    {
        var mockService=new Mock<IAppSettingsService>();
        return mockService.Object;
    }
    #endregion
}
