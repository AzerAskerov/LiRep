import { libra, endpoints } from './../module';
import * as XLSX from 'ts-xlsx';



class RecalculateInvoiceListController {
    private loadEndpoint;
    private createEndpoint;
    private hideEndpoint;
    private selectedFile = null;

    constructor (scope: any, http: ng.IHttpService, navigation: INavigationService) {
        scope.model = {};
        scope.loadEndpoint = endpoints.RecalculateLoad;
        scope.processEndpoint = endpoints.RecalculateProcess;
        
        scope.selectFile = function (file) {
            scope.selectedFile = file;
        };


        scope.processExcel = function (data) {
            //Read the Excel File data.
            var workbook = XLSX.read(data, {
                type: 'binary'
            });

            //Fetch the name of First Sheet.
            var firstSheet = workbook.SheetNames[0];

            //Read all rows from First Sheet into an JSON array.
            var excelRows = XLSX.utils.sheet_to_json(workbook.Sheets[firstSheet]);

            if (excelRows.length > 0) {
                // Save excel data to database.
                http.post(scope.processEndpoint, { invoices: excelRows })
                .then((result: any) => {
                    scope.model.issues = result.data.issues;
                    
                    if (result.data.isSuccess) {
                    scope.model.invoices = result.data.model;
                    }
                });
            }
        };


        scope.import = function () {
            var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.xls|.xlsx)$/;
            if (regex.test(scope.selectedFile.name.toLowerCase())) {
                if (typeof (FileReader) != "undefined") {
                    var reader = new FileReader();
                    //For Browsers other than IE.
                    if (reader.readAsBinaryString) {
                        reader.onload = function (e) {
                            scope.processExcel(e.target["result"]);
                        };
                        reader.readAsBinaryString(scope.selectedFile);
                    } 
                } else {
                    window.alert("This browser does not support HTML5.");
                }
            } else {
                window.alert("Please upload a valid Excel file.");
            }
        }
    }
}

libra.controller("recalculateInvoiceListController", ["$scope", "$http", "navigationService",  RecalculateInvoiceListController])