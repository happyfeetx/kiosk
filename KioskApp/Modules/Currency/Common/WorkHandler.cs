﻿#region USING_DIRECTIVES

using DSharpPlus;

using System.Collections.Immutable;

using KioskApp.Common;

#endregion USING_DIRECTIVES

namespace KioskApp.Modules.Currency.Common
{
    public static class WorkHandler
    {
        private static ImmutableArray<string> _workAnswers = new string[] {
            "hard",
            "as a taxi driver",
            "in a grocery shop",
            "for the local mafia",
            "as bathroom cleaner"
        }.ToImmutableArray();

        private static ImmutableArray<string> _workStreetsPositiveAnswers = new string[] {
            "danced whole night",
            "made a good use of the party night",
            "got hired for a bachelor's party",
            "lurked around the singles bar"
        }.ToImmutableArray();

        private static ImmutableArray<string> _workStreetsNegativeAnswers = new string[] {
            "got robbed",
            "got ambushed by the local mafia",
            "got his wallet stolen",
            "drank too much - gambled away all the earnings"
        }.ToImmutableArray();

        private static ImmutableArray<string> _crimePositiveAnswers = new string[] {
            "successfully robbed the bank",
            "took up a hit-man contract",
            "stole some wallets",
        }.ToImmutableArray();

        public static string GetWorkString(int earned, string currency = null)
            => $"worked {KRandom.Generator.ChooseRandomElement(_workAnswers)} and earned {Formatter.Bold(earned.ToString())} {currency ?? "credits"}!";

        public static string GetWorkStreetsString(int change, string currency = null)
        {
            if (change > 0)
                return $"{KRandom.Generator.ChooseRandomElement(_workStreetsPositiveAnswers)} and earned {Formatter.Bold(change.ToString())} {currency ?? "credits"}!";
            else if (change < 0)
                return $"{KRandom.Generator.ChooseRandomElement(_workStreetsNegativeAnswers)} and lost {Formatter.Bold((-change).ToString())} {currency ?? "credits"}!";
            else
                return "had no luck and got nothing this evening!";
        }

        public static string GetCrimeString(int change, string currency = null)
        {
            if (change > 0)
                return $"{KRandom.Generator.ChooseRandomElement(_crimePositiveAnswers)} and got away with {Formatter.Bold(change.ToString())} {currency ?? "credits"}!";
            else if (change < 0)
                return $"got caught and got bailed out of jail for {Formatter.Bold((-change).ToString())} {currency ?? "credits"}!";
            else
                return "had no luck and got nothing this evening!";
        }
    }
}