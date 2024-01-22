import { libra } from "./../module";

class MoneyFilter {
    constructor(numberFilter) {
        return value => {
            return (value ? numberFilter(value, 2) : value === 0 ? "0.00" : "-" ) + " ₼";
        };
    }
}

libra.filter("libraMoney", [
    "numberFilter",
    MoneyFilter
]);