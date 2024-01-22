using System.Collections.Generic;
using Libra.Contract;

namespace Libra.Web
{
    public class Navigation
    {
        public static Dictionary<string, NavigationModule> Modules { get; } = new Dictionary<string, NavigationModule>
        {
            [NavigationModule.INVOICES] = new NavigationModule
            {
                Id = NavigationModule.INVOICES,
                Url = "/Invoice/List",
                Roles = { Role.InvoiceViewer,Role.NewAdmin },
                Icon = "indent",
                Title = "INVOICE_LIST_PAGE_TITLE"
            },
            [NavigationModule.SECONDARY_INVOICES] = new NavigationModule
            {
                Id = NavigationModule.SECONDARY_INVOICES,
                Url = "/Invoice/ListSecondary",
                Roles = { Role.SecondaryInvoiceViewer,Role.NewAdmin },
                Icon = "outdent",
                Title = "INVOICE_SECONDARY_LIST_PAGE_TITLE"
            },
            [NavigationModule.ACTS] = new NavigationModule
            {
                Id = NavigationModule.ACTS,
                Url = "/Act/List",
                Roles = { Role.PrimaryActCreator,Role.ActCustomPayer,Role.ActBankPayer, },
                Icon = "envelope-o",
                Title = "ACT_LIST_PAGE_TITLE"
            },
            [NavigationModule.APPROVALS] = new NavigationModule
            {
                Id = NavigationModule.APPROVALS,
                Url = "/Act/Approvals",
                Roles = { Role.ActCurator, Role.Underwriter },
                Icon = "envelope-open-o",
                Title = "ACT_APPROVAL_PAGE_TITLE"
            },
            [NavigationModule.PAYOUTS] = new NavigationModule
            {
                Id = NavigationModule.PAYOUTS,
                Url = "/Payout/List",
                Roles = { Role.ActPayer },
                Icon = "money",
                Title = "PAYOUT_LIST_PAGE_TITLE"
            },
            [NavigationModule.USERS] = new NavigationModule
            {
                Id = NavigationModule.USERS,
                Url = "/User/List",
                Roles = { Role.UserAdmin },
                Icon = "users",
                Title = "USER_LIST_PAGE_TITLE"
            },
            [NavigationModule.COMMISSIONS] = new NavigationModule
            {
                Id = NavigationModule.COMMISSIONS,
                Url = "/Commission/List",
                Roles = { Role.UserAdmin },
                Icon = "percent",
                Title = "COMMISSION_LIST_PAGE_TITLE"
            },
            [NavigationModule.RECALCULATE] = new NavigationModule
            {
                Id = NavigationModule.RECALCULATE,
                Url = "/Recalculate/List",
                Roles = { Role.UserAdmin },
                Icon = "repeat",
                Title = "RECALCULATE_LIST_PAGE_TITLE"
            }
            ,
              [NavigationModule.COMISSIONCHANGE] = new NavigationModule
              {
                  Id = NavigationModule.COMISSIONCHANGE,
                  Url = "/ComissionChange/Index",
                  Roles = { Role.CanPredefineCommission, Role.CustomCommission },
                  Icon = "paypal",
                  Title = "COMISSIONCHANGE_LIST_PAGE"
              }
        };
    }

    public class NavigationModule
    {
        public const string INVOICES = "invoices";
        public const string SECONDARY_INVOICES = "secondary-invoices";
        public const string ACTS = "acts";
        public const string APPROVALS = "approvals";
        public const string PAYOUTS = "payouts";
        public const string USERS = "users";
        public const string COMMISSIONS = "commissions";
        public const string PROFILE = "profile";
        public const string RECALCULATE = "recalculate";
        public const string COMISSIONCHANGE = "comissionchange";

        public string Id { get; set; }
        public string Url { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();
        public string Icon { get; set; }
        public string Title { get; set; }
    }
}