using Libra.Contract;
using Libra.Contract.Models;
using Libra.Services.Database;
using Libra.Services.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Libra.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUser user;
        private readonly InvoiceMapper invoiceMapper;
        private readonly ITranslationProvider translationProvider;

        public InvoiceService(IUser user, InvoiceMapper invoiceMapper, ITranslationProvider translationProvider)
        {
            this.user = user;
            this.invoiceMapper = invoiceMapper;
            this.translationProvider = translationProvider;
        }

        public void Syncronize()
        {
            var now = DateTime.Now;
            var random = new Random(5);

            var names = new[]
            {
                "IVAN", "PETR", "MAMED", "DENIS", "ASLAN", "VICTOR", "MAXIM", "RUSLAN", "BORIS", "VLADIMIR"
            };

            var companies = new[]
            {
                "PE", "LA", "RO", "VU", "WI", "GA", "TY", "PO", "XE", "RA"
            };

            var fullProcessStatus = (int) (ActType.Primary | ActType.Secondary);

            using (var db = new LibraDb())
            {
                for (var i = 0; i < 10; i++)
                {
                    var amount = random.Next(50, 100);
                    var id = $"{GetSymbol(now.Month)}{GetSymbol(now.Day)}{GetSymbol(now.Hour)}{now:mmss}{i}";

                    var invoice = new DbInvoice
                    {
                        Id = id,
                        PolicyNumber = $"ADX{now:ssmmHH}",
                        Product = i % 3 + 1,
                        PolicyHolderType = i % 2 + 1,
                        PolicyHolderCode = random.Next(1000000, 9999999)
                            .ToString(),
                        PolicyHolderName = i % 2 == 0
                            ? $"{names[random.Next(0, 9)]}OV {names[random.Next(0, 9)]} {names[random.Next(0, 9)]}OVICH"
                            : $"{companies[random.Next(0, 9)]}{companies[random.Next(0, 9)]}{companies[random.Next(0, 9)]} LTD",
                        BeneficiaryType = 2,
                        BeneficiaryCode = "3366999",
                        BeneficiaryName = "ARVIK LTD",
                        Premium = amount,
                        PayablePremium = amount,
                        CreateDate = now,
                        CreatorId = user.Id,
                        ProcessStatus = fullProcessStatus,
                        Payments =
                        {
                            new DbPayment
                            {
                                InvoiceId = id,
                                Amount = amount,
                                PayDate = now
                            }
                        }
                    };

                    CalculateCommission(db, invoice, null, user.Username.ToUpperInvariant());

                    db.Invoices.Add(invoice);
                }

                db.SaveChanges();
            }
        }

        public void ImportInvoices(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ImportedInvoicesModel));
            StringReader reader = new StringReader(xml);
            ImportedInvoicesModel list = (ImportedInvoicesModel) serializer.Deserialize(reader);

            using (var db = new LibraDb())
            {
                // Force loading all users and use subsequent calls to db.Users.Local, to find a user amongst both new and existing users.
                //db.Users.Load();
                // Preload all commission configs to reduce number of queries. As a result in method CalculateCommission(...) we query db.CommissionConfigs.Local 
                //db.CommissionConfigs.Load();
                foreach (ImportedInvoiceModel invoiceModel in list.Invoices)
                {
                    if (invoiceModel.InvoiceType != (short) (FinancialsInvoiceType.Invoice))
                        continue;

                    DbUser mediator =
                        db.Users.FirstOrDefault(u => u.Username.Equals(invoiceModel.Mediator.Login, StringComparison.OrdinalIgnoreCase));
                    if (mediator == null)
                    {
                        mediator = new DbUser();
                        mediator.Name = invoiceModel.Mediator.FirstName;
                        mediator.Surname = invoiceModel.Mediator.LastName;
                        mediator.Username = invoiceModel.Mediator.Login;
                        mediator.Password = "";
                        db.Users.Add(mediator);
                    }


                    DbInvoice invoice = db.Invoices.Find(invoiceModel.InvoiceNumber);
                    if (invoice == null)
                    {
                       
                        invoice = new DbInvoice();
                        invoice.Id = invoiceModel.InvoiceNumber;
                        invoice.PolicyNumber = invoiceModel.PolicyNumber;
                        invoice.Product = invoiceModel.Product ?? 0;
                        invoice.PolicyHolderCode = invoiceModel.PolicyHolder.Code;
                        invoice.PolicyHolderName = invoiceModel.PolicyHolder.Name;
                        invoice.PolicyHolderType = invoiceModel.PolicyHolder.Type;
                        invoice.BeneficiaryCode = invoiceModel.Beneficiary?.Code ?? "";
                        invoice.BeneficiaryName = invoiceModel.Beneficiary?.Name ?? "";
                        invoice.BeneficiaryType = invoiceModel.Beneficiary?.Type ?? 0;
                        invoice.InsuredAddressRegion = invoiceModel.PolicyHolder.AddressRegion;
                        invoice.LoginRegion = invoiceModel.Mediator.LoginRegion;
                        invoice.CreateDate = invoiceModel.CreatedDate;
                        invoice.Creator = mediator;
                        invoice.PayablePremium = invoiceModel.PayablePremium;
                        invoice.Premium = invoiceModel.Premium;
                        invoice.ProcessStatus = (int) (ActType.Primary | ActType.Secondary);
                        invoice.VehicleType = invoiceModel.Vehicle.VehicleType;
                        invoice.EngineCapacity = invoiceModel.Vehicle.EngineCapacity;
                        invoice.IsCommissionFromIms = invoiceModel.UserCommission.HasValue;
                        invoice.VehicleModel = invoiceModel.Vehicle.Model;
                        invoice.VehicleBrand = invoiceModel.Vehicle.Brand;
                        invoice.VehicleAge = invoiceModel.Vehicle.VehicleAge;
                        invoice.VehicleRegion = invoiceModel.Vehicle.VehicleRegion;
                        
                        db.Invoices.Add(invoice);

                        CalculateCommission(db, invoice,  invoiceModel.UserCommission, mediator.Username.ToUpperInvariant());
                    }

                    // Invoice can be exported for the second time, if it was paid
                    if (invoiceModel.PaidDate != null)
                    {
                        DbPayment payments = db.Payments.FirstOrDefault(x =>
                            x.InvoiceId == invoiceModel.InvoiceNumber && x.Amount == invoiceModel.PaidPremium && x.PayDate == invoiceModel.PaidDate);

                        if (payments == null)
                        {
                            // If this invoice was just added, or haven't been paid before, then add payment info
                            DbPayment payment;
                            if (db.Entry(invoice).State == EntityState.Added ||
                                // will not make redundant DB read, if invoice was just created
                                (payment = db.Payments.FirstOrDefault(p => p.InvoiceId == invoice.Id)) == null && !invoice.Payments.Any(x =>
                                    x.InvoiceId == invoiceModel.InvoiceNumber && x.Amount == invoiceModel.PaidPremium &&
                                    x.PayDate == invoiceModel.PaidDate))
                            {
                                payment = new DbPayment();
                                payment.Invoice = invoice;
                                payment.PayDate = invoiceModel.PaidDate.Value;
                                payment.Amount = invoiceModel.PaidPremium.Value;
                                invoice.Payments.Add(payment);
                            }
                            else
                            // Otherwise just update payment info
                            {
                                payment.PayDate = invoiceModel.PaidDate.Value;
                                payment.Amount = invoiceModel.PaidPremium.Value;

                                //if invoice comes second time with paidpremium = 0, it means that payment for that invoice was refunded (rollbacked)
                                if (invoiceModel.PaidPremium.Value == 0)
                                {
                                    //if for rollbacked invoice already have been created act, we should send email to user
                                    if (db.Acts.FirstOrDefault(x => (x.Status != (int) ActStatus.Cancelled || x.Status != (int) ActStatus.Rejected)
                                                                    && x.Invoices.Any(inv => inv.Id == invoiceModel.InvoiceNumber)) != null)
                                    {
                                        HttpClient httpClient = new HttpClient();
                                        httpClient.DefaultRequestHeaders.Add("username", ConfigurationManager.AppSettings["IMSAPIUSERNAME"]);
                                        httpClient.DefaultRequestHeaders.Add("password", ConfigurationManager.AppSettings["IMSAPIPASSWORD"]);

                                        string[] emails = ConfigurationManager.AppSettings["PAYMENTROLLBACKEDUSER"].Split(';');
                                        string emailBody = string.Format(ConfigurationManager.AppSettings["PAYMENTEMAILBODY"], invoiceModel.InvoiceNumber);
                                        string emailSubject = ConfigurationManager.AppSettings["PAYMENTEMAILSUBJECT"];

                                        foreach (string email in emails)
                                        {
                                            //send email
                                            EmailMessageModel emailModel = new EmailMessageModel
                                            {
                                                To = email,
                                                Body = emailBody,
                                                ObjectGuid = null,
                                                Subject = emailSubject,
                                                SubType = QueueItemSubtype.EmailWithoutAttachment,
                                                SystemOid = 4002
                                            };

                                            var emailResult = Task.Run(async () =>
                                                await httpClient.PostJsonAsync<EmailMessageModel, string>
                                                    ($"{ConfigurationManager.AppSettings["IMSAPIBASEURL"]}/messaging/sendemail", emailModel)).Result;
                                        }
                                    }
                                    else
                                    {
                                        //if there are not any acts cascade delete all invoices
                                        db.Payments.Remove(payment);
                                        var commList = db.Commissions.Where(x => x.InvoiceId == invoice.Id).ToList();
                                        db.Commissions.RemoveRange(commList);
                                        db.Invoices.Remove(invoice);
                                    }
                                }
                            }
                        }
                    }

                    db.SaveChanges();
                }
            }
        }

        private void CalculateCommission(LibraDb db, DbInvoice invoice, decimal? customCommissionAmount, string username)
        {
            var beneficiaryCode = invoice.BeneficiaryCode?.ToUpperInvariant();

            List<DbManualCommission> manualCommissions = db.ManualCommissions.Where(c => c.InvoiceId == invoice.Id).ToList();

            if (manualCommissions.Count > 0)
            {
                foreach (var item in manualCommissions)
                {
                    invoice.Commissions.Add(new DbCommission
                    {
                        InvoiceId = invoice.Id,
                        ActType = item.ActType,
                        PayoutType = item.PayoutType,
                        Amount = customCommissionAmount ?? invoice.PayablePremium * item.Amount / 100
                    });

                    invoice.ProcessStatus = invoice.ProcessStatus & ~item.ActType;
                }

                return;
            }
            
            List<DbCommissionConfig> commissionConfigs = db.CommissionConfigs
                .Where(c =>
                    invoice.PayablePremium >= c.PremiumFrom && invoice.PayablePremium <= c.PremiumTo
                                                            && (!c.Product.HasValue || c.Product == invoice.Product)
                                                            //&& (!c.Brand.HasValue || c.Brand == invoice.Brand)
                                                            && (!c.PolicyHolderType.HasValue || c.PolicyHolderType == invoice.PolicyHolderType)
                                                            && (c.Username == null || c.Username.Trim() == username.Trim())
                                                            && (c.BeneficiaryCode == null || c.BeneficiaryCode == beneficiaryCode)
                                                            && invoice.CreateDate >= c.ValidFrom && invoice.CreateDate <= c.ValidTo).ToList();

            if (invoice.Product == 45)
            {
                commissionConfigs = commissionConfigs.Where(c =>
                (!c.VehicleType.HasValue || c.VehicleType == invoice.VehicleType)
                && (c.ModelName==null || c.ModelName == invoice.VehicleModel)
                && (c.BrandName == null || c.BrandName == invoice.VehicleBrand)
                && (invoice.EngineCapacity.HasValue && (!c.EngineCapacityFrom.HasValue || invoice.EngineCapacity >= c.EngineCapacityFrom) && (!c.EngineCapacityTo.HasValue || invoice.EngineCapacity <= c.EngineCapacityTo))
                && (invoice.VehicleAge.HasValue && (!c.VehicleAgeFrom.HasValue || invoice.VehicleAge>=c.VehicleAgeFrom) && (!c.VehicleAgeTo.HasValue || invoice.VehicleAge <= c.VehicleAgeTo))
                && (c.VehicleRegion==null || c.VehicleRegion == invoice.VehicleRegion)
                && (c.InsuredAddressRegion==null || c.InsuredAddressRegion == invoice.InsuredAddressRegion)
                && (c.LoginRegion == null || c.LoginRegion == invoice.LoginRegion)
                ).ToList();
                               
            }
            

            var configGroup = commissionConfigs
                .GroupBy(c => new
                {
                    c.ActType,
                    c.PayoutType
                });

            foreach (var item in configGroup)
            {
                if (customCommissionAmount.HasValue)
                {
                    invoice.Commissions.Add(new DbCommission
                    {
                        InvoiceId = invoice.Id,
                        ActType = item.Key.ActType,
                        PayoutType = item.Key.PayoutType,
                        Amount = (decimal) customCommissionAmount
                    });
                }
                else
                {
                    DbCommissionConfig configItem;
                    if (item.Any(x=>x.Product==45))
                    {
                        configItem = item.Where(i => i.VehicleType != null)
                            .OrderByDescending(GetWeight)
                            .First();
                    }
                    else
                    {
                        configItem = item
                            .OrderByDescending(GetWeight)
                            .First();
                    }
                    
                    var commission = invoice.PayablePremium * configItem.AmountPercent / 100 + configItem.AmountFixed;

                    commission = Math.Max(commission, Math.Max(configItem.AmountMin ?? 0, 0));
                    commission = Math.Min(commission, Math.Min(configItem.AmountMax ?? decimal.MaxValue, invoice.PayablePremium));

                    commission = Math.Round(commission, 2);

                    invoice.Commissions.Add(new DbCommission
                    {
                        InvoiceId = invoice.Id,
                        ActType = item.Key.ActType,
                        PayoutType = item.Key.PayoutType,
                        Amount = commission
                    });
                }

                invoice.ProcessStatus = invoice.ProcessStatus & ~item.Key.ActType;
            }
        }

        private static int GetWeight(DbCommissionConfig config)
        {
            return (config.VehicleType.HasValue ? 1 : 0)
                   + (config.BrandName != null ? 2 : 0)
                   + (config.ModelName != null ? 4 : 0)
                   + (config.Username != null ? 8 : 0)
                   + (config.BeneficiaryCode != null ? 16 : 0);
        }


        private static char GetSymbol(int i)
        {
            return (char) (i % 26 + 65);
        }

        public OperationResult<InvoiceViewModel> Load(InvoiceFilter filter)
        {
            if (filter.CreateDateFrom.HasValue)
            {
                filter.CreateDateFrom = filter.CreateDateFrom.Value.Date;
            }

            if (filter.CreateDateTill.HasValue)
            {
                filter.CreateDateTill = filter.CreateDateTill.Value.Date.AddDays(1);
            }

            if (filter.PayDateFrom.HasValue)
            {
                filter.PayDateFrom = filter.PayDateFrom.Value.Date;
            }

            if (filter.PayDateTill.HasValue)
            {
                filter.PayDateTill = filter.PayDateTill.Value.Date.AddDays(1);
            }

            using (var db = new LibraDb())
            {
                const int itemsPerPage = 100;
                IQueryable<DbInvoiceView> allInvoices;

                if (user.IsInRole(Role.NewAdmin))
                {
                    allInvoices = db.InvoiceView
                        .Where(i =>
                            (!filter.CreateDateFrom.HasValue || i.CreateDate >= filter.CreateDateFrom.Value)
                            && (!filter.CreateDateTill.HasValue || i.CreateDate < filter.CreateDateTill.Value)
                            && (!filter.PayDateFrom.HasValue || i.PayDate >= filter.PayDateFrom.Value)
                            && (filter.Invoice == null || i.Id.StartsWith(filter.Invoice))
                            && (filter.Policy == null || i.PolicyNumber.Contains(filter.Policy))
                            && (!filter.PayDateTill.HasValue || i.PayDate < filter.PayDateTill.Value)
                            && (string.IsNullOrEmpty(filter.Agent) || i.Creator.Contains(filter.Agent))
                            && ( /*filter.Status != InvoiceStatus.Paid ||*/ i.UnpaidPremium == 0)
                            //&& (filter.Status != InvoiceStatus.PartiallyPaid || i.UnpaidPremium > 0)
                            && (!filter.Product.HasValue || filter.Product == (Product) i.Product)
                            && (i.ProcessStatus & (int) filter.Type) == 0
                            && i.ProcessStatus != 4)
                        .Include(i => i.Commissions)
                        .Where(i => !filter.PayoutType.HasValue
                                    || i.Commissions.Any(c => filter.PayoutType == (PayoutType)c.PayoutType
                                                              && filter.Type == (ActType)c.ActType));
                }

                else
                {
                    if (user.IsInRole(Role.SupervisorInvoiceViewer))
                    {
                        filter.UserIds.AddRange(db.Users.Where(i => i.Supervisors.Contains(user.Id.ToString())).Select(i => i.Id).ToList());
                    }

                    allInvoices = db.InvoiceView
                        .Where(i =>
                            (!filter.UserIds.Any() || filter.UserIds.Contains(i.CreatorId))
                            && (!filter.CreateDateFrom.HasValue || i.CreateDate >= filter.CreateDateFrom.Value)
                            && (!filter.CreateDateTill.HasValue || i.CreateDate < filter.CreateDateTill.Value)
                            && (!filter.PayDateFrom.HasValue || i.PayDate >= filter.PayDateFrom.Value)
                            && (filter.Invoice == null || i.Id.StartsWith(filter.Invoice))
                            && (filter.Policy == null || i.PolicyNumber.Contains(filter.Policy))
                            && (!filter.PayDateTill.HasValue || i.PayDate < filter.PayDateTill.Value)
                            && (string.IsNullOrEmpty(filter.Agent) || i.Creator.Contains(filter.Agent))
                            && ( /*filter.Status != InvoiceStatus.Paid ||*/ i.UnpaidPremium == 0)
                            //&& (filter.Status != InvoiceStatus.PartiallyPaid || i.UnpaidPremium > 0)
                            && (!filter.Product.HasValue || filter.Product == (Product) i.Product)
                            && (i.ProcessStatus & (int) filter.Type) == 0
                            && i.ProcessStatus != 4)
                        .Include(i => i.Commissions)
                        .Where(i => !filter.PayoutType.HasValue
                                    || i.Commissions.Any(c => filter.PayoutType == (PayoutType) c.PayoutType
                                                              && filter.Type == (ActType) c.ActType));
                }

                var itemsCount = allInvoices.Count();
                var selectedInvoices = allInvoices
                    .OrderBy(i => i.Id).Skip((filter.PageNumber - 1) * itemsPerPage).Take(itemsPerPage);

                var invoicesList = selectedInvoices.ToList();
                
                var invoices = invoicesList
                    .Select(i => new InvoiceProjection
                    {
                        Invoice = i,
                        Commissions = i.Commissions.Where(c => filter.Type == (ActType) c.ActType)
                    }).AsEnumerable()
                    .Select(invoiceMapper.MapInvoiceModel)
                    .ToList();

                return new OperationResult<InvoiceViewModel>(new InvoiceViewModel
                {
                    InvoicesModel = invoices,
                    ItemsCount = itemsCount
                });
            }
        }


        public OperationResult<InvoiceModel> HideInvoices(string[] invoices)
        {
            using (var db = new LibraDb())
            {
                var selectedInvoices = db
                    .Invoices
                    .Include(a => a.Commissions)
                    .Where(i => invoices.Contains(i.Id)).ToList();

                selectedInvoices.ForEach(x => x.ProcessStatus = 4);
                db.SaveChanges();

                return new OperationResult<InvoiceModel>(translationProvider.GetString("INVOICE_HIDED"), IssueSeverity.Success);
            }
        }

        public OperationResult<List<PolicyInvoiceModel>> GetPolicyWithInvoices(string policyNumber)
        {
            // policyNumber = "31110008516";
            string baseUrl = ConfigurationManager.AppSettings["IMSAPIBASEURL"];
            string queryProxyUser = ConfigurationManager.AppSettings["IMSAPIUSERNAME"];
            string queryProxyPassword = ConfigurationManager.AppSettings["IMSAPIPASSWORD"];

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("username", queryProxyUser);
            httpClient.DefaultRequestHeaders.Add("password", queryProxyPassword);

            QueryModel queryModel = new QueryModel();
            queryModel.Parameters = "{\"policy_number\":\"" + policyNumber + "\"}";
            queryModel.DataType = "json";
            queryModel.ActionName = "GetPolicyAndInvoicebyPolicyNr";

            var result = Task.Run(async () => await httpClient.PostJsonAsync<QueryModel, QueryResult> 
                ($"{baseUrl}/queryProxy/runScript", queryModel)).Result;
            
            if (result.IsSuccess)
            {
                var obj = JsonConvert.DeserializeObject<Items>(result.Model.Data);
                OperationResult<List<PolicyInvoiceModel>> operation = new OperationResult<List<PolicyInvoiceModel>>();
                operation.Model = obj.items;
                return operation;
            }
            else
            {
                OperationResult<List<PolicyInvoiceModel>> operation = new OperationResult<List<PolicyInvoiceModel>>("", IssueSeverity.Error);
                return operation;
            }
        }
    }
}