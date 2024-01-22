import { libra } from "./../module";

class DateFormatDirective implements ng.IDirective {
    constructor() {
        return {
            require: "ngModel",
            link(scope: ng.IScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes, controller: ng.INgModelController) {

                controller.$formatters.unshift(function (modelValue) {
                    
                   
                    if (!modelValue) {
                        return "";
                    }
                    
                    const d = new Date(modelValue);
                    const pad = (num, size) => {
                       var s = "000000000" + num;
                       return s.substr(s.length-size);
                    }
       
                    return `${pad(d.getDate(), 2)}.${pad(d.getMonth() + 1, 2)}.${pad(d.getFullYear(), 4)}`
                });
                
            }
        }
    }
}

libra.directive("libraDateFormat", [
    DateFormatDirective
]);