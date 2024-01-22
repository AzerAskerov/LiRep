import { libra, endpoints } from './../module'


class CommissionListController {

    constructor (scope: any, http: ng.IHttpService, navigation: INavigationService) {
        scope.model = [];
        scope.issues = [];
        scope.classifiers = {};

        scope.sort = function(keyname){
            scope.sortKey = keyname;   //set the sortKey to the param passed
            scope.reverse = !scope.reverse; //if true make it false and vice versa
        }

        const setCommissionsTouched = () => {
            _.each(scope.commissionForm.$error, (field) => {
                _.each(field, (errorField) => {
                    errorField.$setTouched();
                })
            });
        }

        const filterCommissions = () => {

            scope.bakFilter = _.assign({}, scope.filter);
            scope.showFormWarning = false;
                       
            _.filter(
                scope.model, 
                (c:any)=> c.validFrom.toString().indexOf('.') !== -1?  c.validFrom = new Date(c.validFrom.split(".").reverse().join("-") + "T00:00:00"):c.validFrom
                        
            );

            _.filter(
                scope.model, 
                (c:any)=> c.validTo.toString().indexOf('.') !== -1? c.validTo = new Date( c.validTo.split(".").reverse().join("-") + "T00:00:00"): c.validTo                        
            );


            scope.commissions = _.filter(
                scope.model, 
                (c:any) => c.actType == scope.filter.actType
                        && (!scope.filter.product || c.product == scope.filter.product)
                        && (!scope.filter.brand || c.brand == scope.filter.brand)
                        && (!scope.filter.policyHolderType || c.policyHolderType == scope.filter.policyHolderType)
                        && (!scope.filter.payoutType || c.payoutType == scope.filter.payoutType)
                        && (!scope.filter.agent || c.username && c.username.toUpperCase() == scope.filter.agent.toUpperCase())
                        && (!scope.filter.beneficiary || c.beneficiaryCode && c.beneficiaryCode.toUpperCase() == scope.filter.beneficiary.toUpperCase())
                        && (!scope.filter.validFrom || new Date(c.validFrom) >= new Date(scope.filter.validFrom.split(".").reverse().join('/')))
                        && (!scope.filter.validTo || new Date(c.validTo) <= new Date(scope.filter.validTo.split(".").reverse().join('/')))
                        && (!scope.filter.vehicleType || c.vehicleType == scope.filter.vehicleType)
                        );
        }

        scope.init = function (model) {
            scope.model = model;
            scope.filter = {
                actType: 1,
                product: null,
                brand: null,
                policyHolderType: null,
                payoutType: null,
                agent: "",
                beneficiary: "",
                validFrom: "",
                validTo: "",
                vehicleType: null,
                engineCapacityFrom: "",
                engineCapacityTo: ""
            };

            filterCommissions();
        };

        scope.reload = () => {

            if (scope.commissionForm.$valid) {
               // filterCommissions();

               //For checking Vehicle type is 'Passenger' (3) . If it is not clear filters
               if(scope.filter.vehicleType != 3) {
                    scope.filter.engineCapacityFrom = "";
                    scope.filter.engineCapacityTo = "";
               }

               http.post(endpoints.CommissionList, scope.filter)
               .then((result: any) => {
                scope.commissions = result.data.model;
               });
            } else {
                scope.issues = [];
                scope.showFormWarning = true;
                setCommissionsTouched();
                scope.filter = _.assign({}, scope.bakFilter);
            }

        };

        scope.waitAndReload = _.debounce(() => {

            scope.reload();
            scope.$apply();

        }, 800);

        scope.deleteConfirm = function (item) {
            if (window.confirm("Komissiyanı silmək istədiyinizə əminsiniz?")) {
                scope.deleteCommission(item);
            } else {
                return;
            }
        }

        scope.updateConfirm = function(item){
            if(window.confirm("Komissiyanı dəyişdirmək istədiyinizə əminsiniz?")){
                scope.updateCommission(item);
            } else{
                return;
            }
        }

        scope.addCommission = () => {
            scope.model.push({
                actType: scope.filter.actType,
                payoutType: scope.filter.payoutType ? scope.filter.payoutType : 1,
                product: scope.filter.product,
                brand: scope.filter.brand,
                policyHolderType: scope.filter.policyHolderType,
                username: scope.filter.agent,
                beneficiaryCode: scope.filter.beneficiary,
                validFrom : scope.filter.validFrom,
                validTo : scope.filter.validTo
            });
            filterCommissions();
        }

        scope.deleteCommission = (item) => {
            
            http.post(endpoints.CommissionDelete,item /*scope.model*/)
            .then((result: any) => {
                scope.showFormWarning = false;
                scope.issues = result.data.issues;

                if (result.data.isSuccess || item.Id) {
                    var index = scope.commissions.indexOf(item);

            if (index > -1) {
                scope.commissions.splice(index, 1);
            }

            index = scope.model.indexOf(item);

            if (index > -1) {
                scope.model.splice(index, 1);
            }

                    scope.commissionForm.$setPristine();
                    scope.commissionForm.$setUntouched();
                    scope.commissionForm.$setDirty();

                }
            });
        }

        scope.updateCommission = (item) => {

            http.post(endpoints.CommissionUpdate,item /*scope.model*/)
            .then((result: any) => {
                scope.showFormWarning = false;
                scope.issues = result.data.issues;

                if (result.data.isSuccess) {
                   
                    var index = scope.commissions.indexOf(item);
                   
        
                    index = scope.model.indexOf(item);
                    if (index > -1) {
                        scope.model.splice(index, 1);
                    }
        
                    scope.commissionForm.$setDirty();
                }
            });
        }

        scope.engineCapacityVisibility = () => {
            //Engine Capacity filter option must be visible only when Vehicle type is 'Passenger' (3)
            return scope.filter.vehicleType == 3
        }

        scope.isProductCmtpl = (product) => {
            return product == 45;
        }

        scope.isVehicleTypePassenger = (vehicleType) => {
            return vehicleType == 3;
        }

        scope.productSelected = (commission) => {
            if(commission.product != 45) {
                commission.vehicleType = null;
                commission.engineCapacityFrom = null;
                commission.engineCapacityTo = null;
            }
        }

        scope.vehicleTypeSelected = (commission) => {
            if(commission.vehicleType != 3) {
                commission.engineCapacityFrom = null;
                commission.engineCapacityTo = null;
            }
        }
    }
}

libra.controller("commissionListController", ["$scope", "$http", "navigationService", CommissionListController])