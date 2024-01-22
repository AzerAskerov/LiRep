import { libra, endpoints } from './../module'

 class ComissionChangeController {

    private loadEndpoint;
    private createEndpoint;
    private indexEndpoint;

constructor(scope: any, http: ng.IHttpService, navigation: INavigationService){

    scope.loadEndpoint = endpoints.GetPolicy;
    scope.createEndpoint = endpoints.ComissionChangeSave;
    scope.indexEndpoint = endpoints.ComissionChangeIndex;
 

    scope.policies=[];

    scope.model ={};

    scope.actModel = {};

    scope.result = {};

    scope.invoices = [];



    scope.init = () =>
    {

    }

    scope.reload = () => {

        var policy_number = scope.model.policy_number;
                      scope.policies.length=0;

        http.post(scope.loadEndpoint,{policy_number:policy_number} )
            .then((result: any) => {
               if(result.data.model.length!==0)
               {
                   
                scope.model.start_date = result.data.model[0].start_date;
                scope.model.end_date = result.data.model[0].end_date;
                scope.model.client_name = result.data.model[0].client_name;
                scope.model.lob_oid = result.data.model[0].lob_oid;
                scope.model.amount_premium = result.data.model[0].amount_premium;
                scope.model.client_code = result.data.model[0].client_code;
                scope.model.create_time = result.data.model[0].create_time;
                scope.model.amount_paid = result.data.model[0].amount_paid;
                scope.model.com = result.data.model[0].com;


                scope.policies = result.data.model;
                console.log(scope.policies);
                console.log(scope.model);
               }

               else{
                      scope.model = {};
                      scope.policies.length=0;

                      console.log(scope.policies);
                      console.log(scope.model);
               }
            });

    };





    scope.createAct =  (policy) =>
     {

        http.post( endpoints.CheckInvoice,{invoice_number:policy.invoice_number}  )
        .then((result: any) => {
    
           if(result.data.model== null)
           {
    
            document.getElementById("msg").style.display = "block";
            document.getElementById("msg").innerText = "This invoice doesn't exist in Libra";
    
           }
    
           else 
           
           {
            document.getElementById("msg").style.display = "none";

            scope.actModel.acttype = policy.acttype;
            scope.actModel.payouttype = policy.payouttype;
            scope.actModel.amount = policy.amount;
            scope.actModel.invoice_number = policy.invoice_number;
            scope.actModel.client_name = scope.model.client_name;
            scope.actModel.policy_number = scope.model.policy_number;
            scope.actModel.com = scope.model.com;
    
               
        http.post( endpoints.CreateActFromComissionChange,  scope.actModel)
            .then((result: any) => {
                scope.model.issues = result.data.issues;
    
                if (result.data.isSuccess) {
                    navigation.post(endpoints.ActPreview, result.data.model);
                    document.getElementById("msg").style.display = "none";
                    document.getElementById("msg").innerText ="";
                }

                else
                {
                    document.getElementById("msg").style.display = "block";
                    document.getElementById("msg").innerText = scope.model.issues[0].message +" for Invoice Nr: " + scope.actModel.invoice_number;

                }
            });  

           }
        });  
      
     

       

             
    }

    scope.createComissionChange = () =>
    
    {
       
        scope.result.changes=[] ;

        scope.policies.forEach(elem =>
             {
            if(elem.amount>=0 )
            {
                scope.result.changes.push(elem);
            }
        });
        console.log(scope.result.changes);
      
       if(scope.result.changes.length>0)
       {
                        
                     
                         http.post(scope.createEndpoint,scope.result )
                           .then((result: any) => {
                               scope.model.issues = result.data.issues;
                               console.log(result);
                               var msgblck = document.getElementById("msg");
                               msgblck.style.display = "block";
                               msgblck.innerText = scope.model.issues[0].message;
                                scope.result.isSuccess= result.data.isSuccess;
                              
                           }); 
                       
                      


       
       }
        
        

          
    }

}



}



libra.controller("comissionChangeController", ["$scope", "$http", "navigationService", ComissionChangeController])