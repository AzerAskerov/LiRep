import { libra, endpoints } from './../module'

class ActListController {
    constructor (scope: any, http: ng.IHttpService, navigation: INavigationService) {
        scope.model = {};

        scope.sort = function(keyname){
            scope.sortKey = keyname;   //set the sortKey to the param passed
            scope.reverse = !scope.reverse; //if true make it false and vice versa
        }

        scope.init = function () {

            scope.filter = {
                status: null,
                receiver: "",
                creator: "",
                createDateFrom: "",
                createDateTill: "",
                payoutDateFrom: "",
                payoutDateTill: ""
            };

            scope.reload();
        };

        scope.reload = () => {
            http.post(endpoints.ActList, scope.filter)
                .then((result: any) => {
                    scope.model.acts = result.data.model;
                });
        };

        scope.policyFilterChanged = () => {
            http.post(endpoints.GetPolicyFilterList, scope.filter)
                .then((result: any) => {
                    scope.filter.ListActNumber = result.data.model;

                    http.post(endpoints.ActList, scope.filter)
                    .then((result: any) => {
                        scope.model.acts = result.data.model;
                    });
                });
        };

        scope.invoiceFilterChanged = () => {
            http.post(endpoints.GetInvoiceFilterList, scope.filter)
                .then((result: any) => {
                    scope.filter.ListActNumber = result.data.model;

                    http.post(endpoints.ActList, scope.filter)
                    .then((result: any) => {
                        scope.model.acts = result.data.model;
                    });
                });
        };

        scope.waitAndReload = _.debounce(scope.reload, 800);

        scope.open = function(act) {
            navigation.redirect(`${endpoints.ActView}/${act.id}`);
        }

        scope.exportExcel = () => {
            http.post(endpoints.ActExportExcel, scope.filter)
            .then((result: any) => {
                window.open(endpoints.ActDownloadExcel + '?fileGuid=' + result.data.FileGuid);
            });;
        }
    }    
}

libra.controller("actListController", ["$scope", "$http", "navigationService", ActListController])