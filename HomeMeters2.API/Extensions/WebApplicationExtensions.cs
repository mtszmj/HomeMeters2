namespace HomeMeters2.API.Extensions;

public static class WebApplicationExtensions
{
    public const string EntryPointFromTestArg = "testProjectEntryPoint"; 

    public static bool IsIntegrationTest(this IHostApplicationBuilder builder)
    {
        return builder.Configuration.GetValue<bool>(EntryPointFromTestArg);
    }

    public static IWebHostBuilder FlagAsIntegrationTest(this IWebHostBuilder builder)
    {
        return builder.UseSetting(EntryPointFromTestArg, "true");
    }
}