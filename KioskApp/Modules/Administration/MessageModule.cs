#region USING DIRECTIVES

using KioskApp.Common;
using KioskApp.Common.Attributes;
using KioskApp.Database;
using KioskApp.Exceptions;
using KioskApp.Extensions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using System;
using System.Threading.Tasks;

#endregion USING DIRECTIVES

namespace KioskApp.Modules.Administration
{
    [Group("message"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("m", "msg")]
    [RequireOwnerOrPermissions(Permissions.ManageMessages)]
    [Description("Utility commands for text-channels.")]
    public class MessageModule : AppModule
    {
        public MessageModule(SharedData shared, DatabaseContextBuilder dbb) 
            : base(shared, dbb)
        {
            this.ModuleColor = DiscordColor.Green;
        }

        [Command("pin")]
        public async Task PinMessageAsync(CommandContext ctx )
        {

        }
    }
}
