using System.Reflection;
using System.Runtime.CompilerServices;
using HomeMeters2.API.Constants;
using HomeMeters2.API.DataAccess;
using HomeMeters2.API.Extensions;
using HomeMeters2.API.Logging;
using HomeMeters2.API.Services.PublicIds;
using HomeMeters2.API.Users;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

[assembly: InternalsVisibleTo("HomeMeters2.API.Tests")]

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Add services to the container.
    builder.Services.AddOptions<SqidsConfiguration>()
        .BindConfiguration("Sqids")
        .ValidateDataAnnotations()
        .ValidateOnStart();
    builder.Services.AddScoped<PublicIdGenerator>();
    
    UseSerilog(builder);

    builder.Services.AddAuthentication();
    
    builder.Services.AddAuthorization();
    builder.Services
        .AddIdentityApiEndpoints<User>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        ;
    
    builder.Services.AddScoped<TokenService>();

    builder.Services.AddHealthChecks();
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(o => o.InferSecuritySchemes());
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer("name=ConnectionStrings:DefaultConnection")
            .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
            .EnableDetailedErrors(builder.Environment.IsDevelopment())
            .LogTo(Log.Logger.Debug, LogLevel.Information)
        );
    builder.Services.AddAutoMapper(typeof(Program));

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    UseSerilogRequestLogging(app);
    app.MapHealthChecks("/api/healthcheck");

    app.UseHttpsRedirection();
    
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();


    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(LogIds.AppStartup, "Application startup");
    app.Lifetime.ApplicationStopping.Register(() => logger.LogWarning(LogIds.AppClose, "Application close"));

    app.MapGroup(UsersConstants.EndpointPath).WithTags(UsersConstants.Tag).MapIdentityApi<User>();
    
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
    if (IsEntityFrameworkMigration())
    {
        return;
    }
    
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

bool IsEntityFrameworkMigration()
{
    return Assembly.GetEntryAssembly()?.GetName()?.Name?.Equals("ef", StringComparison.OrdinalIgnoreCase) ?? false;
}