using Autofac;
using Libra.Contract;

namespace Libra.Services
{
    public class ContainerConfig : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UserService>()
                .As<IUserService>()
                .SingleInstance();

            builder.RegisterType<CommissionService>()
                .As<ICommissionService>()
                .SingleInstance();

            builder.RegisterType<InvoiceService>()
                .As<IInvoiceService>()
                .SingleInstance();

            builder.RegisterType<InvoiceMapper>()
                .As<InvoiceMapper>()
                .SingleInstance();

            builder.RegisterType<ActService>()
                .As<IActService>()
                .SingleInstance();

            builder.RegisterType<PayoutService>()
                .As<IPayoutService>()
                .SingleInstance();

            builder.RegisterType<RecalculateService>()
                .As<IRecalculateService>()
                .SingleInstance();
        }
    }
}
