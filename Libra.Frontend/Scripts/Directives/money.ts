import { libra } from "./../module";

class MoneyDirective implements ng.IDirective {
    constructor(numberFilter: ng.IFilterNumber) {
        return {
            require: "ngModel",
            restrict: "A",
            link: (scope, element, attributes, controller: ng.INgModelController) => {
                
                element.addClass("input-number");

                controller.$parsers.push((viewValue: any) => {

                    if (_.isEmpty(viewValue)) {
                        return null;
                    }

                    if (isNaN(<number>viewValue)) {
                        return undefined;
                    }

                    return parseFloat(viewValue);
                });

                controller.$formatters.push((value: number) => {
                    return value ? numberFilter(value, 2) : value === 0 ? "0.00" : "";
                });

            }
        }
    }
}

libra.directive("libraMoney", ["numberFilter", MoneyDirective]);