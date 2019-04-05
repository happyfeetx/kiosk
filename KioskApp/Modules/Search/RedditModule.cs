#region USING DIRECTIVES

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
    [Description("Reddit commands. Group call prints hottest posts from given subreddit.")]
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
            => this.SearchAndSendResults(ctx, sub, RedditCategory.Hot);

        #region HELPER FUNCTIONS

        private async Task SearchAndSendResults(CommandContext ctx, string sub, RedditCategory category)
        {
            string url = RedditService.GetFeedURLForSubreddit(sub, category, out string rsub);
            if (url is null)
                throw new CommandFailedException("That subreddit doesn't exist.");

            IReadOnlyList<SyndicationItem> res = RssService.GetFeedResults(url);
            if (res is null)
                throw new CommandFailedException($"Failed to get the data from that subreddit ({Formatter.Bold(rsub)}).");

            await RssService.SendFeedResultsAsync(ctx.Channel, res);
        }

        #endregion HELPER FUNCTIONS
    }
}