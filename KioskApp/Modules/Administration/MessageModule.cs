﻿#region USING DIRECTIVES

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using KioskApp.Common.Attributes;
using KioskApp.Exceptions;
using KioskApp.Extensions;
using KioskApp.Database;

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

#endregion USING DIRECTIVES

namespace KioskApp.Modules.Administration
{
    [Group("message"), Module(ModuleType.Administration), NotBlocked]
    [Description("Commands for manipulating messages.")]
    [Aliases("m", "msg", "msgs", "messages")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class MessageModule : AppModule
    {
        public MessageModule(SharedData shared, DatabaseContextBuilder dbb)
            : base(shared, dbb)
        {
            this.ModuleColor = DiscordColor.Azure;
        }

        #region COMMAND_MESSAGES_MODIFY

        [Command("modify")]
        [Description("Modify the given message.")]
        [Aliases("edit", "mod", "e", "m")]
        [UsageExamples("!messages modify 408226948855234561 modified text")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task ModifyMessageAsync(CommandContext ctx,
                                            [Description("Message.")] DiscordMessage message,
                                            [RemainingText, Description("New content.")] string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new CommandFailedException("Missing new message content!");

            await message.ModifyAsync(content);
            await this.InformAsync(ctx, important: false);
        }

        #endregion COMMAND_MESSAGES_MODIFY

        #region COMMAND_MESSAGES_PIN

        [Command("pin")]
        [Description("Pins the message given by ID. If the message is not provided, pins the last sent message before command invocation.")]
        [Aliases("p")]
        [UsageExamples("!messages pin",
                       "!messages pin 408226948855234561")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task PinMessageAsync(CommandContext ctx,
                                         [Description("Message.")] DiscordMessage message = null)
        {
            message = message ?? (await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1))?.FirstOrDefault();

            if (message is null)
                throw new CommandFailedException("Cannot retrieve the message!");

            await message.PinAsync();
        }

        #endregion COMMAND_MESSAGES_PIN

        #region COMMAND_MESSAGES_UNPIN

        [Command("unpin"), Priority(1)]
        [Description("Unpins the message at given index (starting from 1) or message ID. If the index is not given, unpins the most recent one.")]
        [Aliases("up")]
        [UsageExamples("!messages unpin 12345645687955",
                       "!messages unpin 10")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task UnpinMessageAsync(CommandContext ctx,
                                           [Description("Message.")] DiscordMessage message)
        {
            await message.UnpinAsync();
            await this.InformAsync(ctx, "Removed the specified pin.", important: false);
        }

        [Command("unpin"), Priority(0)]
        public async Task UnpinMessageAsync(CommandContext ctx,
                                           [Description("Index (starting from 1).")] int index = 1)
        {
            IReadOnlyList<DiscordMessage> pinned = await ctx.Channel.GetPinnedMessagesAsync();

            if (index < 1 || index > pinned.Count)
                throw new CommandFailedException($"Invalid index (must be in range [1, {pinned.Count}]!");

            await pinned.ElementAt(index - 1).UnpinAsync();
            await this.InformAsync(ctx, "Removed the specified pin.", important: false);
        }

        #endregion COMMAND_MESSAGES_UNPIN

        #region COMMAND_MESSAGES_UNPINALL

        [Command("unpinall")]
        [Description("Unpins all pinned messages in this channel.")]
        [Aliases("upa")]
        [UsageExamples("!messages unpinall")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task UnpinAllMessagesAsync(CommandContext ctx)
        {
            IReadOnlyList<DiscordMessage> pinned = await ctx.Channel.GetPinnedMessagesAsync();

            int failed = 0;
            foreach (DiscordMessage m in pinned)
            {
                try
                {
                    await m.UnpinAsync();
                }
                catch
                {
                    failed++;
                }
            }

            if (failed > 0)
                await this.InformFailureAsync(ctx, $"Failed to unpin {failed} messages!");
            else
                await this.InformAsync(ctx, "Successfully unpinned all messages in this channel", important: false);
        }

        #endregion COMMAND_MESSAGES_UNPINALL

        #region COMMAND_MESSAGES_LISTPINNED

        [Command("listpinned")]
        [Description("List pinned messages in this channel.")]
        [Aliases("lp", "listpins", "listpin", "pinned")]
        [UsageExamples("!messages listpinned")]
        public async Task ListPinnedMessagesAsync(CommandContext ctx)
        {
            IReadOnlyList<DiscordMessage> pinned = await ctx.Channel.GetPinnedMessagesAsync();

            if (!pinned.Any())
            {
                await this.InformFailureAsync(ctx, "No pinned messages in this channel");
                return;
            }

            var pages = pinned.Select(m => new Page()
            {
                Content = $"Author: {Formatter.Bold(m.Author.Username)} {m.CreationTimestamp.ToUtcTimestamp()}",
                Embed = m.Embeds.FirstOrDefault() ?? new DiscordEmbedBuilder()
                {
                    Title = "Jump to",
                    Description = m.Content ?? Formatter.Italic("Empty message."),
                    Url = m.JumpLink.ToString()
                }.Build()
            }).ToList();

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages);
        }

        #endregion COMMAND_MESSAGES_LISTPINNED
    }
}