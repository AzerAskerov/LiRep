import { libra, endpoints } from './../module'

class AccountLoginController {
    constructor (scope: any, http: ng.IHttpService, navigation: INavigationService) {
        
        scope.model = {};

        scope.login = () => {
            http.post(endpoints.Login, scope.model)
                .then((result: any) => {
                    scope.model.issues = result.data.issues;
                    if (result.data.isSuccess) {
                        navigation.redirect();
                    } 
                });
        };
    }
}

libra.controller("accountLoginController", ["$scope", "$http", "navigationService", AccountLoginController])