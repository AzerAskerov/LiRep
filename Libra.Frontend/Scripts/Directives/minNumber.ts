import { libra } from "./../module";

class MinNumberDirective implements ng.IDirective {
    constructor() {
        return {
            require: "ngModel",
            link(scope: ng.IScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes, controller: ng.INgModelController) {

                const minNumber = attributes["libraMinNumber"] as string;

                let minValue = undefined;

                if (_.isNumber(minNumber)) {
                    minValue = parseInt(minNumber);
                } else {
                    scope.$watch(minNumber, (value) => {
                        minValue = value;
                        controller.$validate();
                    });
                }
                
                controller.$validators["libraMinNumber"] = (modelValue: string, viewValue: string) => {
                    if (controller.$isEmpty(viewValue)) {
                        return true;
                    }

                    if (!_.isNumber(minValue) || !_.isNumber(modelValue)) {
                        return true;
                    }

                    return parseInt(modelValue) >= minValue;
                };
            }
        }
    }
}

libra.directive("libraMinNumber", [
    MinNumberDirective
]);