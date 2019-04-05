#region USING DIRECTIVES

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

using KioskApp.Common;

using Newtonsoft.Json;

#endregion USING DIRECTIVES

namespace KioskApp {

    internal static class KioskApp {

        // Strings <<
        public static readonly string ApplicationName = "KioskApp";

        public static readonly string ApplicationVersion = "V1.0_TEST_BUILD";
        // Strings >>

        private static BotConfig BotConfiguration { get; set; }

        private static async Task Main(string[] args) {
            try {
                PrintBuildInformation();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                await LoadBotConfigAsync();

                try {
                } catch (TaskCanceledException) {
                }
            } catch (Exception e) {
                Console.WriteLine($"\nException occured: {e.GetType()} :\n{e.Message}");
                if (!(e.InnerException is null))
                    Console.WriteLine($"Inner exception: {e.InnerException.GetType()} :\n{e.InnerException.Message}");
                Console.ReadKey();
            }
            Console.WriteLine("\nPowering off...");
        }

        private static void PrintBuildInformation() {
            var a = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(a.Location);

            Console.WriteLine($"{ApplicationName} {ApplicationVersion} ({fvi.FileVersion})");
            Console.WriteLine();
        }

        private static async Task LoadBotConfigAsync() {
            Console.WriteLine("\r[1] Loading configuration...");

            string json = "{}";
            var utf8 = new UTF8Encoding(false);
            var fi = new FileInfo("Resources/config.json");
            if (!fi.Exists) {
                Console.WriteLine("\rLoading configuration failed!");

                json = JsonConvert.SerializeObject(BotConfig.Default, Formatting.Indented);
                using (FileStream fs = fi.Create())
                using (var sw = new StreamWriter(fs, utf8)) {
                    await sw.WriteAsync(json);
                    await sw.FlushAsync();
                }

                Console.WriteLine("New configuration file has been created at:");
                Console.WriteLine(fi.FullName);
                Console.WriteLine("Please fill it in with appropriate values and re-run the program.");

                throw new IOException("Configuration file not found!");
            }

            using (FileStream fs = fi.OpenRead())
            using (var sr = new StreamReader(fs, utf8)) {
                json = await sr.ReadToEndAsync();
            }

            BotConfiguration = JsonConvert.DeserializeObject<BotConfig>(json);
        }
    }
}