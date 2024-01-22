using Libra.Contract;
using Libra.Contract.Models;
using Libra.Services.Database;
using Libra.Services.Helper;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;

namespace Libra.Services
{
    public class ActService : IActService
    {
        private readonly IUser user;
        private readonly ITranslationProvider translationProvider;
        private readonly InvoiceMapper invoiceMapper;

        private const string EMPTY_ID = "---";

        public ActService(ITranslationProvider translationProvider, InvoiceMapper invoiceMapper, IUser user)
        {
            this.user = user;
            this.translationProvider = translationProvider;
            this.invoiceMapper = invoiceMapper;
        }

        public OperationResult<ActModel> Create(string[] invoices, ActType type, IUser user, int? customUserID = null)
        {
            if (customUserID == null && !IsTypeAvailable(type, user))
            {
                return new OperationResult<ActModel>(translationProvider.GetString("ACT_TYPE_FORBIDDEN"), IssueSeverity.Error);
            }

            using (var db = new LibraDb())
            {
                DbUser customUser = db.Users
                    .Where(x => x.Id == customUserID)
                    .Include(i => i.Approvals).FirstOrDefault();

                var selectedInvoices = db.InvoiceView
                    .Where(i => invoices.Contains(i.Id)).ToList();
                
                foreach (var invoice in selectedInvoices)
                {
                    invoice.Commissions = db.CommissionView.Where(c => c.InvoiceId == invoice.Id).ToList();
                }

                #region validation invoice payout type   and check same mediators invoices

                if (selectedInvoices.Select(x => x.Commissions.Where(u => u.ActType == 1).FirstOrDefault()?.PayoutType).Where(j => j != null)
                        .GroupBy(y => y).Count() > 1 ||
                    selectedInvoices.Select(x => x.Commissions.Where(u => u.ActType == 2).FirstOrDefault()?.PayoutType).Where(j => j != null)
                        .GroupBy(y => y).Count() > 1 ||
                    selectedInvoices.Select(x => x.Commissions.Where(u => u.ActType == 3).FirstOrDefault()?.PayoutType).Where(j => j != null)
                        .GroupBy(y => y).Count() > 1)
                {
                    return new OperationResult<ActModel>(translationProvider.GetString("DIFFERENT_PAYOUT_TYPE_FORBIDDEN"), IssueSeverity.Error);
                }

                #endregion
                
                int? groupId = null;
                groupId = getGroupIdContainsAllProducts(selectedInvoices);
                if (groupId == null)
                {
                    return new OperationResult<ActModel>(translationProvider.GetString("INVOICE_PRODUCT_TYPE"), IssueSeverity.Error);
                }

                var records = selectedInvoices
                    .Select(i => new InvoiceProjection
                    {
                        Invoice = i,
                        Commissions = i.Commissions.Where(c => c.ActType == (int) type)
                    })
                    .Select(invoiceMapper.MapInvoiceModel)
                    .ToList();

                var commission = records.SelectMany(r => r.Commissions).Sum(c => c.CalculatedAmount);

                var result = new OperationResult<ActModel>(new ActModel
                {
                    Id = EMPTY_ID,
                    Status = ActStatus.Draft,
                    Type = type,
                    StatusText = translationProvider.GetString(ActStatus.Draft),
                    CreatorId = customUserID == null ? user.Id : customUser.Id,
                    Creator = customUserID == null ? user.FullName : $"{customUser.Name} {customUser.Surname}",
                    ReceiverId = records.FirstOrDefault()?.CreatorId,
                    Receiver = records.FirstOrDefault()?.Creator,
                    CreateDate = DateTime.Now,
                    Commission = commission,
                    Brokers = GetBrokersList(),
                    GroupId = groupId
                });
                
                if (customUserID == null)
                {
                    db.Users
                        .Where(u => user.Supervisors.Contains(u.Id))
                        .ToList()
                        .Select(u => new ApprovalModel
                        {
                            ApproverId = u.Id,
                            Approver = u.FullName()
                        })
                        .ToList()
                        .ForEach(result.Model.Approvals.Add);
                }
                else
                {
                    db.Users
                        .Where(u => customUser.Supervisors.Contains(u.Id.ToString()))
                        .ToList()
                        .Select(u => new ApprovalModel
                        {
                            ApproverId = u.Id,
                            Approver = u.FullName()
                        })
                        .ToList()
                        .ForEach(result.Model.Approvals.Add);
                }

                List<string> underwrittingApprovableProductList =
                    db.Lobs.Where(x => x.approvable).Select(x => ((Product) x.lob_oid).ToString()).ToList();

                if (records.Any(x => underwrittingApprovableProductList.Contains(x.Product)))
                {
                    result.Model.Approvals.Add(new ApprovalModel
                    {
                        ApproverId = null,
                        Approver = translationProvider.GetString("ACT_APPROVAL_UNDERWRITER")
                    });
                }

                records.ForEach(result.Model.Invoices.Add);

                result.Model.AutoApprove =
                    customUserID == null ? user.IsInRole(Role.ApprovedActCreator) : customUser.Role == (int) Role.ApprovedActCreator;
                result.Model.CustomCommissions =
                    customUserID == null ? user.IsInRole(Role.CustomActCreator) : customUser.Role == (int) Role.CustomActCreator;
                result.Model.InsurerID = groupId == 1 ? Insurer.ATESHGAHLIFE : Insurer.ATESHGAH;
                return result;
            }
        }
        
        public OperationResult<ActModel> Add(string id, string[] invoices, ActType type, IUser user)
        {
            if (!IsTypeAvailable(type, user))
            {
                return new OperationResult<ActModel>(translationProvider.GetString("ACT_TYPE_FORBIDDEN"), IssueSeverity.Error);
            }

            using (var db = new LibraDb())
            {
                var act = db.Acts.Where(a => a.Id == id)
                    .Include(i => i.Invoices)
                    .Include(i => i.Commissions).FirstOrDefault();
                
                var listOfActInvoices = act.Invoices.Select(x => x.Id).ToList();
                var actInvoices = db.Invoices.Where(a => listOfActInvoices.Contains(a.Id))
                    .Include(i => i.Commissions)
                    .ToList();
                
                var selectedInvoicesProducts = db
                    .InvoiceView
                    .Where(i => invoices.Contains(i.Id))
                    .Include(i => i.Commissions).ToList();

                #region validation invoice payout type   and check same mediators invoices

                //invoice payout type
                if (selectedInvoicesProducts.Select(x => x.Commissions.FirstOrDefault(u => u.ActType == 1)?.PayoutType).Where(j => j != null)
                        .GroupBy(y => y).Count() > 1 ||
                    selectedInvoicesProducts.Select(x => x.Commissions.FirstOrDefault(u => u.ActType == 2)?.PayoutType).Where(j => j != null)
                        .GroupBy(y => y).Count() > 1 ||
                    selectedInvoicesProducts.Select(x => x.Commissions.FirstOrDefault(u => u.ActType == 3)?.PayoutType).Where(j => j != null)
                        .GroupBy(y => y).Count() > 1)
                {
                    return new OperationResult<ActModel>(translationProvider.GetString("DIFFERENT_PAYOUT_TYPE_FORBIDDEN"), IssueSeverity.Error);
                }
                
                if (actInvoices.Any(a => a.Commissions.Where(u => u.ActType == 1)
                        .Any(b => b.PayoutType != selectedInvoicesProducts.FirstOrDefault().Commissions.FirstOrDefault(u => u.ActType == 1)?.PayoutType))
                    ||
                    actInvoices.Any(a => a.Commissions.Where(u => u.ActType == 2)
                        .Any(b => b.PayoutType != selectedInvoicesProducts.FirstOrDefault().Commissions.FirstOrDefault(u => u.ActType == 2)?.PayoutType))
                    ||
                    actInvoices.Any(a => a.Commissions.Where(u => u.ActType == 3)
                        .Any(b => b.PayoutType != selectedInvoicesProducts.FirstOrDefault().Commissions.FirstOrDefault(u => u.ActType == 3)?.PayoutType)))
                {
                    return new OperationResult<ActModel>(translationProvider.GetString("DIFFERENT_PAYOUT_TYPE_FORBIDDEN"), IssueSeverity.Error);
                }

                #endregion

                int? groupId = null;
                groupId = getGroupIdContainsAllProducts(selectedInvoicesProducts, act.Invoices.ToList());
                if (groupId == null)
                {
                    return new OperationResult<ActModel>(translationProvider.GetString("INVOICE_PRODUCT_TYPE"), IssueSeverity.Error);
                }
                
                if (act == null
                    || act.CreatorId != user.Id
                    || act.Status != (int) ActStatus.Draft
                    || act.Type != (int) type)
                {
                    return new OperationResult<ActModel>(translationProvider.GetString("ACT_NOT_FOUND"), IssueSeverity.Error);
                }

                var result = Load(id, user);
                result.Model.GroupId = groupId;
                if (!result.IsSuccess)
                {
                    return result;
                }

                var model = result.Model;
                
                var newRecords = db
                    .InvoiceView
                    .Where(i => invoices.Contains(i.Id))
                    .Include(i => i.Commissions)
                    .Select(i => new InvoiceProjection
                    {
                        Invoice = i,
                        Commissions = i.Commissions.Where(c => c.ActType == (int) type)
                    }).AsEnumerable()
                    .Select(invoiceMapper.MapInvoiceModel)
                    .ToList();

                foreach (var record in newRecords)
                {
                    model.Invoices.Add(record);
                }

                return Save(model, user);
            }
        }

