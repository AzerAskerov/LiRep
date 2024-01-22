/// <reference path="../Typings/libra.d.ts" />

export const endpoints = {
    Home: "/",
    Login: "/Account/Login",
    InvoicesSync: "/Invoice/Syncronize",
    InvoicesLoad: "/Invoice/Load",
    InvoicesHide: "/Invoice/HideInvoices",
    InvoicesLoadSecondary: "/Invoice/LoadSecondary",
    ActCreate: "/Act/Create",
    ActCreateSecondary: "/Act/CreateSecondary",
    ActAdd: "/Act/Add",
    ActAddSecondary: "/Act/AddSecondary",    
    ActPreview: "/Act/Preview",
    ActView: "/Act/View",
    ActSave: "/Act/Save",
    ActSend: "/Act/Send",
    ActList: "/Act/List",
    GetPolicyFilterList: "/Act/GetPolicyFilterList",
    GetInvoiceFilterList: "/Act/GetInvoiceFilterList",
    GetPolicyFilterListPayout: "/Payout/GetPolicyFilterList",
    ActExportExcel: "/Act/ExportExcel",
    ActDownloadExcel: "/Act/Download",
    ActApprovals: "/Act/Approvals",
    PayoutList: "/Payout/List",
    PayoutPay: "/Payout/Pay",
    PayoutPayBank: "/Payout/PayBank",
    PayoutUnpaidBankPayments: "/Payout/UnpaidBankPayments",
    PayoutExportFile: "/Payout/ExportFile",
    ActReject: "/Act/Reject",
    ActApprove: "/Act/Approve",
    ActCancel: "/Act/Cancel",
    ActApproved: "/Act/Approved",
    UserList: "/User/List",
    UserProfile: "/User/Profile",
    UserSave: "/User/Save",
    CommissionList: "/Commission/List",
    CommissionFilteredList: "/Commission/LoadList",
    CommissionSave: "/Commission/Save",
    CommissionDelete: "/Commission/Delete",
    CommissionUpdate: "/Commission/Update",
    RecalculateLoad: "/Recalculate/List",
    RecalculateProcess: "/Recalculate/ProcessInvoices",
    GetPolicy:"/ComissionChange/Load",
    ComissionChangeSave:"/ComissionChange/SaveCommissionChanges",
    ComissionChangeIndex:"/ComissionChange/Index",
    CreateActFromComissionChange:"/Act/CreateFromCommissionChange",
    CheckInvoice:"/ComissionChange/CheckInvoice"
};

export const libra = angular
    .module("libra", [
        'angularUtils.directives.dirPagination','datetime', 'ngFileUpload'
    ])
    .value("settings", (<any>window).settings)
    .config(["$httpProvider", (httpProvider: ng.IHttpProvider) => {

        httpProvider.interceptors.push([
            "$rootScope",
            "$q",
            (rootScope: ng.IRootScopeService, q: ng.IQService, navigation: INavigationService) => {

                var requestCount = 0;

                return {
                    request: (request) => {

                        if (requestCount === 0) {
                            rootScope.$broadcast("HttpStarted");
                        }

                        requestCount = requestCount + 1;
                        return request || q.when(request);
                    },
                    responseError: (response: ng.IHttpPromiseCallbackArg<any>) => {

                        if (response.status === 401) {
                            navigation.redirect(endpoints.Login);
                        }

                        requestCount = requestCount - 1;
                        if (requestCount === 0) {
                            rootScope.$broadcast("HttpCompleted");
                        }

                        return q.reject(response);
                    },
                    response: (response) => {

                        requestCount = requestCount - 1;
                        if (requestCount === 0) {
                            rootScope.$broadcast("HttpCompleted");
                        }

                        return response || q.when(response);
                    }
                }
            }
        ]);


    }]);