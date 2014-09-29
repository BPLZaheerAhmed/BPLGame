using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreGame
{
        public enum HttpVerb
        {
            GET,
            POST,
            PUT,
            DELETE
        }
        public enum PreGameTicketStatus
        {
            Opened = 1,
            Shifted = 2,
            Closing = 3,
            Closed = 4,
            Cancel = 5,
            Paid = 6,
            TicketCanceledByPOS = 7,
            ConfirmedPaidInPOS = 8,
            CancelByPreGame = 9,
            ReOpenByPOS = 10

        };
 
}
