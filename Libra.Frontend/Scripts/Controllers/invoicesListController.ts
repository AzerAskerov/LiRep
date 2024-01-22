import { libra, endpoints } from './../module'

class InvoicesListController {
    private loadEndpoint;
    private createEndpoint;
    private hideEndpoint;

    constructor (scope: any, http: ng.IHttpService, navigation: INavigationService) {
        scope.model = {};
        scope.model.selectedInvoices = [];
        scope.loadEndpoint = endpoints.InvoicesLoad;
        scope.createEndpoint = endpoints.ActCreate;
        scope.hideEndpoint = endpoints.InvoicesHide;
        scope.addEndpoint = endpoints.ActAdd;

        scope.sort = function (keyname) {
            scope.sortKey = keyname;   //set the sortKey to the param passed
            scope.reverse = !scope.reverse; //if true make it false and vice versa
        }

        scope.init = function (isLegal: boolean) {

            scope.filter = {
                status: 1,
                createDateFrom: "",
                createDateTill: "",
                payDateFrom: "",
                payDateTill: ""
            };

            if (isLegal) {
                scope.loadEndpoint = endpoints.InvoicesLoadSecondary;
                scope.createEndpoint = endpoints.ActCreateSecondary;
                scope.addEndpoint = endpoints.ActAddSecondary;
            }

            scope.reload();
        };

        scope.reload = function (newPage) {
            scope.filter.pageNumber = newPage;

            if(newPage === undefined) {
                scope.model.selectedInvoices = [];
            }

            http.post(scope.loadEndpoint, scope.filter)
                .then((result: any) => {
                    scope.model.invoices = result.data.model.invoicesModel;
                    scope.model.itemsCount = result.data.model.itemsCount;
                });
        };

        scope.waitAndReload = _.debounce(scope.reload, 800);

        scope.syncronize = () => {
            http.post(endpoints.InvoicesSync, {})
                .then((result: any) => {
                    scope.model.issues = result.data.issues;

                    if (result.data.isSuccess) {
                        scope.init();
                    }
                });
        };

        scope.selectInvoice = (invoice) => {
            if (!scope.isSelected(invoice)) {
                scope.model.selectedInvoices.push(invoice);
            }
            else {
                scope.deSelectInvoice(invoice)
            }
        }

        scope.deSelectInvoice = (invoice) => {
            const getInvoice = scope.model.selectedInvoices
                .map((i, index) => (i.number === invoice.number) ? index : null)
                .filter(data => data !== null);
                
            const index = getInvoice[0]
            
            scope.model.selectedInvoices.splice(index, 1);
        }

        scope.isSelected = (invoice) => {
            return scope.model.selectedInvoices.some(i => i.number === invoice.number);
        }

        scope.allSelected = () => {
            console.log(scope.model.invoices.every(e => scope.model.selectedInvoices.some(i => i.number === e.number)))
            return scope.model.invoices.every(e => scope.model.selectedInvoices.some(i => i.number === e.number))
        }

        scope.selectAll = () => {
            if(!scope.allSelected()) {
                _.each(scope.model.invoices, (i: any) => {
                    if(!scope.isSelected(i))
                    scope.model.selectedInvoices.push(i);
                })
            }
            else {
                _.each(scope.model.invoices, (i: any) => {
                    scope.deSelectInvoice(i);
                })
            }
        }

        scope.createAct = () => {
            http.post(scope.createEndpoint, scope.model.selectedInvoices.map(i => i.number))
                .then((result: any) => {
                    scope.model.issues = result.data.issues;

                    if (result.data.isSuccess) {
                        navigation.post(endpoints.ActPreview, result.data.model);
                    }
                });
        }

        scope.hideInvoicesDialog = () => {
            (<any>$("#hideInvoicesDialog")).modal("show");
        }

        scope.hideSelectedInvoices = () => {
            (<any>$("#hideInvoicesDialog")).modal("hide");

            http.post(scope.hideEndpoint, scope.model.selectedInvoices.map(i => i.number))
                .then((result: any) => {
                    scope.model.issues = result.data.issues;

                    if (result.data.isSuccess) {
                        scope.reload();
                    }
                });
        }

        scope.addToActDialog = () => {
            (<any>$("#addToActDialog")).modal("show");
        }

        scope.addToAct = () => {
            (<any>$("#addToActDialog")).modal("hide");

            http.post(`${scope.addEndpoint}/${scope.actId}`, scope.model.selectedInvoices.map(i => i.number))
                .then((result: any) => {
                    scope.model.issues = result.data.issues;

                    if (result.data.isSuccess) {
                        navigation.redirect(`${endpoints.ActView}/${scope.actId}`);
                    }
                });
        }

        scope.anyInvoiceSelected = () => {
            return scope.model.selectedInvoices.length !== 0 ? true : false;
        }
    }
}

libra.controller("invoicesListController", ["$scope", "$http", "navigationService", InvoicesListController])