using System.Reflection;
using System.Runtime.CompilerServices;
using HomeMeters2.API.Extensions;
using HomeMeters2.API.Logging;
using HomeMeters2.API.Places;
using Serilog;
using Serilog.Events;

[assembly: InternalsVisibleTo("HomeMeters2.API.Tests")]

try
{
    var builder = WebApplication.CreateBuilder(args);

    
    // Add services to the container.
    builder.Services.AddSingleton<InMemoryTestData>();
    
    UseSerilog(builder);

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    UseSerilogRequestLogging(app);
    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(LogIds.AppStartup, "Application startup");
    app.Lifetime.ApplicationStopping.Register(() => logger.LogWarning(LogIds.AppClose, "Application close"));
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

void UseSerilog(WebApplicationBuilder webApplicationBuilder)
{
    if (webApplicationBuilder.IsIntegrationTest())
    {
        webApplicationBuilder.Host.UseSerilog((context, configuration) =>
            configuration.WriteTo.Console().MinimumLevel.Debug());
        return;
    }
    
    var directoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
    if (directoryName is null)
        throw new InvalidOperationException("Cannot get directory name for assembly");
    
    var loggerSettingsConfiguration = new ConfigurationBuilder()
        .SetBasePath(directoryName)
        .AddJsonFile("loggerSettings.json", false, true)
        .Build();

    webApplicationBuilder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(loggerSettingsConfiguration));
}

void UseSerilogRequestLogging(WebApplication webApplication)
{
    webApplication.UseSerilogRequestLogging(options =>
    {
        options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        };
    });
}