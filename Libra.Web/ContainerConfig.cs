using Autofac;
using Autofac.Integration.Mvc;
using Libra.Contract;

namespace Libra.Web
{
    public class ContainerConfig : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterControllers(typeof(LibraApplication).Assembly)
                .PropertiesAutowired();

            builder.RegisterType<User>()
                .As<IUser>()
                .SingleInstance();

            builder.RegisterType<TranslationProvider>()
                .As<ITranslationProvider>()
                .SingleInstance();
        }
    }
}