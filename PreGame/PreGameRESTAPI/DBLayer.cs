﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;

namespace PreGameRESTAPI
{
    public class DBPreGameAPI
    {
        private  MySqlConnection OpenConnection()
        {
            MySqlConnection conn;
            string myConnectionString =  ConfigurationManager.AppSettings["PreGameComm"].ToString(); 
            
            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = myConnectionString;
                conn.Open();
                return conn;
            }
            catch (Exception)
            {
                throw ;
            }
        }

        /// <summary>
        ///  This function returen all unopened tickets
        /// </summary>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetAllTickets(int status,int? IsUpdated, Int16 StoreID)
        {
            MySqlConnection con = OpenConnection();
            string qry = "select t.*, u.`first_name` ,  u.last_name, u.`email` FROM tbl_tickets t INNER JOIN tbl_users u ON u.id = t.`user_id` WHERE store_id =  " + StoreID;
            if (status != 0)
            {
                qry = qry + " and t.status = " + status;
            }
            if (IsUpdated != -1)
            {
                qry = qry + " and is_updated =" + IsUpdated;
            }

            MySqlCommand cmd = new MySqlCommand(qry, con);
            MySqlDataAdapter adpt = new MySqlDataAdapter(cmd);
            DataSet dsResult = new DataSet();
            adpt.Fill(dsResult);
            con.Close();
            con.Dispose();
            
            return ConvertDataTabletoString(dsResult.Tables[0]);
        }
        public List<Dictionary<string, object>> GetTicketDataByID(Int64 TickerID)
        {
            MySqlConnection con = OpenConnection();
            MySqlCommand cmd = new MySqlCommand("Select * from tbl_tickets where Is_Updated = true and ticket_ID = " + TickerID.ToString(), con);
            MySqlDataAdapter adpt = new MySqlDataAdapter(cmd);
            DataSet dsResult = new DataSet();
            adpt.Fill(dsResult);
            con.Close();
            con.Dispose();
           return ConvertDataTabletoString(dsResult.Tables[0]);
        }

        public int UpdateTicketStatus(Int64 TickerID, int isUpdated, int Status, Int64 POS_Ticket_ID )
        {
            MySqlConnection con = OpenConnection();
            MySqlCommand cmd = new MySqlCommand("Update tbl_tickets Set status = " + Status + ", is_Updated = " + isUpdated + ", pos_ticket_id = " + POS_Ticket_ID + " where Id = " + TickerID.ToString(), con);
            int result = cmd.ExecuteNonQuery();
            con.Close();
            con.Dispose();
            return result;
        }
        public int UpdateTicketAmount(Int64 POS_Ticket_ID, Int32 status, Decimal pos_amount_spent )
        {
            
            MySqlConnection con = OpenConnection();
            MySqlCommand cmd = new MySqlCommand("Update tbl_tickets Set status = " + status.ToString() + ", pos_amount_spent = " + pos_amount_spent  + ", is_Updated = 2 where POS_Ticket_ID = " + POS_Ticket_ID.ToString(), con);
            int result = cmd.ExecuteNonQuery();
            con.Close();
            con.Dispose();
            return result;
        }
        public List<Dictionary<string, object>> GetTicketUsers(Int64 TickerID)
        {
            MySqlConnection con = OpenConnection();
            MySqlCommand cmd = new MySqlCommand("Select * from tbl_ticket_users where ticket_id =" + TickerID.ToString(), con);
            MySqlDataAdapter adpt = new MySqlDataAdapter(cmd);
            DataSet dsResult = new DataSet();
            adpt.Fill(dsResult);
            con.Close();
            con.Dispose();
           return ConvertDataTabletoString(dsResult.Tables[0]);
        }
        
        public List<Dictionary<string,object>> ConvertDataTabletoString(DataTable dt)
        {

            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            //return serializer.Serialize(rows);
            return rows;
        }
        

    }
}