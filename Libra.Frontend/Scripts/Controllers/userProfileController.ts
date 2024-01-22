import { libra, endpoints } from './../module'

class UserProfileController {

    constructor (scope: any, http: ng.IHttpService, navigation: INavigationService) {
        scope.model = {};

        scope.save = () => {
            http.post(endpoints.UserSave, scope.model)
                .then((result: any) => {
                    scope.model.issues = result.data.issues;
                });
        }
    }
}

libra.controller("userProfileController", ["$scope", "$http", "navigationService", UserProfileController])
