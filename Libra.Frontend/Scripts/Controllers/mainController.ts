import { libra } from './../module'

class MainController {
    constructor (scope: any, rootScope: ng.IRootScopeService, window: ng.IWindowService) {
        
        scope.httpInProgress = false;

        window.onbeforeunload = () => {
            scope.httpInProgress = true;
            scope.$apply();
        };

        rootScope.$on("HttpStarted", () => {
           scope.httpInProgress = true; 
        });

        rootScope.$on("HttpCompleted", () => {
           scope.httpInProgress = false; 
        });
    }
}

libra.controller("mainController", ["$scope", "$rootScope", "$window", MainController])