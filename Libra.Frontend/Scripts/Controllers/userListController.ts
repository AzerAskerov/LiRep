import { libra, endpoints } from './../module'

class UserListController {
    constructor (scope: any, http: ng.IHttpService, navigation: INavigationService) {
        scope.model = {};

        scope.init = function () {
            http.post(endpoints.UserList, {})
                .then((result: any) => {
                    scope.model.users = result.data.model;
                });
        };

        scope.open = function(user) {
            navigation.redirect(`${endpoints.UserProfile}/${user.username}`);
        }
    }    
}

libra.controller("userListController", ["$scope", "$http", "navigationService", UserListController])