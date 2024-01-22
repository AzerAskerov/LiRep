import { libra } from "./../module";

class SelectDirective implements ng.IDirective {
    constructor() {
        return {
            replace: true,
            require: "ngModel",
            restrict: "E",
            scope: {
                items: "=items"
            },
            template: require("html!../../Templates/Select.html")
        }
    }
}

libra.directive("libraSelect", [
    SelectDirective
]);