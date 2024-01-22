import { libra } from "./../module";

class OnPressEnterDirective implements ng.IDirective {
    constructor() {
        return (scope: ng.IScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) => {
            element.bind("keydown keypress", event => {
                if (event.which === 13) {
                    scope.$apply(() => {
                        scope.$eval(attributes["libraOnPressEnter"]);
                        $(event.target).blur();
                    });

                    event.preventDefault();
                }
            });
        }
    }
}

libra.directive("libraOnPressEnter", [
    OnPressEnterDirective
]);