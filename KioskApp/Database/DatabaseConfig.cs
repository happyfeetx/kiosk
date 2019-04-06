#region USING DIRECTIVES

using Newtonsoft.Json;
using static KioskApp.Database.DatabaseContextBuilder;

#endregion USING DIRECTIVES

namespace KioskApp.Database
{
    public sealed class DatabaseConfig
    {
        [JsonProperty("database")]
        public string DatabaseName { get; set; }

        [JsonProperty("provider")]
        public DatabaseProvider Provider { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        public static DatabaseConfig Default => new DatabaseConfig
        {
            DatabaseName = "kdb",
            Provider = DatabaseProvider.SQLite,
            Hostname = "localhost",
            Password = "",
            Port = 8844,
            Username = ""
        };
    }
}