import { libra, endpoints } from './../module'

class NavigationService implements INavigationService {
    private window: ng.IWindowService;
    private rootScope: ng.IRootScopeService;

    constructor(window: ng.IWindowService, rootScope: ng.IRootScopeService) {
        this.window = window;
        this.rootScope = rootScope;
    }

    redirect = (url?: string) => {
        this.rootScope.$broadcast("HttpStarted");

        if (!url) {
            url = this.getQueryParam("ReturnUrl");
        }
        if (!url) {
            url = endpoints.Home;
        }
        this.window.location.href = url;
    }

    post = (url: string, model: any, isIFrame: boolean) => {
        var doc = document;

        if (isIFrame) {
            var frame = $("<iframe>", {
                style: "display:none;"
            })
            .appendTo('body')[0];

            doc = (<any>frame).contentDocument;
        }

        var form = doc.createElement("form");
        form.setAttribute("method", "post");
        form.setAttribute("action", url);


        var hiddenField = doc.createElement("input");
        hiddenField.setAttribute("type", "hidden");
        hiddenField.setAttribute("name", "model");
        hiddenField.setAttribute("value", JSON.stringify(model));

        form.appendChild(hiddenField);
        doc.body.appendChild(form);
        form.submit();
    }

    private getQueryParam = (name: string): string => {
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(this.window.location.href);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    }
}

libra.service("navigationService", ["$window", "$rootScope", NavigationService])