import { libra } from "./../module";

class DateFilter {
     constructor() {
         return (date) => {
             if (!date) {
                 return "";
             }
             
             const d = new Date(date);
             const pad = (num, size) => {
                var s = "000000000" + num;
                return s.substr(s.length-size);
             }

             return `${pad(d.getDate(), 2)}.${pad(d.getMonth() + 1, 2)}.${pad(d.getFullYear(), 4)}`
         }
     }
     
}

libra.filter("libraDate", [
    DateFilter
])