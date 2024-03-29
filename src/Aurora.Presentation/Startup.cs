using Aurora.Application;
using Aurora.Infrastructure;
using Aurora.Infrastructure.Services;
using Aurora.Presentation.Extensions;
using Aurora.Presentation.Services;
using Aurora.Scrapers;
using Aurora.Shared.Models;
using Hangfire;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Aurora.Presentation;

public class Startup
{
    private static readonly string CorsPolicyName = "AllowAll";
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        //TODO: Reuse those assemblies in other discovery classes
        var auroraAssemblies = Assembly.GetExecutingAssembly()
            .GetReferencedAssemblies()
            .Where(assembly => assembly.Name is not null && assembly.Name.StartsWith(nameof(Aurora)))
            .Select(name => Assembly.Load(name))
            .ToArray();
        services
            .BindConfigSections(Configuration, auroraAssemblies)
            .AddApplication()
            .AddInfrastructure(Configuration)
            .AddScrapers();

        services.AddControllers()
            .AddJsonOptions(x =>
            {
                x.JsonSerializerOptions.Converters.Add(new OneOfJsonConverterFactory());
            });

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.AllowAnyOrigin();
            });
        });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Aurora", Version = "v1" });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseDeveloperExceptionPage();
        app.UseCors(CorsPolicyName);
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aurora v1"));

        app.UseAuthentication();

        app.UseHangfireDashboard(options: new DashboardOptions()
        {
            IgnoreAntiforgeryToken = true,
            Authorization = new[] { new HangfireAuthorization() }
        });

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHub<NotificationHub>("hub/notifications");
        });
    }
}
