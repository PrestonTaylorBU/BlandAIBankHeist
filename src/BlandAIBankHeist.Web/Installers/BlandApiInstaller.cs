using BlandAIBankHeist.Web.Services;

namespace BlandAIBankHeist.Web.Installers;

public static class BlandApiInstaller
{
    static public IServiceCollection AddBlandApiServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IBlandApiService, BlandApiService>();

        serviceCollection.AddHttpClient();

        return serviceCollection;
    }
}
