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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IPreGameAPI" in both code and config file together.
    [ServiceContract]
    public interface IPreGameAPI
    {
        [OperationContract]
        List<Dictionary<string, object>> GetTickets(string Status, string isupdated, string StoreID);

        [OperationContract]
        List<Dictionary<string, object>> GetTicketUsers(string Ticket_ID);

        [OperationContract]
        string UpdateTicketStatus(string TicketID, string IsUpdated, string status, string POS_Ticket_ID);

        [OperationContract]
        string UpdateTicketAmount(string POS_Ticket_ID, string status, string pos_amount_spent);
    }
}
