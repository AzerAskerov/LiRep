import { libra } from "./../module";

class CommissionFilter {
    constructor(numberFilter, moneyFilter) {
        return amount => {
            if (!amount) {
                return "-";
            }

            return `${moneyFilter(amount.value)} (${amount.percent ? numberFilter(amount.percent, 1) : "0.0"} %)`;
        };
    }
}

libra.filter("libraCommission", [
    "numberFilter",
    "libraMoneyFilter",
    CommissionFilter
]);