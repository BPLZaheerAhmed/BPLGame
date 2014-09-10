using PreGame.dinnerwere;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PreGame
{
    public partial class PreGameController : Form
    {
        dinnerwere.VirtualClientClient dwvc;
        public PreGameController()
        {
            InitializeComponent();
            dwvc = new dinnerwere.VirtualClientClient();

            PreGameTimer.Enabled = true;
            ContextMenu cMenu = new System.Windows.Forms.ContextMenu();
            cMenu.MenuItems.Add("Configure PreGame", mnuPreGameClick);
            cMenu.MenuItems.Add("Exit", mnuExitClick);
            notifyIcon1.Visible = true;
            notifyIcon1.ContextMenu = cMenu;

            //wsTransaction trn = new wsTransaction();
            //trn.PaymentAmount = 4;
            //trn.TenderType = "Pre-Game";
            //trn.TenderTypeID = 20;
            //trn.RefNumber = "55719";

            //wsTicketChangeResult tResult = dwvc.addTransactionToTicket(0, 55719, trn);
            //int i = tResult.NewTransactionID;

            //dwvc.CloseTicket(0, 55719);



        }





        private void PreGameTimer_Tick(object sender, EventArgs e)
        {
            OpenNewTickets();
            UpdateShiftedTickets();
            UpdateTicketsForClosing();
            UpdateTicketsForPayment();
            //UpdateAllTicketsfromDinnerwareToAPI();

            lblLastRun.Text = "Last Run: " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");
        }

        private void UpdateAllTicketsfromDinnerwareToAPI()
        {
            //  GetAllTicketsfromDinnerware();
        }

        private void OpenNewTickets()
        {
            List<Dictionary<string, object>> jsonResults = new List<Dictionary<string, object>>();
            jsonResults = PreGameAPICaller.GetPreGameAPITickers("1", "1", "5");
            foreach (Dictionary<string, object> dic in jsonResults)
            {
                try
                {
                    object value = 0;
                    dic.TryGetValue("status", out value);

                    Int16 status = Convert.ToInt16(value);
                    switch (status)
                    {
                        case (int)PreGameTicketStatus.Opened:
                            Int64 iTicketID = CreateTicketsinDinnerware(dic);
                            if (iTicketID != 0)
                            {
                                object ID = 0;
                                dic.TryGetValue("id", out ID);
                                PreGameAPICaller.UpdateTicketOnPreGame(Convert.ToInt64(ID), (int)PreGameTicketStatus.Shifted, iTicketID);
                            }
                            break;


                        default:
                            break;
                    }

                }
                catch { }
            }



        }


        private void UpdateShiftedTickets()
        {

            List<Dictionary<string, object>> jsonResults = new List<Dictionary<string, object>>();
            jsonResults = PreGameAPICaller.GetPreGameAPITickers("2", "-1", "5");
            foreach (Dictionary<string, object> dic in jsonResults)
            {

                try
                {
                    object value = 0;
                    dic.TryGetValue("status", out value);

                    Int16 status = Convert.ToInt16(value);
                    switch (status)
                    {

                        case (int)PreGameTicketStatus.Shifted:
                            object pos_ticket_id = 0;
                            dic.TryGetValue("pos_ticket_id", out pos_ticket_id);

                            object pos_amount_spent = 0;
                            dic.TryGetValue("pos_amount_spent", out pos_amount_spent);
                            wsTicket ticket = dwvc.getTicket(Convert.ToInt32(pos_ticket_id));

                            if (ticket.CloseTime != DateTime.MinValue)
                            {
                                PreGameAPICaller.UpdateTicketAmountOnPreGame(Convert.ToInt64(pos_ticket_id), Convert.ToDecimal(ticket.AmountDue), (Int32)PreGameTicketStatus.TicketCanceledByPOS);

                            }
                            else
                            {
                                if (ticket.AmountDue != Convert.ToDecimal(pos_amount_spent))
                                {
                                    PreGameAPICaller.UpdateTicketAmountOnPreGame(Convert.ToInt64(pos_ticket_id), Convert.ToDecimal(ticket.AmountDue), (Int32)PreGameTicketStatus.Shifted);

                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch { }
            }






        }


        private void UpdateTicketsForClosing()
        {
            List<Dictionary<string, object>> jsonResults = new List<Dictionary<string, object>>();
            jsonResults = PreGameAPICaller.GetPreGameAPITickers("3", "1", "5");
            foreach (Dictionary<string, object> dic in jsonResults)
            {
                try
                {
                object value = 0;
                dic.TryGetValue("status", out value);

                Int16 status = Convert.ToInt16(value);
                switch (status)
                {

                    case (int)PreGameTicketStatus.Closing:
                        object pos_ticket_id = 0;
                        dic.TryGetValue("pos_ticket_id", out pos_ticket_id);

                        object pos_amount_spent = 0;
                        dic.TryGetValue("pos_amount_spent", out pos_amount_spent);
                        wsTicket ticket = dwvc.getTicket(Convert.ToInt32(pos_ticket_id));
                        dwvc.VoidTicket(0, (int)pos_ticket_id, 25);
                        
                            PreGameAPICaller.UpdateTicketAmountOnPreGame(Convert.ToInt64(pos_ticket_id), Convert.ToDecimal(ticket.AmountDue), (Int32)PreGameTicketStatus.Closed);
                        

                        break;
                    default:
                        break;
                }
                }
                catch{}
            }




        }

        private void UpdateTicketsForPayment()
        {
            List<Dictionary<string, object>> jsonResults = new List<Dictionary<string, object>>();
            jsonResults = PreGameAPICaller.GetPreGameAPITickers("6", "-1", "5");
            foreach (Dictionary<string, object> dic in jsonResults)
            {

                object value = 0;
                dic.TryGetValue("status", out value);

                Int16 status = Convert.ToInt16(value);
                try
                {
                switch (status)
                {

                    case (int)PreGameTicketStatus.Paid:
                        object pos_ticket_id = 0;
                        dic.TryGetValue("pos_ticket_id", out pos_ticket_id);

                        object pos_amount_spent = 0;
                        dic.TryGetValue("pos_amount_spent", out pos_amount_spent);
                        wsTicket ticket = dwvc.getTicket(Convert.ToInt32(pos_ticket_id));
                        dwvc.ReopenClosedTicket(0, (int)pos_ticket_id);
                        
                    wsTransaction trn = new wsTransaction();
                    trn.PaymentAmount = (Decimal)pos_amount_spent;
                    trn.TenderType = "Pre-Game";
                    trn.TenderTypeID = 20;
                    trn.RefNumber = pos_ticket_id.ToString();

                    wsTicketChangeResult tResult = dwvc.addTransactionToTicket(0, (int)pos_ticket_id, trn);
                    int i = tResult.NewTransactionID;

                        //dwvc.CloseTicket(0, 55719);
                    PreGameAPICaller.UpdateTicketAmountOnPreGame(Convert.ToInt64(pos_ticket_id), Convert.ToDecimal(ticket.AmountDue), (Int32)PreGameTicketStatus.ConfirmedPaidInPOS);

                        break;
                    default:
                        break;
                }
                     }
            catch{
            }
            }
                




        }

        private int CreateTicketsinDinnerware(Dictionary<string, object> dictinory)
        {

            // Get Title
            object title = null;
            dictinory.TryGetValue("name", out title);

            //Get PinCode
            object PicCode = null;
            dictinory.TryGetValue("pin_code", out PicCode);

            string TicketTitle = title.ToString() + "-" + PicCode.ToString();


            wsTrialTicket trialticker = new wsTrialTicket();
            trialticker.TicketName = TicketTitle;
            trialticker.SchemaNumber = dwvc.GetSchema();

            Int32 pendingID = dwvc.TrialCommit(0, trialticker).PendingID;

            return dwvc.CommitPendingTicketWithNoTransaction(pendingID);

        }











        #region "Events"
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void mnuPreGameClick(object sender, EventArgs e)
        {
            this.Show();
        }

        private void mnuExitClick(object sender, EventArgs e)
        {
            Application.Exit();

        }
        #endregion
    }

}
