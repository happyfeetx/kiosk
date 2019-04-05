#region USING_DIRECTIVES

using System.Net.Http;

#endregion USING_DIRECTIVES

namespace KioskApp.Services
{
    public abstract class KioskHttpService : IKioskService
    {
        protected static HttpClientHandler _handler = new HttpClientHandler { AllowAutoRedirect = false };
        protected static HttpClient _http = new HttpClient(_handler, true);

        public abstract bool IsDisabled();
    }
}