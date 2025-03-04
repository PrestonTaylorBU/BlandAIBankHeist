using Serilog;

namespace BlandAIBankHeist.Web.Installers;

public static class SerilogInstaller
{
    static public IHostBuilder UseSerilogWithConfiguration(this IHostBuilder host, IConfiguration configuration)
    {
        host.UseSerilog((_, config) =>
        {
            config.ReadFrom.Configuration(configuration);
        });

        return host;
    }
}
