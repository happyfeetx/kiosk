#region USING DIRECTIVES

using KioskApp.Database;
using KioskApp.Services;

#endregion USING DIRECTIVES

namespace KioskApp.Modules
{
    public abstract class KServiceModule<TService> : AppModule where TService : IKioskService
    {
        protected TService Service { get; }

        protected KServiceModule(TService service, SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.Service = service;
        }
    }
}