using FormBE.Core.Services;
using FormBE.Persistence.Model;
using Microsoft.Extensions.DependencyInjection;

namespace FormBE.Core.Util;

public static class CoreSetup
{
    public static void ConfigureCore(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IFormService, FormService>();
        services.AddScoped<IFieldGroupService, FieldGroupService>();
        services.AddScoped<ISingleChoiceFieldService, SingleChoiceFieldService>();
        services.AddScoped<IOptionResponseService, OptionResponseService>();
        services.AddScoped<IFieldService, FieldService>();
        services.AddScoped<IFieldTypeService, FieldTypeService>();
        services.AddScoped<IFieldResponseService, FieldResponseService>();
    }
}
