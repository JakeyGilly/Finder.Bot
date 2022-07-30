namespace Finder.Bot.Handlers {
    public class Appsettings {
        public string token { get; set; } = "paste token here";
        public ulong testGuild { get; set; } = 0;
        public ConnectionStrings ConnectionStrings { get; set; } = new ConnectionStrings();
    }

    public class ConnectionStrings {
        public string Default { get; set; } = "Server=localhost;Database=finder;Username=finder;Password=enter database password;";
    }
}