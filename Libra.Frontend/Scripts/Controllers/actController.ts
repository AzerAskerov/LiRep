import { libra, endpoints } from './../module'

class ActController {
    constructor (scope: any, http: ng.IHttpService, navigation: INavigationService) {
        scope.model = {};
        scope.receivers = [];

        scope.init = function (model) {
            scope.model = model;
            var r = {};

            _.each(scope.model.invoices, (i: any) => {

                if (!r[i.creatorId]) {
                    scope.receivers.push({key: i.creatorId, value: i.creator});
                    r[i.creatorId] = true;
                }
            })
        };

        scope.showApprovals = () => {
            return scope.model.approvals
                && scope.model.approvals.length > 0
                && (!scope.model.autoApprove
                 || !scope.actForm.$valid 
                 || _.some(scope.model.invoices, (i: any) => _.some(i.commissions, (c: any) => c.customAmount !== c.calculatedAmount)));
        }

        scope.deleteInvoice = (item) => {
            var index = scope.model.invoices.indexOf(item);
            if (index > -1) {
                scope.model.invoices.splice(index, 1);
                scope.commissionChanged(null,item);
            }
        }

        scope.commissionChanged = (commission,invoice) => {
            scope.model.commission = _.sum(scope.model.invoices.map(i => _.sum(i.commissions.map(c => c.customAmount)))); 
            commission.customPercent =  commission.customAmount *100 / invoice.premium  ;

        }

        scope.commissionPercentChanged = (commission,invoice) => {
            commission.customAmount =  invoice.premium *  commission.customPercent/100; 
            scope.model.commission = _.sum(scope.model.invoices.map(i => _.sum(i.commissions.map(c => c.customAmount))));
        }

        scope.isCommissionChanged =  (item) => { 
            var result =   item.commissions.some(function (value) {
                 return value.totalAmount.value  != value.customAmount; 
                });   
             return result;
    };

    //check both manualCommissions and commissions sent from IMS
    scope.isPredefinedCommission =  (item) => {
        var result =   item.commissions.some(function (value) {
             return value.isManual; 
            });   
         return result || item.isCommissionFromIms;
};

        const save = (send: boolean) => {
            http.post(send ? endpoints.ActSend : endpoints.ActSave, scope.model)
                .then((result: any) => {
                    scope.model.issues = result.data.issues;

                    if (result.data.isSuccess) {
                        navigation.redirect(`${endpoints.ActView}/${result.data.model.id}`)
                    }
                });
        };

        scope.save = function() {
            save(false);
        }

        scope.send = function() {
            save(true);
        }

        scope.approve = function() {
             http.post(`${endpoints.ActApprove}/${scope.model.id}`, {})
                .then((result: any) => {
                    scope.model.issues = result.data.issues;
                    if (result.data.isSuccess) {
                        scope.model = result.data.model;
                    }
                });           
        } 
        
        scope.rejectActDialog = () => {
            (<any>$("#rejectActDialog")).modal("show");
        }


        scope.reject = function() {
            http.post(endpoints.ActReject, scope.model) 
                .then((result: any) => {
                    scope.model.issues = result.data.issues;
                    if (result.data.isSuccess) {
                        scope.model = result.data.model;
                    }
                    (<any>$("#rejectActDialog")).modal("hide");
                });           
        }

        scope.cancel = function() {
             http.post(`${endpoints.ActCancel}/${scope.model.id}`, {})
                .then((result: any) => {
                    scope.model.issues = result.data.issues;
                    if (result.data.isSuccess) {
                        scope.model = result.data.model;
                    }
                });           
        }

        scope.canApprove = function(userId, isUnderwriter) {
            if (scope.model.status !== 1) {
                return false;
            }
            
            var userApproval = scope.model.approvals.find(a => a.approverId === userId);
            var underwriterApproval = scope.model.approvals.find(a => a.approverId == null);
            return userApproval && !userApproval.isApproved 
                || isUnderwriter && underwriterApproval && !underwriterApproval.isApproved;
        }
        scope.canReject = function(userId, isUnderwriter) {
            if (scope.model.status !== 1 && scope.model.status !== 4) {
                return false;
            }
            var userApproval = scope.model.approvals.find(a => a.approverId === userId);
            var underwriterApproval = scope.model.approvals.find(a => a.approverId == null);
            return userApproval && (userApproval.isApproved || userApproval.isApproved === null)
                || isUnderwriter && underwriterApproval && underwriterApproval.isApproved === null;            
        }
    }
}

libra.controller("actController", ["$scope", "$http", "navigationService", ActController])
