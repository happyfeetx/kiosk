#region USING_DIRECTIVES

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Text.RegularExpressions;
using System.Threading.Tasks;

using KioskApp.Common.Attributes;
using KioskApp.Database;
using KioskApp.Exceptions;
using KioskApp.Modules.Search.Services;

#endregion USING_DIRECTIVES

namespace KioskApp.Modules.Search
{
    [Group("steam"), Module(ModuleType.Searches), NotBlocked]
    [Description("Steam commands. Group call searches steam profiles for a given ID.")]
    [Aliases("s", "st")]
    [UsageExamples("!steam profile 123456123")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class SteamModule : KServiceModule<SteamService>
    {
        public SteamModule(SteamService steam, SharedData shared, DatabaseContextBuilder db)
            : base(steam, shared, db)
        {
            this.ModuleColor = DiscordColor.Blue;
        }

        #region COMMAND_STEAM_PROFILE

        [Command("profile")]
        [Description("Get Steam user information for user based on his ID.")]
        [Aliases("id", "user")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("ID.")] ulong id)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            DiscordEmbed em = await this.Service.GetEmbeddedInfoAsync(id);
            if (em is null)
            {
                await this.InformFailureAsync(ctx, "User with such ID does not exist!");
                return;
            }

            await ctx.RespondAsync(embed: em);
        }

        #endregion COMMAND_STEAM_PROFILE
    }
}