        public OperationResult<ActModel> Save(ActModel act, IUser user)
        {
            return Save(act, user, false);
        }

        public OperationResult<ActModel> Send(ActModel act, IUser user)
        {
            return Save(act, user, true);
        }

        private OperationResult<ActModel> Save(ActModel act, IUser user, bool send, int? customUserID = null)
        {
            if (customUserID == null && !IsTypeAvailable(act.Type, user))
            {
                return new OperationResult<ActModel>(translationProvider.GetString("ACT_TYPE_FORBIDDEN"), IssueSeverity.Error);
            }
            
            var isNew = act.Id == EMPTY_ID;

            DbAct dbAct;
            using (var db = new LibraDb())
            {
                DbUser customUser = db.Users.Where(x => x.Id == customUserID)
                    .Include(i => i.Approvals).FirstOrDefault();

                using (var scope = new TransactionScope(
                           TransactionScopeOption.Required,
                           new TransactionOptions {IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead}))
                {
                    var actInvoices = act.Invoices.ToDictionary(i => i.Number, i => i);

                    var dbActInvoices = db.InvoiceView
                        .Where(i => (!isNew || (i.ProcessStatus & (int)act.Type) == 0) && actInvoices.Keys.Contains(i.Id)).ToList();

                    foreach (var invoice in dbActInvoices)
                    {
                        invoice.Commissions = db.CommissionView.Where(c => c.InvoiceId == invoice.Id).ToList();
                    }

                    var dbInvoices = dbActInvoices
                        .ToDictionary(i => i.Id, i => i);

                    if (act.GroupId == null)
                    {
                        act.GroupId = getGroupIdContainsAllProducts(dbActInvoices);
                    }

                    var customCommission = false;
                    var customCommissionAllowed = customUserID == null
                        ? user.IsInRole(Role.CustomActCreator)
                        : (((Role) customUser.Role & Role.CustomActCreator) == Role.CustomActCreator);

                    foreach (var invoice in actInvoices.Values)
                    {
                        if (!dbInvoices.ContainsKey(invoice.Number))
                        {
                            return new OperationResult<ActModel>(translationProvider.GetString("INVALID_STATE"), IssueSeverity.Error);
                        }

                        var dbInvoice = dbInvoices[invoice.Number];
                        if (invoice.PaidPremium != dbInvoice.PaidPremium)
                        {
                            return new OperationResult<ActModel>(translationProvider.GetString("INVALID_STATE"), IssueSeverity.Error);
                        }
                        
                        //if act has been created manually then process has to continue over commission amount check foreach loop
                        if (act.IsManual) continue;
                        foreach (var commission in invoice.Commissions)
                        {
                            var dbCommission =
                                dbInvoice.Commissions.FirstOrDefault(c => c.PayoutType == (int) commission.Type && c.ActType == (int) act.Type);
                            if (dbCommission == null)
                            {
                                return new OperationResult<ActModel>(translationProvider.GetString("INVALID_STATE"), IssueSeverity.Error);
                            }

                            if (commission.CustomAmount == dbCommission.CustomAmount)
                            {
                                continue;
                            }

                            if (!customCommissionAllowed)
                            {
                                return new OperationResult<ActModel>(translationProvider.GetString("CUSTOM_COMMISSION_FORBIDDEN"), IssueSeverity.Error);
                            }

                            customCommission = true;
                            break;
                        }
                    }

                    var actId = isNew
                        ? $"ACT{int.Parse(db.Acts.Max(a => a.Id)?.Remove(0, 3) ?? "0") + 1:D5}"
                        : act.Id;
                    
                    if (isNew)
                    {
                        dbAct = new DbAct
                        {
                            Id = actId,
                            Status = send ? (int) ActStatus.Sent : (int) ActStatus.Draft,
                            Type = (int) act.Type,
                            CreatorId = customUserID == null ? user.Id : customUser.Id,
                            ReceiverId = act.ReceiverId ?? user.Id,
                            CreateDate = DateTime.Now,
                            Insurer = act.InsurerID == null ? null : (int?) act.InsurerID,
                            BrokerId = act.BrokerID,
                            GroupId = act.GroupId
                        };

                        db.Acts.Add(dbAct);
                    }
                    else
                    {
                        dbAct = db.Acts.Single(a => a.Id == actId);
                        dbAct.BrokerId = act.BrokerID;
                        dbAct.Insurer = (int?) act.InsurerID;
                        dbAct.GroupId = act.GroupId;
                        var commissions = db.ActCommissions.Where(c => c.ActId == actId).ToList();
                        db.ActCommissions.RemoveRange(commissions);
                        var invoices = commissions.Select(c => c.InvoiceId).Distinct();
                        foreach (var invoice in db.Invoices.Where(i => invoices.Contains(i.Id)))
                        {
                            invoice.ProcessStatus = (invoice.ProcessStatus | (int) act.Type) ^ (int) act.Type;
                        }

                        if (send)
                        {
                            dbAct.Status = (int) ActStatus.Sent;
                        }
                    }

                    //Checking if new commission added 

                    #region

                    var icommissions = act.Invoices.SelectMany(r => r.Commissions);
                    foreach (var item in icommissions)
                    {
                        // if commission is not in database but in model then add to database
                        var newManuallyAdded = db.Commissions.FirstOrDefault(c =>
                            c.InvoiceId == item.InvoiceId && c.PayoutType == (int) item.Type && c.ActType == (int) act.Type);
                        if (newManuallyAdded == null)
                        {
                            newManuallyAdded = new DbCommission
                            {
                                Amount = item.CalculatedAmount,
                                ActType = (int) act.Type,
                                InvoiceId = item.InvoiceId,
                                PayoutType = (int) item.Type
                            };
                            db.Commissions.Add(newManuallyAdded);
                            db.SaveChanges();
                        }
                    }

                    #endregion

                    foreach (var c in actInvoices.Values.SelectMany(i => i.Commissions))
                    {
                        if (act.IsManual)
                        {
                            if (c.Type == act.PayoutType)
                            {
                                var commission = new DbActCommission
                                {
                                    ActId = actId,
                                    Amount = c.CustomAmount,
                                    InvoiceId = c.InvoiceId,
                                    PayoutType = (int) c.Type
                                };
                                dbAct.Commissions.Add(commission);
                            }
                        }
                        else 
                        {
                            var commission = new DbActCommission
                            {
                                ActId = actId,
                                Amount = c.CustomAmount,
                                InvoiceId = c.InvoiceId,
                                PayoutType = (int) c.Type
                            };
                            dbAct.Commissions.Add(commission);
                        }
                    }

                    if (!act.IsManual)
                    {
                        foreach (var invoice in db.Invoices.Where(i => actInvoices.Keys.Contains(i.Id)))
                        {
                            invoice.ProcessStatus = invoice.ProcessStatus | (int) act.Type;
                        }
                    }

                    var approve = !customCommission && (customUserID == null
                        ? user.IsInRole(Role.ApprovedActCreator)
                        : customUser.Role == (int) Role.ApprovedActCreator);

                    if (send)
                    {
                        if (approve)
                        {
                            ApproveAct(dbAct);
                            if (!isNew)
                            {
                                db.Approvals.RemoveRange(db.Approvals.Where(a => a.ActId == actId));
                            }
                        }
                        else if (isNew)
                        {
                            CreateApprovals(dbAct, actInvoices.Values, user, db, send);
                        }
                    }
                    else if (isNew)
                    {
                        if (customUserID == null)
                            CreateApprovals(dbAct, actInvoices.Values, user, db, send);
                        else
                            CreateApprovals(dbAct, actInvoices.Values, user, db, false, customerId: customUserID);
                    }
                    //if Act is created manually, we insert new record to Approvals Table to show this act in Act/List and Act/Approvals page 
                    if (act.IsManual)
                    {
                        dbAct.Approvals.Add(new DbApproval
                        {
                            ActId = act.Id,
                            ApproverId = null,
                        });
                    }

                    db.SaveChanges();
                    scope.Complete();
                }
            }

            return new OperationResult<ActModel>(MapActModel(dbAct));
        }

