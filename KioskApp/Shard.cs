﻿#region USING_DIRECTIVES

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.VoiceNext;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using KioskApp.Common;
using KioskApp.Database;
using KioskApp.Common.Converters;
using KioskApp.Extensions;
using KioskApp.Modules.Search.Services;
using KioskApp.Modules.Administration.Services;

#endregion USING_DIRECTIVES

namespace KioskApp
{
    public sealed class KioskAppShard
    {
        public static IReadOnlyList<(string Name, Command Cmd)> Commands;

        public static void UpdateCommandList(CommandsNextExtension cnext)
        {
            Commands = cnext.GetAllRegisteredCommands()
                .Where(cmd => cmd.Parent is null)
                .SelectMany(cmd => cmd.Aliases.Select(alias => (alias, cmd)).Concat(new[] { (cmd.Name, cmd) }))
                .ToList()
                .AsReadOnly();
        }

        public int Id { get; }
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension CNext { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public VoiceNextExtension Voice { get; private set; }
        public SharedData SharedData { get; private set; }
        public DatabaseContextBuilder Database { get; private set; }

        public bool IsListening => this.SharedData.ListeningStatus;

        public KioskAppShard(int sid, DatabaseContextBuilder dbb, SharedData shared)
        {
            this.Id = sid;
            this.Database = dbb;
            this.SharedData = shared;
        }

        public async Task StartAsync()
        {
            await this.Client.ConnectAsync();
        }

        public async Task DisposeAsync()
        {
            await this.Client.DisconnectAsync();
            this.Client.Dispose();
        }

        public void Log(LogLevel level, string message)
            => this.SharedData.LogProvider.Log(level, message, this.Id, DateTime.Now);

        public void LogMany(LogLevel level, params string[] messages)
            => this.SharedData.LogProvider.LogMany(level, this.Id, DateTime.Now, this.SharedData.LogProvider.LogToFile, messages);

        #region SETUP FUNCTIONS

        public void Initialize(AsyncEventHandler<GuildDownloadCompletedEventArgs> onGuildDownloadCompleted)
        {
            this.SetupClient(onGuildDownloadCompleted);
            this.SetupCommands();
            this.SetupInteractivity();
            this.SetupVoice();

            AsyncExecutionManager.RegisterEventListeners(this.Client, this);
        }

        private void SetupClient(AsyncEventHandler<GuildDownloadCompletedEventArgs> onGuildDownloadCompleted)
        {
            var cfg = new DiscordConfiguration
            {
                Token = this.SharedData.BotConfiguration.DiscordToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LargeThreshold = 256,
                ShardCount = this.SharedData.BotConfiguration.ShardCount,
                ShardId = this.Id,
                UseInternalLogHandler = false,
                LogLevel = this.SharedData.BotConfiguration.LogLevel
            };

            this.Client = new DiscordClient(cfg);

            this.Client.DebugLogger.LogMessageReceived += (s, e) =>
            {
                this.SharedData.LogProvider.Log(this.Id, e);
            };
            this.Client.Ready += e =>
            {
                this.SharedData.LogProvider.ElevatedLog(LogLevel.Info, "Ready!", this.Id);
                return Task.CompletedTask;
            };
            this.Client.GuildDownloadCompleted += onGuildDownloadCompleted;
        }

        private void SetupCommands()
        {
            var cfg = new CommandsNextConfiguration
            {
                EnableDms = false,
                CaseSensitive = false,
                EnableMentionPrefix = true,
                PrefixResolver = this.PrefixResolverAsync,
                Services = new ServiceCollection()
                    .AddSingleton(this)
                    .AddSingleton(this.SharedData)
                    .AddSingleton(this.Database)
                    .AddSingleton(new AntifloodService(this))
                    .AddSingleton(new AntiInstantLeaveService(this))
                    .AddSingleton(new AntispamService(this))
                    .AddSingleton(new GiphyService(this.SharedData.BotConfiguration.GiphyKey))
                    .AddSingleton(new GoodreadsService(this.SharedData.BotConfiguration.GoodreadsKey))
                    .AddSingleton(new ImgurService(this.SharedData.BotConfiguration.ImgurKey))
                    .AddSingleton(new LinkfilterService(this))
                    .AddSingleton(new OMDbService(this.SharedData.BotConfiguration.OMDbKey))
                    .AddSingleton(new RatelimitService(this))
                    .AddSingleton(new SteamService(this.SharedData.BotConfiguration.SteamKey))
                    .AddSingleton(new WeatherService(this.SharedData.BotConfiguration.WeatherKey))
                    .AddSingleton(new YtService(this.SharedData.BotConfiguration.YouTubeKey))
                    .BuildServiceProvider()
            };
            this.CNext = this.Client.UseCommandsNext(cfg);

            this.CNext.SetHelpFormatter<CustomHelpFormatter>();

            this.CNext.RegisterCommands(Assembly.GetExecutingAssembly());

            this.CNext.RegisterConverter(new CustomBoolConverter());
            this.CNext.RegisterConverter(new CustomActivityTypeConverter());
            this.CNext.RegisterConverter(new CustomTimeWindowConverter());
            this.CNext.RegisterConverter(new CustomIPAddressConverter());
            this.CNext.RegisterConverter(new CustomIPFormatConverter());
            this.CNext.RegisterConverter(new CustomPunishmentActionTypeConverter());

            UpdateCommandList(this.CNext);
        }

        private void SetupInteractivity()
        {
            var cfg = new InteractivityConfiguration()
            {
                PaginationBehavior = TimeoutBehaviour.Ignore,
                PaginationTimeout = TimeSpan.FromMinutes(1),
                Timeout = TimeSpan.FromMinutes(1)
            };

            this.Interactivity = this.Client.UseInteractivity(cfg);
        }

        private void SetupVoice()
        {
            var cfg = new VoiceNextConfiguration()
            {
                AudioFormat = AudioFormat.Default
            };

            this.Voice = this.Client.UseVoiceNext(cfg);
        }

        private Task<int> PrefixResolverAsync(DiscordMessage m)
        {
            string p = this.SharedData.GetGuildPrefix(m.Channel.Guild.Id) ?? this.SharedData.BotConfiguration.DefaultPrefix;
            return Task.FromResult(m.GetStringPrefixLength(p));
        }

        #endregion SETUP FUNCTIONS
    }
}