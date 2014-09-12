using PreGame.dinnerwere;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PreGame
{
    public partial class frmPreGame : Form
    {
        dinnerwere.VirtualClientClient dwvc;
        public frmPreGame()
        {
            InitializeComponent();
            dwvc = new dinnerwere.VirtualClientClient();
          //  GetAllTickets();
        }


        private void btnOpenTab_Click(object sender, EventArgs e)
        {
            wsTrialTicket trialticker = new wsTrialTicket();
            trialticker.TicketName = txtCustomerName.Text + "-" + txtPIN.Text;
            trialticker.SchemaNumber = dwvc.GetSchema();

            Int32 pendingID =  dwvc.TrialCommit(0, trialticker ).PendingID;

            Int32 ticketId = dwvc.CommitPendingTicketWithNoTransaction(pendingID);

            wsPerson p = new wsPerson();
            p.MEMBERSHIP_ID = "123";
            p.FNAME = "Owais Aized";
            Int32[] arr = new Int32[1];
            arr[0] = ticketId;
           Int32 customerId =  dwvc.AddCustomer(0, p);
           dwvc.AssociateCustomerTickets(0, arr, customerId);



        }

        private void GetAllTickets()
        {
            List<wsTicket> tkCollection =  dwvc.getTicketsSinceWithVoided(DateTime.Now.AddDays(-1)).ToList();
            dataGridView1.DataSource = tkCollection.Where(x => (x.UserName == "Admin" && x.CloseTime == DateTime.MinValue)).ToList();


        }

    }
}
