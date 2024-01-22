import { libra } from "./../module";

class MaxNumberDirective implements ng.IDirective {
    constructor() {
        return {
            require: "ngModel",
            link(scope: ng.IScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes, controller: ng.INgModelController) {

                const maxNumber = attributes["libraMaxNumber"] as string;

                let maxValue = undefined;

                if (_.isNumber(maxNumber)) {
                    maxValue = parseFloat(maxNumber);
                } else {
                    scope.$watch(maxNumber, (value) => {
                        maxValue = value;
                        controller.$validate();
                    });
                }
                
                controller.$validators["libraMaxNumber"] = (modelValue: string, viewValue: string) => {
                    if (controller.$isEmpty(viewValue)) {
                        return true;
                    }

                    if (!_.isNumber(maxValue) || !_.isNumber(modelValue)) {
                        return true;
                    }

                    return parseFloat(modelValue) <= maxValue;
                };
            }
        }
    }
}

libra.directive("libraMaxNumber", [
    MaxNumberDirective
]);