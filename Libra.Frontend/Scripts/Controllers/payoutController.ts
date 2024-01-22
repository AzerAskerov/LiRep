import { libra, endpoints } from './../module'

class PayoutController {
    constructor (scope: any, http: ng.IHttpService, navigation: INavigationService, settings: ISettings) {
        scope.model = {};

        scope.sort = function(keyname){
            scope.sortKey = keyname;   //set the sortKey to the param passed
            scope.reverse = !scope.reverse; //if true make it false and vice versa
        }

        scope.init = function () {

            scope.filter = {
                status: null,
                payoutDateFrom: "",
                payoutDateTill: "",
                creator: ""
            };

            scope.reload();
        };

        scope.reload = () => {
            http.post(endpoints.PayoutList, scope.filter)
                .then((result: any) => {
                    scope.model.payouts = result.data.model;
                });
        };

        scope.policyFilterChanged = () => {
            http.post(endpoints.GetPolicyFilterListPayout, scope.filter)
                .then((result: any) => {
                    scope.filter.ListActNumber = result.data.model;
                    http.post(endpoints.PayoutList, scope.filter)
                    .then((result: any) => {
                        scope.model.payouts = result.data.model;
                    });
                });
        };

        scope.waitAndReload = _.debounce(scope.reload, 800);

        scope.init = function () {
            http.post(endpoints.PayoutList, {})
                .then((result: any) => {
                    scope.model.payouts = result.data.model;
                });
        };

        scope.select = function(payout) {
            _.each(scope.model.payouts, (p: any) => {
                p.selected = false;
            });
            payout.selected = true;
            scope.activePayout = payout;
        }

        scope.payDialog = () => {
            scope.payDate = settings.today;
            (<any>$("#payDialog")).modal("show");
        }

        scope.pay = () => {
            (<any>$("#payDialog")).modal("hide");
            var data = <any>_.assign({}, scope.activePayout);
            data.payDate = scope.payDate;

            http.post(endpoints.PayoutPay, data)
                .then((result: any) => {
                    scope.model.issues = result.data.issues;

                    if (result.data.isSuccess) {
                        scope.init();
                    }
                });
        }

        scope.exportDialog = () => {
            (<any>$("#exportDialog")).modal("show");
        }

        scope.export = () => {
            http.get(endpoints.PayoutUnpaidBankPayments)
                .then((result: any) => {
                    scope.model.issues = result.data.issues;

                    if (result.data.isSuccess) {
                        scope.bankPayouts = result.data.model;
                        navigation.post(endpoints.PayoutExportFile, scope.bankPayouts, true);
                    } else {
                        (<any>$("#exportDialog")).modal("hide");
                    }
                });   
        }

        scope.payBank = () => {
             (<any>$("#exportDialog")).modal("hide");

            http.post(endpoints.PayoutPayBank, scope.activePayout)
                .then((result: any) => {
                    scope.model.issues = result.data.issues;
                    scope.bankPayouts = [];

                    if (result.data.isSuccess) {
                        scope.init();
                    }
                });           
        }

        scope.payEnabled = function() {
            return scope.activePayout && scope.activePayout.type != 1 && !scope.activePayout.payDate;            
        }
    }    
}

libra.controller("payoutController", ["$scope", "$http", "navigationService", "settings", PayoutController])