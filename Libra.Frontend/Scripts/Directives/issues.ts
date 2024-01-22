import { libra } from "./../module";

class IssuesDirective implements ng.IDirective {
    constructor() {
        return {
            replace: true,
            restrict: "E",
            scope: {
                issues: "=source"
            },
            template: require("html!../../Templates/Issues.html")
        }
    }
}

libra.directive("libraIssues", [
    IssuesDirective
]);