namespace HomeMeters2.API.Logging;

public static class LogIds
{
    public static readonly EventId AppStartup = new(1, "AppStartup");
    public static readonly EventId AppClose = new(2, "AppClose");


    public static class Place
    {
        public static readonly EventId ControllerException = new(10001, "PlaceControllerException");
    }
}