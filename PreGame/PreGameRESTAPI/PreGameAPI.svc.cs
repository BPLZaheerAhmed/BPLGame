using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace PreGameRESTAPI
{

    [DataContract]
        // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "PreGameAPI" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select PreGameAPI.svc or PreGameAPI.svc.cs at the Solution Explorer and start debugging.
    public class PreGameAPI : IPreGameAPI
    {
        [WebInvoke(Method = "GET",
                        ResponseFormat = WebMessageFormat.Json,
                        UriTemplate = "/GetTickets/{Status},{isupdated},{StoreID}")]
        public List<Dictionary<string, object>> GetTickets(string Status, string isupdated,string StoreID)
        {
            DBPreGameAPI db = new DBPreGameAPI();
            int iStatus = 0; 
            if (Status != "-1")
            {
               iStatus = Convert.ToInt32(Status); 
            }

          

            return db.GetAllTickets(iStatus, Convert.ToInt32(isupdated), Convert.ToInt16(StoreID));

        }



       [WebInvoke(Method = "GET",
                        ResponseFormat = WebMessageFormat.Json,
                        UriTemplate = "/UpdateTicketStatus/{TicketID},{IsUpdated},{status},{POS_Ticket_ID}")]
        public string UpdateTicketStatus(string TicketID, string IsUpdated, string status, string POS_Ticket_ID)
        {
           
            DBPreGameAPI api = new DBPreGameAPI();
            return api.UpdateTicketStatus(Convert.ToInt64(TicketID), Convert.ToInt32(IsUpdated), Convert.ToInt16(status), Convert.ToInt64(POS_Ticket_ID)).ToString();
            
        }


       [WebInvoke(Method = "GET",
                      ResponseFormat = WebMessageFormat.Json,
                      UriTemplate = "/UpdateTicketAmount/{POS_Ticket_ID},{status},{pos_amount_spent}")]
       public string UpdateTicketAmount(string POS_Ticket_ID, string status, string pos_amount_spent)
       {

           DBPreGameAPI api = new DBPreGameAPI();
           return api.UpdateTicketAmount(Convert.ToInt64(POS_Ticket_ID), Convert.ToInt32(status), Convert.ToDecimal(pos_amount_spent)).ToString();

       }


       [WebInvoke(Method = "GET",
                    ResponseFormat = WebMessageFormat.Json,
                    UriTemplate = "/GetTicketUsers/{Ticket_ID}")]
       public List<Dictionary<string, object>> GetTicketUsers(string Ticket_ID)
       {

           DBPreGameAPI api = new DBPreGameAPI();
           return api.GetTicketUsers(Convert.ToInt64(Ticket_ID));

       }

    }
}
