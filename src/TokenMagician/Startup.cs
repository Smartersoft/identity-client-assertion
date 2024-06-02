using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Svrooij.PowerShell.DependencyInjection;
using Svrooij.PowerShell.DependencyInjection.Logging;
namespace TokenMagician;

public class Startup : PsStartup
{
    override public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<TokenCredential, DefaultAzureCredential>();
    }
    public override Action<PowerShellLoggerConfiguration>? ConfigurePowerShellLogging()
    {
        return builder =>
        {
            builder.DefaultLevel = LogLevel.Information;
            builder.LogLevel.Add("System.Net.Http.HttpClient", LogLevel.Warning);
            builder.IncludeCategory = true;
            builder.StripNamespace = true;
        };
    }
}
