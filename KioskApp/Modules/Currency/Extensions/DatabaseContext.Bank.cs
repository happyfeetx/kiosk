﻿using System;
using System.Threading.Tasks;

using KioskApp.Database;
using KioskApp.Database.Models;

namespace KioskApp.Modules.Currency.Extensions
{
    public static class DatabaseContextBankExtensions
    {
        public static async Task<bool> TryDecreaseBankAccountAsync(this DatabaseContext db, ulong uid, ulong gid, long amount)
        {
            BankAccounts account = await db.BankAccounts.FindAsync((long)gid, (long)uid);
            if (account is null || account.Balance < amount)
                return false;
            account.Balance -= amount;
            db.BankAccounts.Update(account);
            return true;
        }

        public static async Task ModifyBankAccountAsync(this DatabaseContext db, ulong uid, ulong gid, Func<long, long> balanceModifier)
        {
            bool created = false;

            BankAccounts account = await db.BankAccounts.FindAsync((long)gid, (long)uid);
            if (account is null)
            {
                account = new BankAccounts()
                {
                    GuildId = gid,
                    UserId = uid
                };
                created = true;
            }

            account.Balance = balanceModifier(account.Balance);
            if (account.Balance < 0)
                account.Balance = 0;

            if (created)
                db.BankAccounts.Add(account);
            else
                db.BankAccounts.Update(account);
        }
    }
}