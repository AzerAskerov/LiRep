import { libra, endpoints } from './../module'

class ActApprovalsController {
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
            http.post(endpoints.ActApprovals, scope.filter)
                .then((result: any) => {
                    scope.model.acts = result.data.model;
                });
        };

        scope.policyFilterChanged = () => {
            http.post(endpoints.GetPolicyFilterList, scope.filter)
                .then((result: any) => {
                    scope.filter.ListActNumber = result.data.model;
                    http.post(endpoints.ActApprovals, scope.filter)
                    .then((result: any) => {
                        scope.model.acts = result.data.model;
                    });
                });
        };

        scope.waitAndReload = _.debounce(scope.reload, 800);
        scope.init = function () {
            http.post(endpoints.ActApprovals, {})
                .then((result: any) => {
                    scope.model.acts = result.data.model;
                });
        };

        scope.open = function(act) {
            navigation.redirect(`${endpoints.ActView}/${act.id}`);
        }
    }    
}

libra.controller("actApprovalsController", ["$scope", "$http", "navigationService", ActApprovalsController])