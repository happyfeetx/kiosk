#region USING_DIRECTIVES

using System;
using System.Threading.Tasks;
using KioskApp.Common;
using KioskApp.Database;
using KioskApp.Database.Models;

#endregion USING_DIRECTIVES

namespace KioskApp.Modules.Administration.Extensions
{
    public static class AppModuleGuildConfigExtensions
    {
        public static async Task<DatabaseGuildConfig> GetGuildConfigAsync(this AppModule module, ulong gid)
        {
            DatabaseGuildConfig gcfg = null;
            using (DatabaseContext db = module.Database.CreateContext())
                gcfg = await db.GuildConfig.FindAsync((long)gid) ?? new DatabaseGuildConfig();
            return gcfg;
        }

        public static async Task<DatabaseGuildConfig> ModifyGuildConfigAsync(this AppModule module,
            ulong gid, Action<DatabaseGuildConfig> action)
        {
            DatabaseGuildConfig gcfg = null;
            using (DatabaseContext db = module.Database.CreateContext())
            {
                gcfg = await db.GuildConfig.FindAsync((long)gid) ?? new DatabaseGuildConfig();
                action(gcfg);
                db.GuildConfig.Update(gcfg);
                await db.SaveChangesAsync();
            }

            CachedGuildConfig cgcfg = module.Shared.GetGuildConfig(gid);
            cgcfg = gcfg.CachedConfig;
            module.Shared.UpdateGuildConfig(gid, _ => cgcfg);

            return gcfg;
        }
    }
}