        public OperationResult<ICollection<ActListItemModel>> Load(ActFilter filter)
        {
            int _type = filter.Types.Count > 0 ? (int) filter.Types[0] : 0;
            if (filter.CreateDateFrom.HasValue)
            {
                filter.CreateDateFrom = filter.CreateDateFrom.Value.Date;
            }

            if (filter.CreateDateTill.HasValue)
            {
                filter.CreateDateTill = filter.CreateDateTill.Value.Date.AddDays(1);
            }

            if (filter.PayoutDateFrom.HasValue)
            {
                filter.PayoutDateFrom = filter.PayoutDateFrom.Value.Date;
            }

            if (filter.PayoutDateTill.HasValue)
            {
                filter.PayoutDateTill = filter.PayoutDateTill.Value.Date.AddDays(1);
            }
            
            if (filter.ApproveDateFrom.HasValue)
            {
                filter.ApproveDateFrom = filter.ApproveDateFrom.Value.Date;
            }
            
            if (filter.ApproveDateTill.HasValue)
            {
                filter.ApproveDateTill = filter.ApproveDateTill.Value.Date.AddDays(1);
            }
            
            if (filter.Types.Count > 0)
            {
                filter.Creator = null;
                filter.CreatorId = null;
            }
            
            bool isActCreator = user.IsInRole(Role.PrimaryActCreator);
            using (var db = new LibraDb())
            {
                List<ActListItemModel> actsList;
                const int itemsPerPage = 100;
                
                if (filter.Types?.Count > 0 && filter.Types?.Count() < 2)
                {
                    int type = (int)filter.Types[0];
                    var allActs = db.ActView
                        .Where(a => (!filter.Status.HasValue || a.Status == filter.Status.Value)
                                    && (string.IsNullOrEmpty(filter.PolicyNumber) || filter.ListActNumber.Count != 0)
                                    && (string.IsNullOrEmpty(filter.InvoiceNumber) || filter.ListActNumber.Count != 0)
                                    && (!filter.ListActNumber.Any() || filter.ListActNumber.Contains(a.Id))
                                    && (string.IsNullOrEmpty(filter.Receiver) || a.ReceiverCode == filter.Receiver)
                                    && (!filter.CreateDateFrom.HasValue || a.CreateDate >= filter.CreateDateFrom.Value)
                                    && (!filter.CreateDateTill.HasValue || a.CreateDate < filter.CreateDateTill.Value)
                                    && (filter.ActNumber == null || filter.ActNumber == a.Id)
                                    && (!filter.PayoutDateFrom.HasValue || a.Payouts.Any(p => p.PayDate >= filter.PayoutDateFrom.Value))
                                    && (!filter.PayoutDateTill.HasValue || a.Payouts.Any(p => p.PayDate < filter.PayoutDateTill.Value))
                                    && (a.PayoutType == type)
                        );
                    
                    var selectedActs = allActs
                        .OrderByDescending(i => i.Id).Take(itemsPerPage);
                    
                    var included = selectedActs.Include(a => a.Payouts);
                
                    actsList = included.ToList()
                        .Select(a => new ActListItemModel
                        {
                            Id = a.Id,
                            Status = a.Status,
                            CreateDate = a.CreateDate,
                            Payouts = a.Payouts.Select(p => new PayoutModel
                            {
                                Type = (PayoutType)p.Type,
                                PayDate = p.PayDate
                            }).ToList(),
                            Amount = a.Amount
                        })
                        .ToList();
                }
                else
                {
                    var allActs = db.Acts
                        .Where(a => (!filter.IsUnderwriter || user.ProductGroup.Contains((int) a.GroupId))
                                    && (!filter.Status.HasValue || a.Status == filter.Status.Value)
                                    && (isActCreator || a.Status != 0)
                                    && (!filter.CreateDateFrom.HasValue || a.CreateDate >= filter.CreateDateFrom.Value)
                                    && (!filter.CreateDateTill.HasValue || a.CreateDate < filter.CreateDateTill.Value)
                                    && (filter.ActNumber == null || filter.ActNumber == a.Id)
                                    && (String.IsNullOrEmpty(filter.PolicyNumber) || a.Invoices.Any(i => i.PolicyNumber == filter.PolicyNumber))
                                    && (String.IsNullOrEmpty(filter.InvoiceNumber) || a.Invoices.Any(i => i.Id == filter.InvoiceNumber)))
                        
                        //Include Approve  with its filters
                        .Include(x => x.Approvals).Where(a => a.Approvals.Any(b =>
                            (!filter.ApproveDateFrom.HasValue || (b.ApproveDate.HasValue
                                ? b.ApproveDate >= filter.ApproveDateFrom.Value
                                : b.RejectDate >= filter.ApproveDateFrom.Value))
                            && (!filter.ApproveDateTill.HasValue || (b.ApproveDate.HasValue
                                ? b.ApproveDate < filter.ApproveDateTill.Value
                                : b.RejectDate < filter.ApproveDateTill.Value))
                            && ((filter.IsUnderwriter) || (filter.ApproverId.HasValue && b.ApproverId == filter.ApproverId.Value) || isActCreator ||
                                _type != 0)
                        ))
                        
                        // Include Creator with its filters
                        .Include(x => x.Creator).Where(a =>
                            (filter.CreatorId == null || a.Creator.Id == (filter.CreatorId ?? 0))
                            && string.IsNullOrEmpty(filter.Creator) || (a.Creator.Name + " " + a.Creator.Surname).Contains(filter.Creator)
                        );

                    // order by Act Number  then Take first logical first count of found row 
                    var selectedActs = allActs
                        .OrderByDescending(i => i.Id).Take(itemsPerPage);

                    var included = selectedActs
                            
                        //Include commision info to found acts
                        .Include(x => x.Commissions)
                        
                        // Include Payouts
                        .Include(x => x.Payouts).Where(f =>
                            (!filter.PayoutDateFrom.HasValue || f.Payouts.Any(p => p.PayDate >= filter.PayoutDateFrom.Value))
                            && (!filter.PayoutDateTill.HasValue || f.Payouts.Any(p => p.PayDate < filter.PayoutDateTill.Value))
                            && (_type == 0 || f.Payouts.Any(a => a.Type == _type))
                        );

                    actsList = included.ToList()
                        .Select(a => new ActListItemModel
                        {
                            Id = a.Id,
                            Status = a.Status,
                            CreateDate = a.CreateDate,
                            Amount = a.Commissions.Sum(c => c.Amount),
                            Creator = a.Creator?.Name + ' ' + a.Creator?.Surname,
                            ApprovalDate = null,
                            ApproverFullname = user.FullName,
                            Payouts = a.Payouts.Select(p => new PayoutModel
                            {
                                Type = (PayoutType) p.Type,
                                PayDate = p.PayDate
                            }).ToList(),
                            IsApproved = a.Approvals.Any(x => x.ApproverId == user.Id)
                                ? (bool) a.Approvals.FirstOrDefault(x => x.ApproverId == user.Id)?.ApproveDate.HasValue
                                    ? true
                                    : (bool) a.Approvals.FirstOrDefault(x => x.ApproverId == user.Id)?.RejectDate.HasValue
                                        ? false
                                        : (bool?) null
                                : null
                        }).ToList();
                }
                
                actsList.ForEach(a =>
                {
                    a.StatusText = translationProvider.GetString((ActStatus) a.Status);
                    a.PayoutInfo = GetPayoutInfo(a.Payouts);
                });

                return new OperationResult<ICollection<ActListItemModel>>(actsList);
            }
        }

