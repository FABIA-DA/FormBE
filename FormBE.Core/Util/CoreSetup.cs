using Microsoft.Extensions.DependencyInjection;

namespace FormBE.Core.Util;

public static class CoreSetup
{
    public static void ConfigureCore(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
    }
}
