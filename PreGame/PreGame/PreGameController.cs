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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PreGame
{
    public partial class PreGameController : Form
    {
        dinnerwere.VirtualClientClient dwvc;
        DataSet dsConfig;
        Thread th1;

        public string LastRefresh { get; set; }
        public string Store_ID { get; set; }

        public string DWIP { get; set; }
        public PreGameController()
        {
            InitializeComponent();
            ReadConfig();
          
            //dwvc = new dinnerwere.VirtualClientClient("BasicHttpBinding_IVirtualClient", "http://"+ DWIP + "/VirtualClient");

            if (dsConfig != null)
            {
                PreGameTimer.Interval = Convert.ToInt32(dsConfig.Tables[0].Rows[0]["Interval"].ToString()) * 1000;
                Store_ID = dsConfig.Tables[0].Rows[0]["StoreID"].ToString();
            }
            else
            {
                PreGameTimer.Interval = 5 * 1000;
            }
            PreGameTimer.Enabled = true;
            ContextMenu cMenu = new System.Windows.Forms.ContextMenu();
            cMenu.MenuItems.Add("Configure PreGame", mnuPreGameClick);
            cMenu.MenuItems.Add("Exit", mnuExitClick);
            notifyIcon1.Visible = true;
            notifyIcon1.ContextMenu = cMenu;
            LastRefresh = "Pending";
            btnThread.BackColor = Color.Red;
            btnThread.Text = "Start";


        }

        private void ThreadFunction()
        {
            while (true)
            {
                string error = "";
                try
                {
                    OpenNewTickets();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("OpenNewTickets : " + ex.Message);
                }
                try
                {
                    UpdateShiftedTickets();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("UpdateShiftedTickets : " + ex.Message);
                }
                try
                {
                    UpdateTicketsForClosing();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("UpdateTicketsForClosing : " + ex.Message);
                }
                 try
                {
                    UpdateTicketsCancelByPreGame();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("UpdateTicketsCancelByPreGame : " + ex.Message);
                }
                
                try
                {
                    UpdateTicketsForPayment();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("UpdateTicketsForPayment : " + ex.Message);

                }

                LastRefresh = "Last Run: " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");
                Thread.Sleep(1000);
            }
        }



        private void PreGameTimer_Tick(object sender, EventArgs e)
        {

            lblLastRun.Text = LastRefresh;
        }


        private void OpenNewTickets()
        {
            try
            {
                List<Dictionary<string, object>> jsonResults = new List<Dictionary<string, object>>();
                jsonResults = PreGameAPICaller.GetPreGameAPITickers("1", "1", Store_ID);
                foreach (Dictionary<string, object> dic in jsonResults)
                {
                    try
                    {
                        object value = 0;
                        dic.TryGetValue("status", out value);

                        Int16 status = Convert.ToInt16(value);

                        object vTicketID = 0;
                        dic.TryGetValue("ticket_id", out vTicketID);

                        Int16 ticket_id = Convert.ToInt16(vTicketID);

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
                    catch (Exception ex)
                    { }
                }

            }
            catch (Exception)
            {
                throw;
            }

        }


        private void UpdateShiftedTickets()
        {
            try
            {
                List<Dictionary<string, object>> jsonResults = new List<Dictionary<string, object>>();
                jsonResults = PreGameAPICaller.GetPreGameAPITickers("2", "-1", Store_ID);
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
            catch (Exception)
            {
                throw;
            }



        }


        private void UpdateTicketsForClosing()
        {
            List<Dictionary<string, object>> jsonResults = new List<Dictionary<string, object>>();
            jsonResults = PreGameAPICaller.GetPreGameAPITickers("3", "1", Store_ID);
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
                catch { }
            }




        }

        private void UpdateTicketsCancelByPreGame()
        {
            List<Dictionary<string, object>> jsonResults = new List<Dictionary<string, object>>();
            jsonResults = PreGameAPICaller.GetPreGameAPITickers("9", "1", Store_ID);
            foreach (Dictionary<string, object> dic in jsonResults)
            {
                try
                {
                    object value = 0;
                    dic.TryGetValue("status", out value);

                    Int16 status = Convert.ToInt16(value);
                    switch (status)
                    {

                        case (int)PreGameTicketStatus.CancelByPreGame:
                            object pos_ticket_id = 0;
                            dic.TryGetValue("pos_ticket_id", out pos_ticket_id);

                            wsTicket ticket = dwvc.getTicket(Convert.ToInt32(pos_ticket_id));
                            dwvc.VoidTicket(0, (int)pos_ticket_id, 25);
                            PreGameAPICaller.UpdateTicketAmountOnPreGame(Convert.ToInt64(pos_ticket_id), Convert.ToDecimal(0), (Int32)PreGameTicketStatus.CancelByPreGame);

                            break;
                        default:
                            break;
                    }
                }
                catch { }
            }




        }

        private void UpdateTicketsForPayment()
        {
            List<Dictionary<string, object>> jsonResults = new List<Dictionary<string, object>>();
            jsonResults = PreGameAPICaller.GetPreGameAPITickers("6", "-1", Store_ID);
            foreach (Dictionary<string, object> dic in jsonResults)
            {
                try
                {
                    object value = 0;
                    dic.TryGetValue("status", out value);

                    Int16 status = Convert.ToInt16(value);
                    switch (status)
                    {

                        case (int)PreGameTicketStatus.Paid:
                            object pos_ticket_id = 0;
                            dic.TryGetValue("pos_ticket_id", out pos_ticket_id);

                            object pos_amount_spent = 0;
                            dic.TryGetValue("pos_amount_spent", out pos_amount_spent);
                            wsTicket ticket = dwvc.getTicket(Convert.ToInt32(pos_ticket_id));
                            dwvc.ReopenClosedTicket(0, (int)pos_ticket_id);
                            dwvc.RenameTicket((int)pos_ticket_id, ticket.Name + " (Paid)");
                            
                            wsTransaction trn = new wsTransaction();
                            trn.PaymentAmount = (Decimal)pos_amount_spent;
                            trn.TenderType = "Pre-Game";
                            trn.TenderTypeID = 20;
                            trn.RefNumber = pos_ticket_id.ToString();

                            wsTicketChangeResult tResult = dwvc.addTransactionToTicket(0, (int)pos_ticket_id, trn);
                            int i = tResult.NewTransactionID;
                            PreGameAPICaller.UpdateTicketAmountOnPreGame(Convert.ToInt64(pos_ticket_id), Convert.ToDecimal(ticket.AmountDue), (Int32)PreGameTicketStatus.ConfirmedPaidInPOS);

                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
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

            //Get PinCode
            object Ticket_ID = null;
            dictinory.TryGetValue("ticket_id", out Ticket_ID);

             //Get PinCode
            object email = null;
            dictinory.TryGetValue("email", out email);

            //Get PinCode
            object fname = null;
            dictinory.TryGetValue("first_name", out fname);

            //Get PinCode
            object lname = null;
            dictinory.TryGetValue("last_name", out lname);

            string TicketTitle = title.ToString() + "-" + PicCode.ToString();


            wsTrialTicket trialticker = new wsTrialTicket();
            trialticker.TicketName = TicketTitle;
            trialticker.SchemaNumber = dwvc.GetSchema();


            Int32 pendingID = dwvc.TrialCommit(0, trialticker).PendingID;

            Int32 POS_Ticket_ID = dwvc.CommitPendingTicketWithNoTransaction(pendingID);
            Int32 customerid = 0;
            Person[] customers = dwvc.GetCustomersByEmail(email.ToString());
            if (customers.Length == 0)
            {
                wsPerson p = new wsPerson();
                p.FNAME = fname.ToString();
                p.LNAME = lname.ToString();
                p.EMAIL = email.ToString();

                customerid = dwvc.AddCustomer(0, p);

            }
            else
            {
                customerid = Convert.ToInt32(customers[0].ID);
            }
            Int32[] arr = new Int32[1];
            arr[0] = POS_Ticket_ID;
            dwvc.AssociateCustomerTickets(0, arr, customerid);


            return POS_Ticket_ID;
            //List<Dictionary<string, object>> jsonUsersResults = new List<Dictionary<string, object>>();
            //jsonUsersResults = PreGameAPICaller.GetPreGameTickersUsers(Ticket_ID.ToString());



        }
        private int AddUsersTOTicketinDinnerware(Dictionary<string, object> dictinory)
        {

            // Get Title
            object title = null;
            dictinory.TryGetValue("name", out title);

            //Get PinCode
            object PicCode = null;
            dictinory.TryGetValue("id", out PicCode);

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

        private void btnSave_Click(object sender, EventArgs e)
        {

            string FilePath = Environment.CurrentDirectory + "\\SystemConfig.xml";

            if (File.Exists(FilePath))
            {

                dsConfig.Tables[0].Rows[0]["StoreID"] = txtStoreID.Text;
                dsConfig.Tables[0].Rows[0]["PGIP"] = txtAPIAddress.Text;
                dsConfig.Tables[0].Rows[0]["Interval"] = txtInterval.Text;
                dsConfig.Tables[0].Rows[0]["DWIP"] = txtDinnerIP.Text;
                dsConfig.WriteXml(FilePath);
            }
            else
            {
                dsConfig = new DataSet();
                DataTable dt = new DataTable();
                DataColumn dcStore = new DataColumn("StoreID");
                DataColumn dcPGIP = new DataColumn("PGIP");
                DataColumn dcInterval = new DataColumn("Interval");
                DataColumn dcDWIP = new DataColumn("DWIP");

                dt.Columns.Add(dcStore);
                dt.Columns.Add(dcPGIP);
                dt.Columns.Add(dcInterval);
                dt.Columns.Add(dcDWIP);


                DataRow drStore = dt.NewRow();
                drStore["StoreID"] = 0;
                drStore["PGIP"] = 0;
                drStore["Interval"] = 0;
                drStore["DWIP"] = 0;

                dt.Rows.Add(drStore);

                dsConfig.Tables.Add(dt);
                dsConfig.WriteXml(FilePath);






            }
            ReadConfig();
            MessageBox.Show("Successfully Saved");
        }

        private void ReadConfig()
        {
            dsConfig = new DataSet();
            string FilePath = Environment.CurrentDirectory + "\\SystemConfig.xml";
            if (File.Exists(FilePath))
            {
                dsConfig.ReadXml(FilePath);
                txtStoreID.Text = dsConfig.Tables[0].Rows[0]["StoreID"].ToString();
                txtAPIAddress.Text = dsConfig.Tables[0].Rows[0]["PGIP"].ToString();
                txtInterval.Text = dsConfig.Tables[0].Rows[0]["Interval"].ToString();
                txtDinnerIP.Text = dsConfig.Tables[0].Rows[0]["DWIP"].ToString() ;
                PreGameAPICaller.PreGameApiIP = txtAPIAddress.Text;
                DWIP = dsConfig.Tables[0].Rows[0]["DWIP"].ToString() ;
                Store_ID = dsConfig.Tables[0].Rows[0]["StoreID"].ToString();


                dwvc = new dinnerwere.VirtualClientClient("BasicHttpBinding_IVirtualClient", "http://" + DWIP + "/VirtualClient");
            }
        }

        private void btnThread_Click(object sender, EventArgs e)
        {
            if (btnThread.Text == "Start")
            {
                if (th1 != null && th1.ThreadState == ThreadState.Suspended)
                    th1.Resume();
                else
                {
                    th1 = new Thread(ThreadFunction);
                    th1.Start();
                }
                btnThread.Text = "Stop";
                btnThread.BackColor = Color.Green;
                btnThread.ForeColor = Color.White;
                btnSave.Enabled = false;
                this.WindowState = FormWindowState.Minimized;
            }
            else
            {
                if (th1 != null && th1.ThreadState == ThreadState.Running)
                    th1.Suspend();
                btnThread.Text = "Start";
                btnThread.BackColor = Color.Maroon;
                btnThread.ForeColor = Color.White;
                btnSave.Enabled = true;
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void txtStoreID_TextChanged(object sender, EventArgs e)
        {

        }
    }

}
