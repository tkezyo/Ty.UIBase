using Microsoft.Extensions.Hosting;

namespace Ty;

public static class UiBootstrap
{
    /// <summary>
    /// Builds the host via Module pipeline, exposes the ServiceProvider and starts background services.
    /// </summary>
    public static async Task<IHost> BuildAndStartHost<TModule>(string[] args, string? dependsOnFolder = null, bool skipVerification = true)
        where TModule : IModule, new()
    {
        var host = await ApplicationHostBuilder.CreateApplicationHost<TModule>(args, dependsOnFolder, skipVerification)
                   ?? throw new Exception("Failed to build application host");

        TyApp.ServiceProvider = host.Services;
        await host.StartAsync();
        return host;
    }
}