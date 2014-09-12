using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace PreGame
{
    public static class PreGameAPICaller
    {
        public static string PreGameApiIP;
        public static int UpdateTicketOnPreGame(Int64 iTicketID, int status, Int64 POS_Ticket_ID)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://" + PreGameApiIP + "/PreGameAPI.svc/UpdateTicketStatus/" + iTicketID.ToString() + ",2," + status + "," + POS_Ticket_ID.ToString());
            request.Method = HttpVerb.GET.ToString();
            request.ContentLength = 0;
            request.ContentType = "text/xml";

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }

                // grab the response
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                        using (var reader = new StreamReader(responseStream))
                        {
                            string iResult = reader.ReadToEnd();
                            return 0;
                        }
                }
            }
            return 0;
        }

        public static int UpdateTicketAmountOnPreGame(Int64 POS_Ticket_ID, Decimal PreGameAmount, int status)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://" + PreGameApiIP + "/PreGameAPI.svc/UpdateTicketAmount/" + POS_Ticket_ID.ToString() + "," + status + "," + PreGameAmount);
            request.Method = HttpVerb.GET.ToString();
            request.ContentLength = 0;
            request.ContentType = "text/xml";

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }

                // grab the response
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                        using (var reader = new StreamReader(responseStream))
                        {
                            string iResult = reader.ReadToEnd();
                            return 0;
                        }
                }
            }
            return 0;
        }
       
        public static List<Dictionary<string, object>> GetPreGameAPITickers(string status, string isupdated, string storeid)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://"+PreGameApiIP+"/PreGameAPI.svc/GetTickets/" + status + "," + isupdated + "," + storeid);
            request.Method = HttpVerb.GET.ToString();
            request.ContentLength = 0;
            request.ContentType = "text/xml";

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }

                // grab the response
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                        using (var reader = new StreamReader(responseStream))
                        {
                            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(List<Dictionary<string, object>>));
                            object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                            List<Dictionary<string, object>> jsonResults = objResponse as List<Dictionary<string, object>>;
                            return jsonResults;
                        }
                }

            }
            return null;
        }

        public static List<Dictionary<string, object>> GetPreGameTickersUsers(string POS_Ticket_Id)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://" + PreGameApiIP + "/PreGameAPI.svc/GetTicketUsers/" + POS_Ticket_Id);
            request.Method = HttpVerb.GET.ToString();
            request.ContentLength = 0;
            request.ContentType = "text/xml";

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }

                // grab the response
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                        using (var reader = new StreamReader(responseStream))
                        {
                            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(List<Dictionary<string, object>>));
                            object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                            List<Dictionary<string, object>> jsonResults = objResponse as List<Dictionary<string, object>>;
                            return jsonResults;
                        }
                }

            }
            return null;
        }


    }
}
