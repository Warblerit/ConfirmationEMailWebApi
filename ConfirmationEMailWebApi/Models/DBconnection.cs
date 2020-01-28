using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ConfirmationEMailWebApi.Models
{
    public class DBconnection
    {
        public DataSet ExecuteDataSet(SqlCommand command, string UserData)
        {
            DataSet dsResult = new DataSet();
            DataTable dT = new DataTable("DTable");
            DataTable ErrdT = new DataTable("DBERRORTBL");
            ErrdT.Columns.Add("Exception");
            string str = "";
            //string strr = "Data Source=I3-PC;Initial Catalog=HB2;User ID=sa;password=sa123;Integrated Security=true";            
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString()))
            {
                try
                {
                    command.Connection = connection;
                    command.CommandTimeout = 100000;
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dsResult);
                }
                catch (Exception Ex)
                {
                    str = Ex.Message;
                    CreateLogFiles loggg = new CreateLogFiles();
                    loggg.ErrorLog("DBconnection - CommandText - " + command.CommandText + " - ErrMsg - " + str);
                }
                finally
                {
                    dsResult.Tables.Add(ErrdT); ErrdT.Dispose(); ErrdT = null;
                }
            }
            return dsResult;
        }
    }
}