        public OperationResult<ActModel> Load(string id, IUser user)
        {
            using (var db = new LibraDb())
            {
                var act = db.Acts
                    .Include(a => a.Approvals.Select(x => x.Approver))
                    .Include(a => a.Creator)
                    .Include(a => a.Receiver)
                    .Include(a => a.Invoices.Select(i => i.Commissions))
                    .FirstOrDefault(a => a.Id == id);

                if (user.IsInRole(Role.Admin) || user.IsInRole(Role.AllActCustomTypeViewver) || user.IsInRole(Role.AllActBankTypeViewer))
                {
                    if (act == null)
                    {
                        return new OperationResult<ActModel>(translationProvider.GetString("ACT_NOT_FOUND"), IssueSeverity.Error);
                    }
                }

                else if (act == null || act.CreatorId != user.Id && act.Status == (int) ActStatus.Draft)
                {
                    return new OperationResult<ActModel>(translationProvider.GetString("ACT_NOT_FOUND"), IssueSeverity.Error);
                }

                var model = MapActModel(act);

                if (user.IsInRole(Role.AllActBankTypeViewer))
                {
                    foreach (var item in model.Invoices)
                    {
                        item.Commissions = item.Commissions.Where(c => c.Type == PayoutType.Bank).ToList();
                    }
                }

                else if (user.IsInRole(Role.AllActCustomTypeViewver))
                {
                    foreach (var item in model.Invoices)
                    {
                        item.Commissions = item.Commissions.Where(c => c.Type == PayoutType.Custom).ToList();
                    }
                }
                
                if (model.Status == ActStatus.Draft)
                {
                    model.AutoApprove = user.IsInRole(Role.ApprovedActCreator);
                    model.CustomCommissions = user.IsInRole(Role.CustomActCreator);
                }

                return new OperationResult<ActModel>(model);
            }
        }

        public OperationResult<ActModel> Approve(string id, IUser user)
        {
            DbAct act;

            using (var db = new LibraDb())
            {
                using (var scope = new TransactionScope(
                           TransactionScopeOption.Required,
                           new TransactionOptions
                           {
                               IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead
                           }))
                {
                    act = db.Acts
                        .Include(a => a.Approvals.Select(x => x.Approver))
                        .Include(a => a.Commissions)
                        .Include(a => a.Creator)
                        .Single(a => a.Id == id);
                    var approval = act.Approvals.FirstOrDefault(a => a.ActId == id && a.ApproverId == user.Id && !a.ApproveDate.HasValue);

                    if (approval == null
                        && user.IsInRole(Role.Underwriter))
                    {
                        approval = act.Approvals.FirstOrDefault(a => a.ActId == id && !a.ApproverId.HasValue && !a.ApproveDate.HasValue);
                    }

                    if (approval == null)
                    {
                        return new OperationResult<ActModel>(translationProvider.GetString("ACT_APPROVAL_INVALID"), IssueSeverity.Error);
                    }

                    if (act.Status != (int) ActStatus.Sent)
                    {
                        return new OperationResult<ActModel>(translationProvider.GetString("ACT_STATUS_INVALID"), IssueSeverity.Error);
                    }

                    approval.ApproveDate = DateTime.Now;
                    approval.RejectDate = null;

                    if (!approval.ApproverId.HasValue)
                    {
                        approval.ApproverId = user.Id;
                        approval.Approver = db.Users.FirstOrDefault(u => u.Id == user.Id);
                    }

                    if (act.Approvals.All(a => a.ApproveDate.HasValue))
                    {
                        ApproveAct(act);
                    }

                    db.SaveChanges();
                    scope.Complete();
                }
            }

            using (var db = new LibraDb())
            {
                ReloadInvoices(act, db);
                return new OperationResult<ActModel>(MapActModel(act));
            }
        }

        public OperationResult<ActModel> Reject(ActModel actParam, IUser user)
        {
            DbAct act;

            using (var db = new LibraDb())
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required,
                           new TransactionOptions
                           {
                               IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead
                           })
                       )
                {
                    act = db.Acts
                        .Include(a => a.Approvals.Select(x => x.Approver))
                        .Include(a => a.Payouts)
                        .Include(a => a.Creator)
                        .Include(a => a.Invoices.Select(i => i.Commissions))
                        .Single(a => a.Id == actParam.Id);
                    var approval = act.Approvals.FirstOrDefault(a => a.ActId == actParam.Id && a.ApproverId == user.Id && !a.RejectDate.HasValue);

                    if (approval == null
                        && user.IsInRole(Role.Underwriter))
                    {
                        approval = act.Approvals.FirstOrDefault(a => a.ActId == actParam.Id && !a.ApproverId.HasValue && !a.RejectDate.HasValue);
                    }

                    if (approval == null)
                    {
                        return new OperationResult<ActModel>(translationProvider.GetString("ACT_APPROVAL_INVALID"), IssueSeverity.Error);
                    }

                    var result = CancelAct(act, ActStatus.Rejected, db);
                    if (!result.IsSuccess)
                    {
                        return result;
                    }

                    approval.ApproveDate = null;
                    approval.RejectDate = DateTime.Now;
                    approval.RejectNote = actParam.RejectNote;

                    if (!approval.ApproverId.HasValue)
                    {
                        approval.ApproverId = user.Id;
                        approval.Approver = db.Users.FirstOrDefault(u => u.Id == user.Id);
                    }

                    db.SaveChanges();
                    scope.Complete();
                }

                string[] actInvoices = act.Invoices.Select(i => i.Id).ToArray<string>();

                actParam.Id = EMPTY_ID;
                actParam.Status = ActStatus.Draft;
                actParam.StatusText = "Draft";

