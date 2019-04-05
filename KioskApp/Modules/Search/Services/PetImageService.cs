#region USING_DIRECTIVES

using Newtonsoft.Json.Linq;

using System.Threading.Tasks;

using KioskApp.Services;

#endregion USING_DIRECTIVES

namespace KioskApp.Modules.Search.Services
{
    public class PetImagesService : KioskHttpService
    {
        public override bool IsDisabled()
            => false;

        public static async Task<string> GetRandomCatImageAsync()
        {
            string data = await _http.GetStringAsync("http://aws.random.cat/meow").ConfigureAwait(false);
            return JObject.Parse(data)["file"].ToString();
        }

        public static async Task<string> GetRandomDogImageAsync()
        {
            string data = await _http.GetStringAsync("https://random.dog/woof").ConfigureAwait(false);
            return "https://random.dog/" + data;
        }
    }
}