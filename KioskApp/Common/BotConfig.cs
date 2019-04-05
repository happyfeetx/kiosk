#region USING DIRECTIVES

using Newtonsoft.Json;

using DSharpPlus;

using System;
using System.IO;
using System.Collections.Generic;

using KioskApp.Database;

#endregion USING DIRECTIVES

namespace KioskApp.Common
{
    public sealed class BotConfig
    {
        [JsonProperty("db-config")]
        public DatabaseConfig DatabaseConfig { get; private set; }

        [JsonProperty("db_sync_interval")]
        public int DatabaseSyncInterval { get; private set; }

        [JsonProperty("prefix")]
        public string DefaultPrefix { get; private set; }

        [JsonProperty("feed_check_interval")]
        public int FeedCheckInterval { get; private set; }

        [JsonProperty("feed_check_start_delay")]
        public int FeedCheckStartDelay { get; private set; }

        [JsonProperty("key-giphy")]
        public string GiphyKey { get; private set; }

        [JsonProperty("key-goodreads")]
        public string GoodreadsKey { get; private set; }

        [JsonProperty("key-imgur")]
        public string ImgurKey { get; private set; }

        [JsonProperty("log-level")]
        public LogLevel LogLevel { get; private set; }

        [JsonProperty("log-path")]
        public string LogPath { get; private set; }

        [JsonProperty("log-to-file")]
        public bool LogToFile { get; private set; }

        [JsonProperty("key-omdb")]
        public string OMDbKey { get; private set; }

        [JsonProperty("shard-count")]
        public int ShardCount { get; private set; }

        [JsonProperty("key-steam")]
        public string SteamKey { get; private set; }

        [JsonProperty("key-weather")]
        public string WeatherKey { get; private set; }

        [JsonProperty("key-youtube")]
        public string YouTubeKey { get; private set; }

        [JsonProperty("token")]
        public string DiscordToken { get; private set; }

        [JsonProperty("logger-special-rules")]
        public List<Logger.SpecialLoggingRule> SpecialLoggerRules { get; private set; }

        [JsonIgnore]
        public static BotConfig Default => new BotConfig
        {
            DatabaseConfig = DatabaseConfig.Default,
            DefaultPrefix = "!",
            DiscordToken = "<Discord API Token here>",
            LogLevel = LogLevel.Info,
            LogPath = "Logs/log.txt",
            LogToFile = false,
            ShardCount = 1,
            SteamKey = "<Steam API Key here>",
            WeatherKey = "<OpenWeatherMaps API Key here>",
            YouTubeKey = "<YouTube API Key here>"
        };
    }
}