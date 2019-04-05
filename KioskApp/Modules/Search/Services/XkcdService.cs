﻿#region USING_DIRECTIVES

using Newtonsoft.Json;

using System;
using System.Threading.Tasks;

using KioskApp.Common;
using KioskApp.Modules.Search.Common;
using KioskApp.Services;

#endregion USING_DIRECTIVES

namespace KioskApp.Modules.Search.Services
{
    public class XkcdService : KioskHttpService
    {
        public static readonly string XkcdUrl = "https://xkcd.com";
        public static readonly int TotalComics = 2040;

        public static string CreateUrlForComic(int id)
        {
            if (id < 1 || id > TotalComics)
                throw new ArgumentException("Comic ID is not valid (max 2019)", nameof(id));

            return $"{XkcdUrl}/{id}";
        }

        public static Task<XkcdComic> GetComicAsync(int? id = null)
        {
            if (id < 1 || id > TotalComics)
                throw new ArgumentException("Comic ID is not valid (max 2019)", nameof(id));

            try
            {
                return id.HasValue ? GetComicByIdAsync(id.Value) : GetLatestComicAsync();
            }
            catch
            {
                return null;
            }
        }

        public static Task<XkcdComic> GetRandomComicAsync()
            => GetComicByIdAsync(KRandom.Generator.Next(TotalComics));

        public override bool IsDisabled()
            => false;

        private static async Task<XkcdComic> GetComicByIdAsync(int id)
        {
            string response = await _http.GetStringAsync($"{XkcdUrl}/{id}/info.0.json").ConfigureAwait(false);
            return JsonConvert.DeserializeObject<XkcdComic>(response);
        }

        private static async Task<XkcdComic> GetLatestComicAsync()
        {
            string response = await _http.GetStringAsync($"{XkcdUrl}/info.0.json").ConfigureAwait(false);
            return JsonConvert.DeserializeObject<XkcdComic>(response);
        }
    }
}