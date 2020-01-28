using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace ConfirmationEMailWebApi.Models
{
    public class CreateLogFiles
    {
        private string sLogFormat;
        private string sErrorTime;

        public CreateLogFiles()
        {
            sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString();
            sErrorTime = sYear + sMonth + sDay;
        }

        public void ErrorLog(string sErrMsg)
        {
            StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["Log"].ToString() + sErrorTime, true);
            sw.WriteLine(sLogFormat + sErrMsg);
            sw.Flush();
            sw.Close();
        }
 
    }
}