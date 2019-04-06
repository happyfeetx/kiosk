﻿#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using KioskApp.Common.Attributes;
using KioskApp.Database;
using KioskApp.Database.Models;
using KioskApp.Exceptions;
using KioskApp.Extensions;
#endregion

namespace KioskApp.Modules.Misc
{
    [Group("birthdays"), Module(ModuleType.Miscellaneous), NotBlocked]
    [Description("Birthday notifications commands. Group call either lists or adds birthday depending if argument is given.")]
    [Aliases("birthday", "bday", "bd", "bdays")]
    [UsageExamples("!birthdays",
                   "!birthday add @Someone #channel_to_send_message_to",
                   "!birthday add @Someone 15.2.1990 #channel_to_send_message_to")]
    [RequireUserPermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class BirthdayModule : AppModule
    {

        public BirthdayModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Wheat;
        }


        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Birthday boy/girl.")] DiscordUser user,
                                     [Description("Channel to send a greeting message to.")] DiscordChannel channel = null,
                                     [Description("Birth date.")] string date = null)
            => this.AddAsync(ctx, user, date, channel);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Birthday boy/girl.")] DiscordUser user,
                                     [Description("Birth date.")] string date = null,
                                     [Description("Channel to send a greeting message to.")] DiscordChannel channel = null)
            => this.AddAsync(ctx, user, date, channel);


        #region COMMAND_BIRTHDAY_ADD
        [Command("add"), Priority(0)]
        [Description("Schedule a birthday notification. If the date is not specified, uses the current date as a birthday date. If the channel is not specified, uses the current channel.")]
        [Aliases("new", "+", "a", "+=", "<", "<<")]
        [UsageExamples("!birthday add @Someone",
                       "!birthday add @Someone #channel_to_send_message_to",
                       "!birthday add @Someone 15.2.1990",
                       "!birthday add @Someone #channel_to_send_message_to 15.2.1990",
                       "!birthday add @Someone 15.2.1990 #channel_to_send_message_to")]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Birthday boy/girl.")] DiscordUser user,
                                  [Description("Birth date.")] string date_str = null,
                                  [Description("Channel to send a greeting message to.")] DiscordChannel channel = null)
        {
            var date = DateTime.UtcNow.Date;
            if (!string.IsNullOrWhiteSpace(date_str) && !DateTime.TryParse(date_str, out date))
                throw new CommandFailedException("The given date is not valid!");

            channel = channel ?? ctx.Channel;

            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException("I can only send birthday notifications in a text channel.");

            using (DatabaseContext db = this.Database.CreateContext())
            {
                db.Birthdays.Add(new DatabaseBirthday()
                {
                    ChannelId = channel.Id,
                    Date = date,
                    GuildId = ctx.Guild.Id,
                    LastUpdateYear = DateTime.Now.Year,
                    UserId = user.Id
                });

                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Added a new birthday in channel {Formatter.Bold(channel.Name)} for {Formatter.Bold(user.Username)}", important: false);
        }

        [Command("add"), Priority(1)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Birthday boy/girl.")] DiscordUser user,
                            [Description("Channel to send a greeting message to.")] DiscordChannel channel = null,
                            [Description("Birth date.")] string date_str = null)
            => this.AddAsync(ctx, user, date_str, channel);
        #endregion

        #region COMMAND_BIRTHDAY_DELETE
        [Command("delete"), Priority(1), UsesInteractivity]
        [Description("Remove status from running queue.")]
        [Aliases("-", "remove", "rm", "del", "-=", ">", ">>")]
        [UsageExamples("!birthday delete @Someone")]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("User whose birthday to remove.")] DiscordUser user)
        {
            using (DatabaseContext db = this.Database.CreateContext())
            {
                db.Birthdays.RemoveRange(db.Birthdays.Where(b => b.GuildId == ctx.Guild.Id && b.UserId == user.Id));
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Removed birthday for {Formatter.Bold(user.Username)}", important: false);
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Channel for which to remove birthdays.")] DiscordChannel channel)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all birthdays in this channel?"))
                return;

            using (DatabaseContext db = this.Database.CreateContext())
            {
                db.Birthdays.RemoveRange(db.Birthdays.Where(b => b.GuildId == ctx.Guild.Id && b.ChannelId == channel.Id));
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Removed birthday notifications in channel {Formatter.Bold(channel.Mention)}", important: false);
        }
        #endregion

        #region COMMAND_BIRTHDAY_LIST
        [Command("list")]
        [Description("List registered birthday notifications for this channel.")]
        [Aliases("ls")]
        [UsageExamples("!birthday list")]
        public async Task ListAsync(CommandContext ctx,
                                   [Description("Channel for which to list.")] DiscordChannel channel = null)
        {
            channel = channel ?? ctx.Channel;

            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException("Birthday notifications are only posted in text channels");

            List<DatabaseBirthday> birthdays;
            using (DatabaseContext db = this.Database.CreateContext())
            {
                birthdays = await db.Birthdays
                    .Where(b => b.GuildId == ctx.Guild.Id && b.ChannelId == channel.Id)
                    .ToListAsync();
            }

            if (!birthdays.Any())
                throw new CommandFailedException("No birthdays registered!");

            var lines = new List<string>();
            foreach (DatabaseBirthday birthday in birthdays)
            {
                try
                {
                    DiscordUser user = await ctx.Client.GetUserAsync(birthday.UserId);
                    lines.Add($"{Formatter.InlineCode(birthday.Date.ToShortDateString())} | {Formatter.Bold(user.Username)} | {channel.Name}");
                }
                catch
                {
                    using (DatabaseContext db = this.Database.CreateContext())
                    {
                        db.Birthdays.RemoveRange(db.Birthdays.Where(b => b.UserId == birthday.UserId));
                        await db.SaveChangesAsync();
                    }
                }
            }

            await ctx.SendCollectionInPagesAsync(
                $"Birthdays registered in channel {ctx.Channel.Name}:",
                lines,
                line => line,
                this.ModuleColor,
                5
            );
        }
        #endregion

        #region COMMAND_BIRTHDAY_LISTALL
        [Command("listall")]
        [Description("List all registered birthdays.")]
        [Aliases("lsa")]
        [UsageExamples("!birthday listall")]
        [RequirePrivilegedUser]
        public async Task ListAllAsync(CommandContext ctx)
        {
            List<DatabaseBirthday> birthdays;
            using (DatabaseContext db = this.Database.CreateContext())
            {
                birthdays = await db.Birthdays
                    .Where(b => b.GuildId == ctx.Guild.Id)
                    .ToListAsync();
            }

            if (!birthdays.Any())
                throw new CommandFailedException("No birthdays registered!");

            var lines = new List<string>();
            foreach (DatabaseBirthday birthday in birthdays)
            {
                try
                {
                    DiscordChannel channel = await ctx.Client.GetChannelAsync(birthday.ChannelId);
                    DiscordUser user = await ctx.Client.GetUserAsync(birthday.UserId);
                    lines.Add($"{Formatter.InlineCode(birthday.Date.ToShortDateString())} | {Formatter.Bold(user.Username)} | {channel.Name}");
                }
                catch
                {
                    using (DatabaseContext db = this.Database.CreateContext())
                    {
                        db.Birthdays.RemoveRange(db.Birthdays.Where(b => b.UserId == birthday.UserId));
                        await db.SaveChangesAsync();
                    }
                }
            }

            await ctx.SendCollectionInPagesAsync(
                "Birthdays:",
                lines,
                line => line,
                this.ModuleColor,
                5
            );
        }
        #endregion
    }
}
