﻿#region USING DIRECTIVES

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

using KioskApp.Common.Attributes;
using KioskApp.Database;
using KioskApp.Database.Models;
using KioskApp.Exceptions;
using KioskApp.Modules.Search.Services;

#endregion USING DIRECTIVES

namespace KioskApp.Modules.Search
{
    [Group("reddit"), Module(ModuleType.Searches), NotBlocked]
    [Description("Reddit commands. Group call prints hottest posts from given sub.")]
    [Aliases("r")]
    [UsageExamples("!reddit aww")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class RedditModule : AppModule
    {
        public RedditModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Orange;
        }

        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Subreddit.")] string sub = "all")
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Hot);

        #region COMMAND_RSS_REDDIT_CONTROVERSIAL

        [Command("controversial")]
        [Description("Get newest controversial posts for a subreddit.")]
        [UsageExamples("!reddit controversial aww")]
        public Task ControversialAsync(CommandContext ctx,
                                      [Description("Subreddit.")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Controversial);

        #endregion COMMAND_RSS_REDDIT_CONTROVERSIAL

        #region COMMAND_RSS_REDDIT_GILDED

        [Command("gilded")]
        [Description("Get newest gilded posts for a subreddit.")]
        [UsageExamples("!reddit gilded aww")]
        public Task GildedAsync(CommandContext ctx,
                               [Description("Subreddit.")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Gilded);

        #endregion COMMAND_RSS_REDDIT_GILDED

        #region COMMAND_RSS_REDDIT_HOT

        [Command("hot")]
        [Description("Get newest hot posts for a subreddit.")]
        [UsageExamples("!reddit hot aww")]
        public Task HotAsync(CommandContext ctx,
                            [Description("Subreddit.")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Hot);

        #endregion COMMAND_RSS_REDDIT_HOT

        #region COMMAND_RSS_REDDIT_NEW

        [Command("new")]
        [Description("Get newest posts for a subreddit.")]
        [Aliases("newest", "latest")]
        [UsageExamples("!reddit new aww")]
        public Task NewAsync(CommandContext ctx,
                            [Description("Subreddit.")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.New);

        #endregion COMMAND_RSS_REDDIT_NEW

        #region COMMAND_RSS_REDDIT_RISING

        [Command("rising")]
        [Description("Get newest rising posts for a subreddit.")]
        [UsageExamples("!reddit rising aww")]
        public Task RisingAsync(CommandContext ctx,
                               [Description("Subreddit.")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Rising);

        #endregion COMMAND_RSS_REDDIT_RISING

        #region COMMAND_RSS_REDDIT_TOP

        [Command("top")]
        [Description("Get top posts for a subreddit.")]
        [UsageExamples("!reddit top aww")]
        public Task TopAsync(CommandContext ctx,
                            [Description("Subreddit.")] string sub)
            => this.SearchAndSendResultsAsync(ctx, sub, RedditCategory.Top);

        #endregion COMMAND_RSS_REDDIT_TOP

        #region COMMAND_RSS_REDDIT_SUBSCRIBE

        [Command("subscribe")]
        [Description("Add new feed for a subreddit.")]
        [Aliases("add", "a", "+", "sub")]
        [UsageExamples("!reddit sub aww")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public Task SubscribeAsync(CommandContext ctx,
                                  [Description("Subreddit.")] string sub)
        {
            string command = $"sub r {sub}";
            Command cmd = ctx.CommandsNext.FindCommand(command, out string args);
            CommandContext fctx = ctx.CommandsNext.CreateFakeContext(ctx.Member, ctx.Channel, command, ctx.Prefix, cmd, args);
            return ctx.CommandsNext.ExecuteCommandAsync(fctx);
        }

        #endregion COMMAND_RSS_REDDIT_SUBSCRIBE

        #region COMMAND_RSS_REDDIT_UNSUBSCRIBE

        [Command("unsubscribe"), Priority(1)]
        [Description("Remove a subreddit feed using subreddit name or subscription ID (use command ``feed list`` to see IDs).")]
        [Aliases("del", "d", "rm", "-", "unsub")]
        [UsageExamples("!reddit unsub aww",
                       "!reddit unsub 12")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public Task UnsubscribeAsync(CommandContext ctx,
                                    [Description("Subreddit.")] string sub)
        {
            string command = $"unsub r {sub}";
            Command cmd = ctx.CommandsNext.FindCommand(command, out string args);
            CommandContext fctx = ctx.CommandsNext.CreateFakeContext(ctx.Member, ctx.Channel, command, ctx.Prefix, cmd, args);
            return ctx.CommandsNext.ExecuteCommandAsync(fctx);
        }

        [Command("unsubscribe"), Priority(0)]
        public async Task UnsubscribeAsync(CommandContext ctx,
                                          [Description("Subscription ID.")] int id)
        {
            using (DatabaseContext db = this.Database.CreateContext())
            {
                db.RssSubscriptions.Remove(new DatabaseRssSubscription() { ChannelId = ctx.Channel.Id, Id = id });
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Removed subscription with ID {Formatter.Bold(id.ToString())}", important: false);
        }

        #endregion COMMAND_RSS_REDDIT_UNSUBSCRIBE

        #region HELPER_FUNCTIONS

        private async Task SearchAndSendResultsAsync(CommandContext ctx, string sub, RedditCategory category)
        {
            string url = RedditService.GetFeedURLForSubreddit(sub, category, out string rsub);
            if (url is null)
                throw new CommandFailedException("That subreddit doesn't exist.");

            IReadOnlyList<SyndicationItem> res = RssService.GetFeedResults(url);
            if (res is null)
                throw new CommandFailedException($"Failed to get the data from that subreddit ({Formatter.Bold(rsub)}).");

            await RssService.SendFeedResultsAsync(ctx.Channel, res);
        }

        #endregion HELPER_FUNCTIONS
    }
}