                Save(actParam, null, false, act.Creator.Id);
            }

            using (var db = new LibraDb())
            {
                ReloadInvoices(act, db);
                return new OperationResult<ActModel>(MapActModel(act));
            }
        }

        public OperationResult<ActModel> Cancel(string id, IUser user)
        {
            using (var db = new LibraDb())
            {
                var act = db.Acts
                    .Include(a => a.Invoices)
                    .Include(a => a.Payouts)
                    .Include(a => a.Creator)
                    .Include(a => a.Approvals)
                    .Single(a => a.Id == id);

                if (act == null)
                {
                    return new OperationResult<ActModel>(translationProvider.GetString("ACT_NOT_FOUND"), IssueSeverity.Error);
                }

                if (act.CreatorId != user.Id)
                {
                    return new OperationResult<ActModel>(translationProvider.GetString("ACT_CREATOR_ACTION"), IssueSeverity.Error);
                }

                var result = CancelAct(act, ActStatus.Cancelled, db);
                if (!result.IsSuccess)
                {
                    return result;
                }

                db.SaveChanges();
                ReloadInvoices(act, db);

                return new OperationResult<ActModel>(MapActModel(act));
            }
        }

        public OperationResult<ActModel> Print(string id, IUser user)
        {
            var act = Load(id, user);
            
            //start
            string Xml;
            var document = new XElement("PrintableDocumentPack");

            var root = new XElement("Policy");
            root.Add(new XElement("PrintoutType", "Application"));
            root.Add(new XElement("SystemLOB", "", new XAttribute("value", "340")));
            root.Add(new XElement("printFormIdentifier", "LibraAct"));

            var custom = new XElement("CustomNodes");
            custom.Add(new XElement("ActNumber", act.Model.Id));
            custom.Add(new XElement("CommissionAmount", act.Model.Commission));
            custom.Add(new XElement("TotalPremium", act.Model.TotalPremium));
            custom.Add(new XElement("InvoiceCount", act.Model.InvoiceCount));
            custom.Add(new XElement("Creator", act.Model.Creator));
            using (var db = new LibraDb())
            {
                var currentusr = db.Users.FirstOrDefault(a => a.Id == act.Model.BrokerID);
                if (currentusr != null)
                {
                    custom.Add(new XElement("BrokerName", currentusr.Name));
                    custom.Add(new XElement("ContractDateAndNo", currentusr.Surname));
                }
            }
            
            custom.Add(new XElement("Insurer",
                act.Model.InsurerID == null ? "" : translationProvider.GetString("INSURER_" + Convert.ToString(act.Model.InsurerID))));

            int sequence = 0;
            var installments = new XElement("Installments");
            foreach (var invoice in act.Model.Invoices.OrderBy(x => x.Product))
            {
                var commissionsList = invoice.Commissions.Where(c => c.CustomAmount > 0);

                if (commissionsList.Any() || invoice.Commission.Value > 0)
                {
                    if (commissionsList.Any())
                        foreach (var commission in commissionsList)
                            sequence = GenerateInvoiceRelatedElements(sequence, installments, invoice, commission.CustomPercent,
                                commission.CustomAmount);
                    else
                        sequence = GenerateInvoiceRelatedElements(sequence, installments, invoice, invoice.Commission.Percent,
                            invoice.Commission.Value);
                }
            }
            
            root.Add(installments);
            root.Add(custom);
            document.Add(root);

            Xml = document.ToString();

            byte[] printFileContent;
            string printFileType;

            string printUrl = ConfigurationManager.AppSettings["STREAMSERV_URL"];
            string printUser = ConfigurationManager.AppSettings["STREAMSERV_USER"];
            string printPassword = ConfigurationManager.AppSettings["STREAMSERV_PASSWORD"];

            var request = (HttpWebRequest) WebRequest.Create(printUrl);
            request.Credentials = new NetworkCredential(printUser, printPassword);
            var requestContent = System.Text.Encoding.UTF8.GetBytes(Xml);
            request.Method = "POST";
            request.ContentLength = requestContent.Length;
            request.ContentType = "text/xml";
            using (var stream = request.GetRequestStream())
            {
                stream.Write(requestContent, 0, requestContent.Length);
                stream.Close();
            }

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                System.Text.Encoding encoding;
                try
                {
                    encoding = System.Text.Encoding.GetEncoding(response.CharacterSet);
                }
                catch (ArgumentException)
                {
                    /* If specified response encoding is not supported
                     * use default encoding */
                    encoding = System.Text.Encoding.Default;
                }

                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream, encoding))
                    {
                        printFileContent = encoding.GetBytes(reader.ReadToEnd());
                        printFileType = response.ContentType;
                        reader.Close();
                    }

                    stream.Close();
                }

                response.Close();
            }

            return new OperationResult<ActModel>(new ActModel() {PrintFileContent = printFileContent, PrintFileType = printFileType});
        }

        private int GenerateInvoiceRelatedElements(int sequence, XElement installments, InvoiceModel invoice, decimal commissionPercent,
            decimal commissionAmount)
        {
            sequence++;
            var installment = new XElement("Installment");

            var payment = new XElement("Payment");

            payment.Add(new XElement("Sequence", sequence));
            payment.Add(new XElement("Amount", invoice.Premium));
            payment.Add(new XElement("Percent", commissionPercent));
            payment.Add(new XElement("ComissionAmount", commissionAmount));
            
            var payDate = DateTime.Now;
            using (var db = new LibraDb())
            {
                payDate = db.Payments.FirstOrDefault(x => x.Invoice.Id == invoice.Number) == null
                    ? DateTime.Now
                    : db.Payments.FirstOrDefault(x => x.Invoice.Id == invoice.Number).PayDate;
            }

            payment.Add(new XElement("Date", payDate));
            payment.Add(new XElement("ProductName", translationProvider.GetString(invoice.Product)));
            payment.Add(new XElement("PolicyNo", invoice.PolicyNumber));
            installment.Add(payment);
            installments.Add(installment);
            return sequence;
        }

        private int? getGroupIdContainsAllProducts(List<DbInvoiceView> invoices, List<DbActInvoiceView> invoicesFromCreatedAct = null)
        {
            using (var db = new LibraDb())
            {
                foreach (var prDetail in db.ProductDetails)
                {
                    var products = MapProductDetailModel(prDetail).ProductIds;
                    if (invoices.All(x => products.Contains(x.Product)))
                    {
                        var groupId = prDetail.GroupId;
                        if (invoicesFromCreatedAct != null &&
                            invoicesFromCreatedAct.Any(x => !products.Contains(x.Product))) //if act creaeted before and invoice adding now
                        {
                            return null;
                        }

                        return groupId;
                    }
                }

                return null;
            }
        }

        private void CreateApprovals(DbAct act, Dictionary<string, InvoiceModel>.ValueCollection invoices, IUser user, LibraDb db, bool send,
            int? customerId = null)
        {
            List<string> underwrittingApprovableProductList = db.Lobs.Where(x => x.approvable).Select(x => ((Product) x.lob_oid).ToString()).ToList();

            if (customerId != null)
            {
                var customerUser = db.Users.FirstOrDefault(u => u.Id == customerId);

                customerUser?
                    .Supervisors.Split(',')
                    .Select(s => Convert.ToInt32(s))
                    .Select(s => new DbApproval
                    {
                        ActId = act.Id,
                        ApproverId = s
                    })
                    .ToList()
                    .ForEach(act.Approvals.Add);
            }
            else
            {
                user
                    .Supervisors
                    .Select(s => new DbApproval
                    {
                        ActId = act.Id,
                        ApproverId = s
                    })
                    .ToList()
                    .ForEach(act.Approvals.Add);
            }

            if (invoices.Any(x => underwrittingApprovableProductList.Contains(x.Product)))
            {
                act.Approvals.Add(new DbApproval
                {
                    ActId = act.Id,
                    ApproverId = null,
                });

                var underwriterName = translationProvider.GetString("ACT_APPROVAL_UNDERWRITER");
                var underwriter = db.Users.FirstOrDefault(x => x.Username == underwriterName);
                SendEmail(underwriter.Email, act.Id);
            }

            //act.Approvals.ToList().ForEach(x=>)

            if (send)
            {
                foreach (var t in act.Approvals.Where(x => x.ApproverId != null))
                {
                    var approver = db.Users.FirstOrDefault(x => x.Id == t.ApproverId);
                    SendEmail(approver.Email, t.ActId);
                }

                //always send email to Approver = translationProvider.GetString("ACT_APPROVAL_UNDERWRITER"). Because He/She always added to approve list of act in CreateAct method
            }
        }


        private static void ApproveAct(DbAct act)
        {
            act.Status = (int) ActStatus.Approved;

            foreach (var p in act.Commissions.GroupBy(c => c.PayoutType))
            {
                act.Payouts.Add(new DbPayout
                {
                    ActId = act.Id,
                    Type = p.Key,
                    Amount = p.Sum(c => c.Amount)
                });
            }
        }

        private OperationResult<ActModel> CancelAct(DbAct act, ActStatus newStatus, LibraDb db)
        {
            if (act.Status != (int) ActStatus.Sent
                && act.Status != (int) ActStatus.Approved
                && act.Status != (int) ActStatus.Rejected)
            {
                return new OperationResult<ActModel>(translationProvider.GetString("ACT_STATUS_INVALID"), IssueSeverity.Error);
            }

            if (act.Payouts.Any(p => p.PayDate.HasValue))
            {
                return new OperationResult<ActModel>(translationProvider.GetString("ACT_IS_PAID"), IssueSeverity.Error);
            }

            if (user.Id == act.Creator.Id && act.Approvals.Any(x => x.ApproveDate != null))
            {
                return new OperationResult<ActModel>(translationProvider.GetString("ACT_HAS_APPROVAL"), IssueSeverity.Error);
            }

            act.Status = (int) newStatus;

            var invoices = act.Invoices.Select(i => i.Id)
                .ToList();
            foreach (var invoice in db.Invoices.Where(i => invoices.Contains(i.Id)))
            {
                invoice.ProcessStatus = (invoice.ProcessStatus | act.Type) ^ act.Type;
            }

            foreach (var payout in act.Payouts.ToArray())
            {
                db.Payouts.Remove(payout);
            }

            return new OperationResult<ActModel>();
        }

        private static void ReloadInvoices(DbAct act, LibraDb db)
        {
            act.Invoices.Clear();

            db.ActInvoiceView
                .Where(i => i.ActId == act.Id)
                .Include(i => i.Commissions)
                .ToList()
                .ForEach(act.Invoices.Add);
        }

        private ActModel MapActModel(DbAct act)
        {
            var model = new ActModel
            {
                Id = act.Id,
                Status = (ActStatus) act.Status,
                StatusText = translationProvider.GetString((ActStatus) act.Status),
                RejectNote = act.Approvals.FirstOrDefault(x => !string.IsNullOrEmpty(x.RejectNote)) == null
                    ? null
                    : act.Approvals.FirstOrDefault(x => !string.IsNullOrEmpty(x.RejectNote)).RejectNote,
                Type = (ActType) act.Type,
                Creator = act.Creator?.FullName(),
                CreatorId = act.CreatorId,
                Receiver = act.Receiver?.FullName(),
                ReceiverId = act.ReceiverId,
                CreateDate = act.CreateDate,
                Commission = act.Invoices.SelectMany(i => i.Commissions).Sum(c => c.CustomAmount),
                TotalPremium = act.Invoices.Sum(c => c.Premium),
                InvoiceCount = act.Invoices?.Count,
                InsurerID = (Insurer?) act.Insurer ?? null,
                BrokerID = act.BrokerId,
                Brokers = GetBrokersList(),
                GroupId = act.GroupId
            };

            act.Invoices?
                .Select(i => new InvoiceProjection
                {
                    Invoice = i,
                    Commissions = i.Commissions
                })
                .Select(invoiceMapper.MapInvoiceModel)
                .ToList()
                .ForEach(model.Invoices.Add);

            act.Approvals?
                .Select(MapApprovalModel)
                .ToList()
                .ForEach(model.Approvals.Add);

            return model;
        }

        internal ApprovalModel MapApprovalModel(DbApproval a)
        {
            return new ApprovalModel
            {
                ApproverId = a.ApproverId,
                Approver = a.ApproverId.HasValue ? a.Approver?.FullName() : translationProvider.GetString("ACT_APPROVAL_UNDERWRITER"),
                IsApproved = a.ApproveDate.HasValue ? true : a.RejectDate.HasValue ? false : (bool?) null
            };
        }

        private static bool IsTypeAvailable(ActType type, IUser user)
        {
            return type == ActType.Primary && user.IsInRole(Role.PrimaryActCreator) || user.IsInRole(Role.UserComission)
                                                                                    || type == ActType.Secondary &&
                                                                                    user.IsInRole(Role.SecondaryActCreator) ||
                                                                                    user.IsInRole(Role.UserComission);
        }

        private string GetPayoutInfo(IEnumerable<PayoutModel> payouts)
        {
            return string.Join(
                " | ",
                payouts.Select(p =>
                    $"{translationProvider.GetString(p.Type)}:{p.PayDate?.ToString(Constants.DATE_FORMAT) ?? "-"}"));
        }

        private List<UserModel> GetBrokersList()
        {
            var brokerList = new List<UserModel>();
            using (var db = new LibraDb())
            {
                var users = db.Users;
                foreach (var currentUser in users)
                {
                    if (((Role) currentUser.Role & Role.Broker) == Role.Broker)
                        brokerList.Add(new UserModel() {Id = currentUser.Id, Username = currentUser.Username});
                }
            }

            return brokerList;
        }

        public OperationResult<ProductDetailModel> LoadGroupProducts(int id)
        {
            using (var db = new LibraDb())
            {
                var detail = db.ProductDetails
                    .Where(g => g.GroupId == id)
                    .FirstOrDefault(g => g.Id == id);


                var model = MapProductDetailModel(detail);
                return new OperationResult<ProductDetailModel>(model);
            }
        }

        private ProductDetailModel MapProductDetailModel(DbProductDetail detail)
        {
            var model = new ProductDetailModel
            {
                Id = detail.Id,
                GroupId = detail.GroupId,
                ProductIds = detail.ProductIds?.Split(',').Select(int.Parse).ToList() ?? new List<int>()
            };

            return model;
        }

        public OperationResult<byte[]> ExportToExcel(string id, IUser user)
        {
            var listInvoices = Load(id, user).Model.Invoices.Select(x =>
            {
                x.Commission.Percent = Convert.ToDecimal(string.Format("{0:0.0}", (Math.Ceiling(x.Commission.Percent * 10) / 10)));
                x.PolicyHolder = x.PolicyHolder.Substring(x.PolicyHolder.IndexOf(',') + 1);
                return x;
            }).ToList();
            
            List<ExportExcelModel> exportList = new List<ExportExcelModel>();

            using (var db = new LibraDb())
            {
                foreach (var invoice in listInvoices)
                {
                    ExportExcelModel exportExcelModel = new ExportExcelModel
                    {
                        Number = invoice.Number,
                        PolicyNumber = invoice.PolicyNumber,
                        PolicyHolder = invoice.PolicyHolder,
                        Creator = invoice.Creator,
                        Login = db.Users.FirstOrDefault(x => x.Id == invoice.CreatorId).Username,
                        Premium = invoice.Premium,
                        Commission = invoice.Commission,
                        Product = invoice.Product,
                        Commissions = invoice.Commissions,
                        PaidDate = db.Payments.FirstOrDefault(x => x.InvoiceId == invoice.Number).PayDate
                    };

                    exportList.Add(exportExcelModel);
                }
            }

            string[] columns =
            {
                "PolicyNumber", "Number", "Product", "PolicyHolder", "Creator", "Login", "PaidDate",
                "Branch", "Premium", /*"CommissionValue",*/ "CommissionPercent",
                "CustomCommissionCustomPercent", "CustomCommissionCustomAmount"
            };

            Dictionary<string, Dictionary<string, string>> formattedColumns = new Dictionary<string, Dictionary<string, string>>
            {
                {"CommissionValue", new Dictionary<string, string> {{"Commission", "{Value}"}}},
                {"CommissionPercent", new Dictionary<string, string> {{"Commission", "{Percent}"}}},

                {"CustomCommissionCustomPercent", new Dictionary<string, string> {{"Commissions", "{CustomPercent}"}}},
                {"CustomCommissionCustomAmount", new Dictionary<string, string> {{"Commissions", "{CustomAmount}"}}}
            };
            //in future think  about how to do it like above
            
            Dictionary<string, string> columnsHeader = new Dictionary<string, string>
            {
                {"PolicyNumber", "Policy number"},
                {"Number", "Invoice number"},
                {"Product", "Product"},
                {"PolicyHolder", "Policy holder"},
                {"Creator", "Agent"},
                {"Login", "Login"},
                {"PaidDate", "Invoice paid date"},
                {"Branch", "Şöbə"},
                {"Premium", "Invoice Premium AZN"},
                //{ "CommissionValue", "Commission AZN"},
                {"CommissionPercent", "Commission %"},
                {"CustomCommissionCustomPercent", "Corrected Commission %"},
                {"CustomCommissionCustomAmount", "Corrected Commission AZN"}
            };

            return new OperationResult<byte[]>(ExportExcel<ExportExcelModel>((List<ExportExcelModel>)exportList, "", true, columnsHeader, formattedColumns, columns));
        }

        public OperationResult<byte[]> ExportToExcel(ActFilter filter)
        {
            var list = Load(filter).Model;
            string[] columns = {"Id", "StatusText", "CreateDate", "PayoutInfo", "Amount"};
            Dictionary<string, string> columnsHeader = new Dictionary<string, string>
            {
                {"Id", "Act number"},
                {"StatusText", "Status"},
                {"CreateDate", "Create date"},
                {"PayoutInfo", "Payout date"},
                {"Amount", "Amount"}
            };

            return new OperationResult<byte[]>(ExportExcel<ActListItemModel>((List<ActListItemModel>) list, "", true, columnsHeader, null, columns));
        }

        private DataTable ListToDataTable<T>(List<T> list, Dictionary<string, Dictionary<string, string>> formattedColumns = null)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable dataTable = new DataTable();

            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDescriptor property = properties[i];
                dataTable.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            object[] values = new object[properties.Count];
            foreach (T item in list)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = properties[i].GetValue(item);
                }

                dataTable.Rows.Add(values);
            }

            if (formattedColumns != null)
            {
                foreach (var item in formattedColumns)
                {
                    dataTable.Columns.Add(item.Key, typeof(string));

                    foreach (var it in item.Value)
                    {
                        for (int x = 0; x < dataTable.Rows.Count; x++)
                        {
                            if (!(dataTable.Rows[x][it.Key].GetType().IsPrimitive || dataTable.Rows[x][it.Key].GetType() == typeof(Decimal) ||
                                  dataTable.Rows[x][it.Key].GetType() == typeof(String)))
                            {
                                object val = dataTable.Rows[x][it.Key];
                                if (dataTable.Rows[x][it.Key].GetType().IsGenericType &&
                                    dataTable.Rows[x][it.Key].GetType().GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    //convert objecty to array. Because string.Format second parameter is  object array
                                    foreach (var comm in dataTable.Rows[x][it.Key] as IList)
                                    {
                                        val = comm;
                                    }
                                }

                                Regex regex = new Regex(@"(?<=\{)[^}]*(?=\})", RegexOptions.IgnoreCase);
                                MatchCollection matches = regex.Matches(it.Value);

                                string format = it.Value;
                                for (int i = 0; i < matches.Count; i++)
                                {
                                    var d = val.GetType().GetProperty(matches[i].Value).GetValue(val);
                                    format = format.Replace(matches[i].Value, d.ToString());
                                }

                                dataTable.Rows[x][item.Key] = format.Replace('{', ' ').Replace('}', ' ');
                            }
                            else
                            {
                                dataTable.Rows[x][item.Key] = string.Format(it.Value, dataTable.Rows[x][it.Key]);
                            }
                        }
                    }
                }
            }

            return dataTable;
        }

        private byte[] ExportExcel(DataTable dataTable, string heading = "", bool showSrNo = false, Dictionary<string, string> columnsHeader = null, params string[] columnsToTake)
        {
            byte[] result = null;
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(String.Format("{0} Data", heading));
                int startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;

                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);

                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;
                        index++;
                    }
                }

                //change columns header
                if (columnsHeader != null)
                {
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        column.Caption = columnsHeader.ContainsKey(column.ColumnName) ? columnsHeader[column.ColumnName] : column.Caption;
                    }
                }

                // add the content into the Excel file  
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);

                // autofit width of cells with small content  
                int columnIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];
                    int maxLength = columnCells.Max(cell => cell.Value != null ? cell.Value.ToString().Count() : 1);
                    if (maxLength < 150)
                    {
                        workSheet.Column(columnIndex).AutoFit();
                    }

                    if (column.DataType == typeof(DateTime))
                    {
                        const string FORMATDATE = "dd/MM/yyyy";

                        workSheet.Column(columnIndex).Style.Numberformat.Format = FORMATDATE;
                    }

                    columnIndex++;
                }

                // format header - bold, yellow on black  
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                    r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    r.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }

                // format cells - add borders  
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);

                    r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    r.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // removed ignored columns
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }

                    if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
                    {
                        workSheet.DeleteColumn(i + 1);
                    }
                }

                if (!String.IsNullOrEmpty(heading))
                {
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Size = 20;

                    workSheet.InsertColumn(1, 1);
                    workSheet.InsertRow(1, 1);
                    workSheet.Column(1).Width = 5;
                }

                dataTable.DefaultView.Sort = "# Asc";
                result = package.GetAsByteArray();
            }

            return result;
        }

        private byte[] ExportExcel<T>(List<T> data, string Heading = "", bool showSlno = false, Dictionary<string, string> columnsHeader = null,
            Dictionary<string, Dictionary<string, string>> formattedColumns = null, params string[] ColumnsToTake)
        {
            return ExportExcel(ListToDataTable<T>(data, formattedColumns), Heading, showSlno, columnsHeader, ColumnsToTake);
        }

        private static void SendEmail(string email, string actId)
        {
            EmailMessageModel emailModel = new EmailMessageModel
            {
                To = email,
                Body = $"{actId} is sent for your approval ... ",
                ObjectGuid = Guid.Empty,
                Subject = "Act Approve",
                SubType = QueueItemSubtype.EmailWithoutAttachment,
                SystemOid = 400,
                IsBodyHtml = true
            };
            
            string baseUrl = ConfigurationManager.AppSettings["IMSAPIBASEURL"];
            string queryProxyUser = ConfigurationManager.AppSettings["IMSAPIUSERNAME"];
            string queryProxyPassword = ConfigurationManager.AppSettings["IMSAPIPASSWORD"];

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("username", queryProxyUser);
            httpClient.DefaultRequestHeaders.Add("password", queryProxyPassword);

            var result = Task.Run(async () => await httpClient.PostJsonAsync<EmailMessageModel, string>
                ($"{baseUrl}/messaging/sendemail", emailModel)).Result;
        }

        class ExportExcelModel
        {
            public string Number { get; set; }
            public string PolicyNumber { get; set; }
            public string PolicyHolder { get; set; }
            public string Product { get; set; }
            public decimal Premium { get; set; }
            public DateTime PaidDate { get; set; }
            public int CreatorId { get; set; }
            public string Creator { get; set; }
            public string Login { get; set; }
            public string Branch { get; set; }
            public AmountModel Commission { get; set; }
            public ICollection<CommissionModel> Commissions { get; set; }
        }

        public OperationResult<ICollection<string>> GetActsWithPolicyNumber(ActFilter filter)
        {
            using (var db = new LibraDb())
            {
                var actNumbers = (from inv in db.Invoices
                    join actCom in db.ActCommissions
                        on inv.Id equals actCom.InvoiceId into actInfo
                    where inv.PolicyNumber == filter.PolicyNumber
                    from actCom in actInfo.DefaultIfEmpty()
                    select actCom.ActId).ToList();

                return new OperationResult<ICollection<string>>(actNumbers);
            }
        }

        public OperationResult<ICollection<string>> GetActsWithInvoiceNumber(ActFilter filter)
        {
            using (var db = new LibraDb())
            {
                var actNumbers = (from inv in db.Invoices
                    join actCom in db.ActCommissions
                        on inv.Id equals actCom.InvoiceId into actInfo
                    where inv.Id == filter.InvoiceNumber
                    from actCom in actInfo.DefaultIfEmpty()
                    select actCom.ActId).ToList();

                return new OperationResult<ICollection<string>>(actNumbers);
            }
        }

        public OperationResult<ActModel> CreateFromCommissionChange(PolicyInvoiceModel model, IUser user, int? customUserID = null)
        {
            if (customUserID == null && !IsTypeAvailable(model.Acttype, user))
            {
                return new OperationResult<ActModel>(translationProvider.GetString("ACT_TYPE_FORBIDDEN"), IssueSeverity.Error);
            }

            using (var db = new LibraDb())
            {
                //just for view , will delete before returning the method

                #region

                DbCommission dbCommission;
                if (model.Acttype != 0 && model.Amount != 0 && model.Payouttype != 0)
                {
                    //try to add new row to Commission table
                    //if record with act and payout type is already in db inform user
                    try
                    {
                        dbCommission = new DbCommission
                        {
                            ActType = (int) model.Acttype,
                            Amount = model.Amount,
                            InvoiceId = model.Invoice_number,
                            PayoutType = (int) model.Payouttype
                        };
                        db.Commissions.Add(dbCommission);
                        db.SaveChanges();
                    }
                    catch
                    {
                        return new OperationResult<ActModel>(translationProvider.GetString("COMMISSION_DUPLICATE_ERROR"), IssueSeverity.Error);
                    }
                }

                else
                {
                    return new OperationResult<ActModel>(translationProvider.GetString("ACT_MISSING_VALUE"), IssueSeverity.Error);
                }

                #endregion
                
                DbUser customUser = db.Users.Where(x => x.Id == customUserID)
                    .Include(i => i.Approvals).FirstOrDefault();

                var selectedInvoices = db
                    .InvoiceView
                    .Where(i => i.Id == model.Invoice_number)
                    .Include(i => i.Commissions)
                    .ToList();

                #region validation invoice payout type   and check same mediators invoices

                if (selectedInvoices.Select(x => x.Commissions.Where(u => u.ActType == 1).FirstOrDefault()?.PayoutType).Where(j => j != null)
                        .GroupBy(y => y).Count() > 1 ||
                    selectedInvoices.Select(x => x.Commissions.Where(u => u.ActType == 2).FirstOrDefault()?.PayoutType).Where(j => j != null)
                        .GroupBy(y => y).Count() > 1 ||
                    selectedInvoices.Select(x => x.Commissions.Where(u => u.ActType == 3).FirstOrDefault()?.PayoutType).Where(j => j != null)
                        .GroupBy(y => y).Count() > 1)
                {
                    return new OperationResult<ActModel>(translationProvider.GetString("DIFFERENT_PAYOUT_TYPE_FORBIDDEN"), IssueSeverity.Error);
                }

                #endregion
                
                int? groupId = null;
                groupId = getGroupIdContainsAllProducts(selectedInvoices);
                if (groupId == null)
                {
                    return new OperationResult<ActModel>(translationProvider.GetString("INVOICE_PRODUCT_TYPE"), IssueSeverity.Error);
                }

                var records = db
                    .InvoiceView
                    .Where(i => i.Id == model.Invoice_number)
                    .Include(i => i.Commissions)
                    .Select(i => new InvoiceProjection
                    {
                        Invoice = i,
                        Commissions = i.Commissions.Where(c => c.ActType == (int) model.Acttype)
                    }).AsEnumerable()
                    .Select(invoiceMapper.MapInvoiceModel)
                    .ToList();

                var commission = records.SelectMany(r => r.Commissions).Sum(c => c.CalculatedAmount);

                var result = new OperationResult<ActModel>(new ActModel
                {
                    Id = EMPTY_ID,
                    Status = ActStatus.Draft,
                    Type = model.Acttype,
                    StatusText = translationProvider.GetString(ActStatus.Draft),
                    CreatorId = customUserID == null ? user.Id : customUser.Id,
                    Creator = customUserID == null ? user.FullName : $"{customUser.Name} {customUser.Surname}",
                    ReceiverId = records.FirstOrDefault()?.CreatorId,
                    Receiver = records.FirstOrDefault()?.Creator,
                    CreateDate = DateTime.Now,
                    Commission = commission,
                    Brokers = GetBrokersList(),
                    GroupId = groupId,
                    IsManual = true,
                    PayoutType = model.Payouttype
                });


                if (customUserID == null)
                {
                    db.Users
                        .Where(u => user.Supervisors.Contains(u.Id))
                        .ToList()
                        .Select(u => new ApprovalModel
                        {
                            ApproverId = u.Id,
                            Approver = u.FullName()
                        })
                        .ToList()
                        .ForEach(result.Model.Approvals.Add);
                }
                else
                {
                    db.Users
                        .Where(u => customUser.Supervisors.Contains(u.Id.ToString()))
                        .ToList()
                        .Select(u => new ApprovalModel
                        {
                            ApproverId = u.Id,
                            Approver = u.FullName()
                        })
                        .ToList()
                        .ForEach(result.Model.Approvals.Add);
                }

                List<string> underwrittingApprovableProductList =
                    db.Lobs.Where(x => x.approvable).Select(x => ((Product) x.lob_oid).ToString()).ToList();

                if (records.Any(x => underwrittingApprovableProductList.Contains(x.Product)))
                {
                    result.Model.Approvals.Add(new ApprovalModel
                    {
                        ApproverId = null,
                        Approver = translationProvider.GetString("ACT_APPROVAL_UNDERWRITER")
                    });
                }

                records.ForEach(result.Model.Invoices.Add);

                result.Model.AutoApprove =
                    customUserID == null ? user.IsInRole(Role.ApprovedActCreator) : customUser.Role == (int) Role.ApprovedActCreator;
                result.Model.CustomCommissions =
                    customUserID == null ? user.IsInRole(Role.CustomActCreator) : customUser.Role == (int) Role.CustomActCreator;
                result.Model.InsurerID = groupId == 1 ? Insurer.ATESHGAHLIFE : Insurer.ATESHGAH;
                db.Commissions.Remove(dbCommission);
                db.SaveChanges();

                return result;
            }
        }
    }
}