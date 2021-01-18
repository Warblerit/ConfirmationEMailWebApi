using ConfirmationEMailWebApi.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient; 
using System.IO;
using System.Linq;
using System.Net; 
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Script.Serialization; 

namespace ConfirmationEMailWebApi.Controllers
{
    [RoutePrefix("API/ConfirmationEMail")]
    public class ConfirmationEMailController : ApiController
    {
        CreateLogFiles log = new CreateLogFiles();
        //Search load
        [HttpPost]
        [Route("ConfirmEMail")]
        public IHttpActionResult ConfirmEMail(ConfirmationEMail All)
        ///public async Task<Response> SomeHTTPAction(ConfirmationEMail All)
        {

            try
            {
                string Response = "";
                string Response1 = "Failure";
                string Response2 = "Failure";
                string Newid = "";
                String AzureBlobPdfURl = "";
                SqlCommand command5 = new SqlCommand();
                DataSet ds5 = new DataSet();
                command5.CommandText = "SP_SMTPMailSetting_Help";
                command5.CommandType = CommandType.StoredProcedure;
                command5.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "SMTP";
                command5.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = "";
                command5.Parameters.Add("@Id", SqlDbType.BigInt).Value = 0;
                ds5 = new DBconnection().ExecuteDataSet(command5, "");
                string Host = ds5.Tables[0].Rows[0][0].ToString();
                string CredentialsUserName = ds5.Tables[0].Rows[0][1].ToString();
                string CredentialsPassword = ds5.Tables[0].Rows[0][2].ToString();
                int Port = Convert.ToInt16(ds5.Tables[0].Rows[0][3]);


                SqlCommand command = new SqlCommand();
                DataSet ds = new DataSet();
                command.CommandText = "SP_ConfirmationEMail_Help";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@Str", SqlDbType.NVarChar).Value = "";
                command.Parameters.Add("@Id", SqlDbType.BigInt).Value = All.BookingId;
                ds = new DBconnection().ExecuteDataSet(command, "");


                #region
                if (All.GuestMailChk == true)
                {
                    System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
                    smtp.Port = Port;
                    smtp.Host = Host; smtp.Credentials = new System.Net.NetworkCredential(CredentialsUserName, CredentialsPassword);
                    smtp.EnableSsl = true;
                    string MailContent = "";

                    #region
                    if (ds.Tables[0].Rows[0][8].ToString() == "Bed")
                    {
                        if (ds.Tables[10].Rows.Count > 0)
                        {
                            message.From = new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][0].ToString(), "", System.Text.Encoding.UTF8);
                        }
                        else
                        {
                            message.From = new System.Net.Mail.MailAddress("stay@hummingbirdindia.com", "", System.Text.Encoding.UTF8);
                        }

                        if (All.ResendFlag == true)
                        {
                            var Mail = All.PropertyGusetEmail.Split(',');
                            for (int i = 0; i < Mail.Length; i++)
                            {
                                try
                                {
                                    message.To.Add(new System.Net.Mail.MailAddress(Mail[i].ToString()));
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + Mail[i].ToString());
                                }
                            }
                            if (All.UserEmail != "")
                            {
                                try
                                {
                                    message.CC.Add(new System.Net.Mail.MailAddress(All.UserEmail));
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + All.UserEmail);
                                }
                            }
                            message.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                        }
                        else
                        {
                            if (ds.Tables[4].Rows[0][0].ToString() == "0")
                            {
                                if (ds.Tables[8].Rows[0][0].ToString() != "")
                                {
                                    message.To.Add(new System.Net.Mail.MailAddress(ds.Tables[8].Rows[0][0].ToString()));
                                }
                            }
                            else
                            {
                                for (int i = 0; i < ds.Tables[5].Rows.Count; i++)
                                {
                                    if (i <= 40)
                                    {
                                        if (ds.Tables[5].Rows[i][0].ToString() != "")
                                        {
                                            try
                                            {
                                                message.To.Add(new System.Net.Mail.MailAddress(ds.Tables[5].Rows[i][0].ToString()));
                                            }
                                            catch (Exception ex)
                                            {
                                                CreateLogFiles log = new CreateLogFiles();
                                                log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + ds.Tables[5].Rows[i][0].ToString());
                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                if (ds.Tables[8].Rows[0][0].ToString() != "")
                                {
                                    try
                                    {
                                        message.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[8].Rows[0][0].ToString()));
                                    }
                                    catch (Exception ex)
                                    {
                                        CreateLogFiles log = new CreateLogFiles();
                                        log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => CC =>" + ds.Tables[8].Rows[0][0].ToString());
                                    }
                                }
                            }
                            //Extra CC
                            ////for (int i = 0; i < ds.Tables[7].Rows.Count; i++)
                            ////{
                            ////    if (ds.Tables[7].Rows[i][0].ToString() != "")
                            ////    {
                            ////        try
                            ////        {
                            ////            message.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[7].Rows[i][0].ToString()));
                            ////        }
                            ////        catch (Exception ex)
                            ////        {
                            ////            CreateLogFiles log = new CreateLogFiles();
                            ////            log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => Extra CC =>" + ds.Tables[7].Rows[i][0].ToString());
                            ////        }
                            ////    }
                            ////}
                            ////// Extra CC email from Front end
                            ////if (ds.Tables[8].Rows[0][1].ToString() != "")
                            ////{
                            ////    string ExtraCC = ds.Tables[8].Rows[0][1].ToString();
                            ////    var ExtraCCEmail = ExtraCC.Split(',');
                            ////    int cnt = ExtraCCEmail.Length;
                            ////    for (int i = 0; i < cnt; i++)
                            ////    {
                            ////        if (ExtraCCEmail[i].ToString() != "")
                            ////        {
                            ////            try
                            ////            {
                            ////                message.CC.Add(new System.Net.Mail.MailAddress(ExtraCCEmail[i].ToString()));
                            ////            }
                            ////            catch (Exception ex)
                            ////            {
                            ////                CreateLogFiles log = new CreateLogFiles();
                            ////                log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => Extra CC From Front End =>" + ExtraCCEmail[i].ToString());
                            ////            }
                            ////        }
                            ////    }
                            ////}
                            ////if (ds.Tables[2].Rows[0][4].ToString() != "")
                            ////{
                            ////    try
                            ////    {
                            ////        message.Bcc.Add(new System.Net.Mail.MailAddress(ds.Tables[2].Rows[0][4].ToString()));
                            ////    }
                            ////    catch (Exception ex)
                            ////    {
                            ////        CreateLogFiles log = new CreateLogFiles();
                            ////        log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => BCc =>" + ds.Tables[2].Rows[0][4].ToString());
                            ////    }
                            ////}
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress("booking_confirmation@staysimplyfied.com"));
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress("bookingbcc@staysimplyfied.com"));
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                        }
                        message.Bcc.Add(new System.Net.Mail.MailAddress("prabakaran@warblerit.com"));

                        message.Subject = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString();

                        // Client Logo
                        string Imagelocation = "";
                        string Imagealt = "";
                        string PtyType = ds.Tables[5].Rows[0][1].ToString();
                        if (PtyType == "MGH")
                        {
                            Imagelocation = ds.Tables[6].Rows[0][4].ToString();
                            Imagealt = ds.Tables[6].Rows[0][5].ToString();
                            if (Imagelocation == "")
                            {
                                Imagelocation = ds.Tables[6].Rows[0][0].ToString();
                                Imagealt = ds.Tables[6].Rows[0][1].ToString();
                            }
                        }
                        else
                        {
                            Imagelocation = ds.Tables[6].Rows[0][0].ToString();
                            Imagealt = ds.Tables[6].Rows[0][1].ToString();
                        }

                        // Contact Email And Phone
                        string ContactEmail = "";
                        string DeskNo = "";
                        DeskNo = ds.Tables[2].Rows[0][13].ToString();
                        ContactEmail = ds.Tables[2].Rows[0][14].ToString();

                        // Map Link
                        string MapLink = "";
                        if (ds.Tables[1].Rows[0][13].ToString() != "")
                        {
                            MapLink = "https://www.google.co.in/maps/place/" + ds.Tables[1].Rows[0][13].ToString();
                        }
                        else
                        {
                            MapLink = "#";
                        }

                        // View in browser Link
                        string id = ds.Tables[2].Rows[0][11].ToString();
                        string link = "http://mybooking.hummingbirdindia.com/?redirect=BookingConfirmation&B=B&R=" + id;

                        // Spl Note
                        string SplNote = ds.Tables[2].Rows[0][8].ToString();
                        if (SplNote == "")
                        {
                            SplNote = "- NA -";
                        }

                        string style = @"<!DOCTYPE html>
                                    <html lang=""en"">
                                    <head>
                                    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
                                    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">  
                                    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge""> 
                                    <meta name=""format-detection"" content=""telephone=no""> 
                                    <title>Booking Confirmation</title>
                                    <link href=""https://fonts.googleapis.com/css?family=Cabin&display=swap"" rel=""stylesheet"">
                                    <style data-roadie-ignore data-immutable=""true"">
                                    @media only screen and (max-width: 596px) {
                                      .small-float-center {
                                        margin: 0 auto !important;
                                        float: none !important;
                                        text-align: center !important;
                                      }
                                      .small-text-center {
                                        text-align: center !important;
                                      }
                                      .small-text-left {
                                        text-align: left !important;
                                      }
                                      .small-text-right {
                                        text-align: right !important;
                                      }
                                    }
                                    @media only screen and (max-width: 596px) {
                                      table.body table.container .hide-for-large {
                                        display: block !important;
                                        width: auto !important;
                                        overflow: visible !important;
                                        max-height: none !important;
                                      }
                                    }
                                   @media only screen and (max-width: 596px) {
                                      table.body table.container .row.hide-for-large,
                                      table.body table.container .row.hide-for-large {
                                        display: table !important;
                                        width: 100% !important;
                                      }
                                    }
                                    @media only screen and (max-width: 596px) {
                                      table.body table.container .show-for-large {
                                        display: none !important;
                                        width: 0;
                                        mso-hide: all;
                                        overflow: hidden;
                                      }
                                    }
                                    @media only screen and (max-width: 596px) {
                                      table.body img {
                                        width: auto !important;
                                        height: auto !important;
                                      }
                                      table.body center {
                                        min-width: 0 !important;
                                      }
                                      table.body .container {
                                        width: 95% !important;
                                      }
                                      table.body .columns,
                                      table.body .column {
                                        height: auto !important;
                                        -moz-box-sizing: border-box;
                                        -webkit-box-sizing: border-box;
                                        box-sizing: border-box;
                                        padding-left: 16px !important;
                                        padding-right: 16px !important;
                                      }
                                      table.body .columns .column,
                                      table.body .columns .columns,
                                      table.body .column .column,
                                      table.body .column .columns {
                                        padding-left: 0 !important;
                                        padding-right: 0 !important;
                                      }
                                      table.body .collapse .columns,
                                      table.body .collapse .column {
                                        padding-left: 0 !important;
                                        padding-right: 0 !important;
                                      }
                                      td.small-1,
                                      th.small-1 {
                                        display: inline-block !important;
                                        width: 8.33333% !important;
                                      }
                                      td.small-2,
                                      th.small-2 {
                                        display: inline-block !important;
                                        width: 16.66667% !important;
                                      }
                                      td.small-3,
                                      th.small-3 {
                                        display: inline-block !important;
                                        width: 25% !important;
                                      }
                                      td.small-4,
                                      th.small-4 {
                                        display: inline-block !important;
                                        width: 33.33333% !important;
                                      }
                                      td.small-5,
                                      th.small-5 {
                                        display: inline-block !important;
                                        width: 41.66667% !important;
                                      }
                                      td.small-6,
                                      th.small-6 {
                                        display: inline-block !important;
                                        width: 50% !important;
                                      }
                                      td.small-7,
                                      th.small-7 {
                                        display: inline-block !important;
                                        width: 58.33333% !important;
                                      }
                                      td.small-8,
                                      th.small-8 {
                                        display: inline-block !important;
                                        width: 66.66667% !important;
                                      }
                                      td.small-9,
                                      th.small-9 {
                                        display: inline-block !important;
                                        width: 75% !important;
                                      }
                                      td.small-10,
                                      th.small-10 {
                                        display: inline-block !important;
                                        width: 83.33333% !important;
                                      }
                                      td.small-11,
                                      th.small-11 {
                                        display: inline-block !important;
                                        width: 91.66667% !important;
                                      }
                                      td.small-12,
                                      th.small-12 {
                                        display: inline-block !important;
                                        width: 100% !important;
                                      }
                                      .columns td.small-12,
                                      .column td.small-12,
                                      .columns th.small-12,
                                      .column th.small-12 {
                                        display: block !important;
                                        width: 100% !important;
                                      }
                                      .body .columns td.small-1,
                                      .body .column td.small-1,
                                      td.small-1 center,
                                      .body .columns th.small-1,
                                      .body .column th.small-1,
                                      th.small-1 center {
                                        display: inline-block !important;
                                        width: 8.33333% !important;
                                      }
                                      .body .columns td.small-2,
                                      .body .column td.small-2,
                                      td.small-2 center,
                                      .body .columns th.small-2,
                                      .body .column th.small-2,
                                      th.small-2 center {
                                        display: inline-block !important;
                                        width: 16.66667% !important;
                                      }
                                      .body .columns td.small-3,
                                      .body .column td.small-3,
                                      td.small-3 center,
                                      .body .columns th.small-3,
                                      .body .column th.small-3,
                                      th.small-3 center {
                                        display: inline-block !important;
                                        width: 25% !important;
                                      }
                                      .body .columns td.small-4,
                                      .body .column td.small-4,
                                      td.small-4 center,
                                      .body .columns th.small-4,
                                      .body .column th.small-4,
                                      th.small-4 center {
                                        display: inline-block !important;
                                        width: 33.33333% !important;
                                      }
                                      .body .columns td.small-5,
                                      .body .column td.small-5,
                                      td.small-5 center,
                                      .body .columns th.small-5,
                                      .body .column th.small-5,
                                      th.small-5 center {
                                        display: inline-block !important;
                                        width: 41.66667% !important;
                                      }
                                      .body .columns td.small-6,
                                      .body .column td.small-6,
                                      td.small-6 center,
                                      .body .columns th.small-6,
                                      .body .column th.small-6,
                                      th.small-6 center {
                                        display: inline-block !important;
                                        width: 50% !important;
                                      }
                                      .body .columns td.small-7,
                                      .body .column td.small-7,
                                      td.small-7 center,
                                      .body .columns th.small-7,
                                      .body .column th.small-7,
                                      th.small-7 center {
                                        display: inline-block !important;
                                        width: 58.33333% !important;
                                      }
                                      .body .columns td.small-8,
                                      .body .column td.small-8,
                                      td.small-8 center,
                                      .body .columns th.small-8,
                                      .body .column th.small-8,
                                      th.small-8 center {
                                        display: inline-block !important;
                                        width: 66.66667% !important;
                                      }
                                      .body .columns td.small-9,
                                      .body .column td.small-9,
                                      td.small-9 center,
                                      .body .columns th.small-9,
                                      .body .column th.small-9,
                                      th.small-9 center {
                                        display: inline-block !important;
                                        width: 75% !important;
                                      }
                                      .body .columns td.small-10,
                                      .body .column td.small-10,
                                      td.small-10 center,
                                      .body .columns th.small-10,
                                      .body .column th.small-10,
                                      th.small-10 center {
                                        display: inline-block !important;
                                        width: 83.33333% !important;
                                      }
                                      .body .columns td.small-11,
                                      .body .column td.small-11,
                                      td.small-11 center,
                                      .body .columns th.small-11,
                                      .body .column th.small-11,
                                      th.small-11 center {
                                        display: inline-block !important;
                                        width: 91.66667% !important;
                                      }
                                      table.body td.small-offset-1,
                                      table.body th.small-offset-1 {
                                        margin-left: 8.33333% !important;
                                        margin-left: 8.33333% !important;
                                      }
                                      table.body td.small-offset-2,
                                      table.body th.small-offset-2 {
                                        margin-left: 16.66667% !important;
                                        margin-left: 16.66667% !important;
                                      }
                                      table.body td.small-offset-3,
                                      table.body th.small-offset-3 {
                                        margin-left: 25% !important;
                                        margin-left: 25% !important;
                                      }
                                      table.body td.small-offset-4,
                                      table.body th.small-offset-4 {
                                        margin-left: 33.33333% !important;
                                        margin-left: 33.33333% !important;
                                      }
                                      table.body td.small-offset-5,
                                      table.body th.small-offset-5 {
                                        margin-left: 41.66667% !important;
                                        margin-left: 41.66667% !important;
                                      }
                                      table.body td.small-offset-6,
                                      table.body th.small-offset-6 {
                                        margin-left: 50% !important;
                                        margin-left: 50% !important;
                                      }
                                      table.body td.small-offset-7,
                                      table.body th.small-offset-7 {
                                        margin-left: 58.33333% !important;
                                        margin-left: 58.33333% !important;
                                      }
                                      table.body td.small-offset-8,
                                      table.body th.small-offset-8 {
                                        margin-left: 66.66667% !important;
                                        margin-left: 66.66667% !important;
                                      }
                                      table.body td.small-offset-9,
                                      table.body th.small-offset-9 {
                                        margin-left: 75% !important;
                                        margin-left: 75% !important;
                                      }
                                      table.body td.small-offset-10,
                                      table.body th.small-offset-10 {
                                        margin-left: 83.33333% !important;
                                        margin-left: 83.33333% !important;
                                      }
                                      table.body td.small-offset-11,
                                      table.body th.small-offset-11 {
                                        margin-left: 91.66667% !important;
                                        margin-left: 91.66667% !important;
                                      }
                                      table.body table.columns td.expander,
                                      table.body table.columns th.expander {
                                        display: none !important;
                                      }
                                      table.body .right-text-pad,
                                      table.body .text-pad-right {
                                        padding-left: 10px !important;
                                      }
                                      table.body .left-text-pad,
                                      table.body .text-pad-left {
                                        padding-right: 10px !important;
                                      }
                                      table.menu {
                                        width: 100% !important;
                                      }
                                      table.menu td,
                                      table.menu th {
                                        width: auto !important;
                                        display: inline-block !important;
                                      }
                                      table.menu.vertical td,
                                      table.menu.vertical th,
                                      table.menu.small-vertical td,
                                      table.menu.small-vertical th {
                                        display: block !important;
                                      }
                                      table.menu[align=""center""] {
                                        width: auto !important;
                                      }
                                      table.button.expand {
                                        width: 100% !important;
                                      }
                                    }
                                    @media only screen and (max-width: 596px) {
                                      .calendar-content {
                                        padding: 0px !important;
                                        width: 288px !important;
                                      }
                                      .not-available-day,
                                      .calendar-today,
                                      .available-day {
                                        height: 40px !important;
                                        width: 40px !important;
                                      }
                                      .day-label {
                                        margin-left: 10% !important;
                                        margin-top: 0% !important;
                                        font-size: 15px;
                                      }
	                                  .p
	                                  {
	                                  font-size:16px
	                                  font-size:4vw
	                                  }
                                    }
                                  </style></head><body style=""font-size:16px;min-width:100%;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;-moz-box-sizing:border-box;-webkit-box-sizing:border-box;box-sizing:border-box;font-family:'Circular', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;margin:0;line-height:1.3;color:#0a0a0a;text-align:left;width:100% !important"">
                                  <div class=""preheader"" style=""mso-hide:all;visibility:hidden;opacity:0;color:transparent;font-size:0px;width:0;height:0;display:none !important""></div>
                                  <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
                                  <meta name=""viewport"" content=""width=device-width"">";

                        string header = "<table class=\"body\" style=\"border-spacing:0;border-collapse:collapse;vertical-align:top;-webkit-hyphens:none;-moz-hyphens:none;hyphens:none;-ms-hyphens:none;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;margin:0;text-align:left;font-size:16px;line-height:19px;background:#f3f3f3;padding:0;width:100%;height:100%;color:#0a0a0a;margin-bottom:0px !important;background-color: white;\">" +
                                        "<tr style=\"padding: 0; vertical-align:top;text-align:left\" >" +
                                        "<td class=\"center\" align=\"center\" valign=\"top\" style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                        "<center style=\"width:100%;min-width:580px\">" +
                                        "<table class=\"container\" style=\"border-spacing:0;border-collapse:collapse;padding:0;vertical-align:top;background:#fefefe;width:580px;margin:0 auto;text-align:inherit;max-width:580px;\">" +
                                        "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                        "<td style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                        "<div style=\"padding-top:10px\">" +
                                        "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                        "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                        "<th style=\"width: 40%;>" +
                                        "<a href=\"https://www.hummingbirdindia.com\" target=\"_blank\" style=\"padding:0;margin:0;text-align:left;line-height:1.3;color:#2199e8;text-decoration:none\">" +
                                        "<img align =\"center\" alt=\"" + Imagealt + "\" class=\"center standard-header\" src=\"" + Imagelocation + "\" style=\"max-width: 120px\" ></a>";

                        string header_cnt1 = " </th> " +
                                             "<th style=\"font-size:16px;padding:20px 0 20px 0;line-height:28px;\">";

                        if (ds.Tables[2].Rows[0][15].ToString() != "")
                        {
                            header_cnt1 += "<p>Hotel Confirmation #: " + ds.Tables[2].Rows[0][15].ToString() + "</p>";
                        }

                        header_cnt1 += "<p>HB Confirmation #: " + ds.Tables[2].Rows[0][2].ToString() + "</p>" +
                                             "</th>" +
                                             "</tr>" +
                                             "</table>" +
                                             "</div>";

                        string HotelName = "<div>" +
                                          "<div class=\"headline-body\" style=\"padding-bottom:15px\">" +
                                          "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                          "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                          "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;width:564px;margin:0 auto;padding-left:16px;padding-right:16px;padding-bottom:0px !important\">" +
                                          "<p class=\"body  body-lg body-link-rausch light text-left   \" style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;line-height:1.4;font-weight:300;font-size:24px;hyphens:none;-ms-hyphens:none;-webkit-hyphens:none;-moz-hyphens:none;text-align:left;margin-bottom:0px !important;color:#0a0a0a; \">" + ds.Tables[1].Rows[0][5].ToString() +
                                          "<br /><span style=\"float: right;font-size:20px;\">Reservation Confirmed</span>" +
                                          "</p>" +
                                          "</th></tr></table></div></div>";

                        string ChkInOutDate = "";
                        if (All.LTIAPIFlag == true)
                        {
                            ChkInOutDate = "<div>" +
                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                  "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                  "<p class=\"body-text-lg light\" style=\"margin:0;text-align:left;padding:0;font-weight:300;font-family:'Circular', 'Cabin', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important;\">" + All.CheckinDate + "</p>" +
                                  "<p class=\"body-text light\" style=\"margin:0;text-align:left;padding:0;font-weight:300;font-family:'Circular', 'Cabin', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important;\">Check-in " + All.CheckinTime + "</p>" +
                                  "</th>" +
                                  "<th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                  "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                  "</th>" +
                                  "<th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                  "<p class=\"body-text-lg light text-right\" style=\"padding:0;margin:0;font-size:17px;font-weight:300;font-family:'Circular', 'Helvetica', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important;\">" + All.CheckoutDate + "</p>" +
                                  "<p class=\"body-text light text-right\"  style=\"padding:0;margin:0;font-size:16px;font-weight:300;font-family:'Circular', 'Helvetica', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important;\">Check-out</p>" +
                                  "</th></tr></table></div>" +
                                  "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                  "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                  "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                  "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                  "</th></tr></table></div>" +
                                  "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                  "<th class=\"col-pad-left-2 col-pad-right-2\" style=\"padding:0;margin:0;padding-left:16px;padding-right:16px\">" +
                                  "<div style=\"margin:0;-webkit-border-radius:8px;border-radius:8px;display:block;border-color:#d9242c;border-width:2px;border-style:dotted dashed;\">" +
                                  "<p class=\"text-center\" style='font-weight:normal;padding:0;margin:0;text-align:center;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;font-size:24px;line-height:32px;margin-bottom:0px !important'>Guest Details</p>" +
                                  "</div></th></tr></table></div>";
                        }
                        else
                        {

                            ChkInOutDate = "<div>" +
                                   "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                   "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                   "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                   "<p class=\"body-text-lg light\" style=\"margin:0;text-align:left;padding:0;font-weight:300;font-family:'Circular', 'Cabin', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important;\">" + ds.Tables[0].Rows[0][10].ToString() + "</p>" +
                                   "<p class=\"body-text light\" style=\"margin:0;text-align:left;padding:0;font-weight:300;font-family:'Circular', 'Cabin', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important;\">Check-in " + ds.Tables[0].Rows[0][9].ToString() + "</p>" +
                                   "</th>" +
                                   "<th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                   "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                   "</th>" +
                                   "<th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                   "<p class=\"body-text-lg light text-right\" style=\"padding:0;margin:0;font-size:17px;font-weight:300;font-family:'Circular', 'Helvetica', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important;\">" + ds.Tables[0].Rows[0][11].ToString() + "</p>" +
                                   "<p class=\"body-text light text-right\"  style=\"padding:0;margin:0;font-size:16px;font-weight:300;font-family:'Circular', 'Helvetica', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important;\">Check-out</p>" +
                                   "</th></tr></table></div>" +
                                   "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                   "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                   "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                   "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                   "</th></tr></table></div>" +
                                   "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                   "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                   "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                   "<th class=\"col-pad-left-2 col-pad-right-2\" style=\"padding:0;margin:0;padding-left:16px;padding-right:16px\">" +
                                   "<div style=\"margin:0;-webkit-border-radius:8px;border-radius:8px;display:block;border-color:#d9242c;border-width:2px;border-style:dotted dashed;\">" +
                                   "<p class=\"text-center\" style='font-weight:normal;padding:0;margin:0;text-align:center;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;font-size:24px;line-height:32px;margin-bottom:0px !important'>Guest Details</p>" +
                                   "</div></th></tr></table></div>";
                        }


                        string TablHdr = "<div style=\"padding-top:8px;padding-bottom:8px;padding-left:16px;padding-right:16px;\">" +
                                         "<table rules=\"rows\" style=\"border:#dbdbdb\"><tr>" +
                                         "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\"><strong>Guest Name</strong></td>" +
                                         "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\"><strong>Room No</strong></td>" +
                                         "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\"><strong>Occupancy</strong></td>" +
                                         "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\"><strong>Tariff / <br>Room / Day</strong></td>" +
                                         "</tr><tr></tr>";

                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            TablHdr +=
                                "<tr style=\"font-style:normal;font-weight:normal;\" class=\"ng-scope\">" +
                                "<td class=\"padd ng-binding\" style=\"vertical-align:middle;text-align:center \">" + ds.Tables[0].Rows[i][12].ToString() + ". " + ds.Tables[0].Rows[i][0].ToString() + " " + ds.Tables[0].Rows[i][13].ToString() + " </td>" +
                                "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[0].Rows[i][6].ToString() + "</td>" +
                                "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[0].Rows[i][7].ToString() + "</td>" +
                                "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">INR " + ds.Tables[0].Rows[i][3].ToString() + "</td>" +
                                "</tr>";
                        }

                        TablHdr += "</table><div><div class=\"row-pad-bot-1\" style=\"padding-bottom:8px !important;padding-top:6px;\"></div>" +
                                    "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                    "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                    "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                    "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:18px;margin-bottom:0px !important;'>Tariff Payment: " + ds.Tables[0].Rows[0][4].ToString() + "</p></th>" +
                                    "<th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                    "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\"></th>" +
                                    "<th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                    "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:18px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important;'>Service Payment: " + ds.Tables[0].Rows[0][5].ToString() + "</p>" +
                                    "</th></tr></table></div></div>";

                        string Note = "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                  "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                  "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                  "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                  "</th></tr></table></div>" +
                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                  "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                  "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:14px;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;\"><strong>Note : </strong>" + SplNote + " </p>" +
                                  "</th></tr></table>";

                        string Address = "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                           "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                            "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                            "</th></tr></table></div>" +
                                            "<div style =\"padding-top:8px;padding-bottom:8px\">" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:24px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;\">Address</p>" +
                                            "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:18px;margin-bottom:0px !important'>" + ds.Tables[1].Rows[0][0].ToString() + "</p>" +
                                            "<p class=\"body-text light\">" + ds.Tables[1].Rows[0][1].ToString() + "</p></th>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<a href=\"" + MapLink + "\" style =\"font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;margin:0;text-align:left;line-height:1.3;color:#2199e8;text-decoration:none\">" +
                                            "<p class=\"body-text-lg light color-rausch text-right\" style='padding:0;margin:0 0 5px 0;word-break:normal;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;line-height:1.2;font-size:24px;text-align:center;color:#d9242c !important;margin-bottom:0px !important'>Get Directions</p>" +
                                            "<p class=\"body-text light\" style='margin:0;text-align:center;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:18px;margin-bottom:0px !important'>" +
                                            "<img style =\"width:36px;height:36px;\" src=\"https://portalvhds4prl9ymlwxnt8.blob.core.windows.net/img/Google_Maps_Icon.png\"/></p></a></th></table></div><div>" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                            "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                            "</th></tr></table></div>";


                        string BookerDtls = "<div style=\"padding-top:8px;padding-bottom:8px\" >" +
                                             "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                             "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                             "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                             "<p style = 'margin:0;text-align:left;padding:0;' > Booked by</p>" +
                                             "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                             "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][3].ToString() + " </ p >" +
                                             "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                             "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                             "<hr>" +
                                             "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                             "<hr>" +
                                             "</th>" +
                                              "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                             "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                             "<p style = 'margin:0;text-align:left;padding:0;' >Client Request #</p>" +
                                             "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                             "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][12].ToString() + " </ p >" +
                                             "</th>" +
                                              "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                             "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                             "<hr>" +
                                             "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                             "<hr>" +
                                             "</th>" +
                                              "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                             "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                             "<p style = 'margin:0;text-align:left;padding:0;' >Issues / feedbacks</p>" +
                                             "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                             "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + DeskNo + "<br>" + ContactEmail + " </ p >" +
                                             "</th>" +
                                             "</tr></table></div>";


                        BookerDtls += "<div>" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                            "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                            "</th></tr></table></div><div style =\"padding-top:2px\" >" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;text-align:left\">" +
                                            "<th style=\"width: 60%;\"><a href=\"" + link + "\" target=\"_blank\"> <span style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:10px 0 10px 16px;margin:0;text-align:left;line-height:1.3;text-decoration:none;font-weight:300;color:#d9242c !important\">Security/Cancellation Policy</span></a>" +
                                            "</th><th style=\"font-size:10px;padding:10px 16px 10px 0;line-height:28px;text-align:right;\" >" +
                                            "<p> Powered by Staysimplyfied.com</p></th></tr></table></div></tr></table></div></td></tr></table></center></td></tr></table>";
                        string EndData = "</body></html>";

                        MailContent = style + header + header_cnt1 + HotelName + ChkInOutDate + TablHdr + Note + Address + BookerDtls + EndData;
                        var PdfContent = header + header_cnt1 + HotelName + ChkInOutDate + TablHdr + Note + Address + BookerDtls;

                        var htmlContent = String.Format(PdfContent, DateTime.Now);
                        var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
                        var pdfBytes = htmlToPdf.GeneratePdf(htmlContent);
                        string path = @"D:\home\site\wwwroot\Confirmations\";

                        var BFilePathWhatsApp = "";

                        if (Directory.Exists(path))
                        {
                            var FileName = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            if (File.Exists(FileName))
                            {
                                var AttachmentName = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                                long ticks = DateTime.Now.Ticks;
                                byte[] bytes = BitConverter.GetBytes(ticks);
                                Newid = Convert.ToBase64String(bytes).Replace('+', '_').Replace('/', '-').TrimEnd('=');
                                File.WriteAllBytes(path + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                                System.Net.Mail.Attachment att1 = new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf");
                                att1.Name = AttachmentName;
                                message.Attachments.Add(att1);
                                BFilePathWhatsApp = path + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            }
                            else
                            {
                                File.WriteAllBytes(path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                                message.Attachments.Add(new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf"));
                                BFilePathWhatsApp = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            }
                        }
                        else
                        {
                            DirectoryInfo di = Directory.CreateDirectory(path);
                            File.WriteAllBytes(path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                            BFilePathWhatsApp = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                        }

                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        CloudConfigurationManager.GetSetting("StorageConnectionString"));
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer container = blobClient.GetContainerReference("bookingconfirmations");
                        var blob = container.GetBlockBlobReference("Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf");
                        try
                        {
                            using (var filestream = File.OpenRead(BFilePathWhatsApp))
                            {
                                blob.Properties.ContentType = "application/pdf";
                                blob.UploadFromStream(filestream);
                            }
                            //File.Delete(path);

                            AzureBlobPdfURl = blob.SnapshotQualifiedUri.AbsoluteUri;


                        }
                        catch (System.Exception e)
                        {
                            throw e;
                        }
                        message.Body = MailContent;
                        message.IsBodyHtml = true;


                    }
                    #endregion
                    #region
                    else
                    {
                        if (ds.Tables[10].Rows.Count > 0)
                        {
                            message.From = new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][0].ToString(), "", System.Text.Encoding.UTF8);
                        }
                        else
                        {
                            message.From = new System.Net.Mail.MailAddress("stay@hummingbirdindia.com", "", System.Text.Encoding.UTF8);
                        }
                        if (All.ResendFlag == true)
                        {
                            var Mail = All.PropertyGusetEmail.Split(',');
                            for (int i = 0; i < Mail.Length; i++)
                            {
                                try
                                {
                                    message.To.Add(new System.Net.Mail.MailAddress(Mail[i].ToString()));
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + Mail[i].ToString());
                                }
                            }
                            if (All.UserEmail != "")
                            {
                                try
                                {
                                    message.CC.Add(new System.Net.Mail.MailAddress(All.UserEmail));
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + All.UserEmail);
                                }
                            }
                            message.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                        }
                        else
                        {
                            if (ds.Tables[4].Rows[0][0].ToString() == "0")
                            {
                                if (ds.Tables[8].Rows[0][0].ToString() != "")
                                {
                                    message.To.Add(new System.Net.Mail.MailAddress(ds.Tables[8].Rows[0][0].ToString()));
                                }
                            }
                            else
                            {
                                for (int i = 0; i < ds.Tables[5].Rows.Count; i++)
                                {
                                    if (i <= 40)
                                    {
                                        if (ds.Tables[5].Rows[i][0].ToString() != "")
                                        {
                                            message.To.Add(new System.Net.Mail.MailAddress(ds.Tables[5].Rows[i][0].ToString()));
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                if (ds.Tables[8].Rows[0][0].ToString() != "")
                                {
                                    try
                                    {
                                        message.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[8].Rows[0][0].ToString()));
                                    }
                                    catch (Exception wer)
                                    {
                                        CreateLogFiles log = new CreateLogFiles();
                                        log.ErrorLog("=> Confirmation Email API => Room Email => Invalid Email => CC => " + ds.Tables[8].Rows[0][0].ToString() +
                                            " => BookingId => " + All.BookingId + ", Err Msg => " + wer.Message);
                                    }

                                }
                            }
                            //////Extra CC
                            ////for (int i = 0; i < ds.Tables[7].Rows.Count; i++)
                            ////{
                            ////    if (ds.Tables[7].Rows[i][0].ToString() != "")
                            ////    {
                            ////        try
                            ////        {
                            ////            message.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[7].Rows[i][0].ToString()));
                            ////        }
                            ////        catch (Exception wer)
                            ////        {
                            ////            CreateLogFiles log = new CreateLogFiles();
                            ////            log.ErrorLog("=> Confirmation Email API => Room Email => Invalid Email => Extra CC => " + ds.Tables[7].Rows[i][0].ToString() +
                            ////                " => BookingId => " + All.BookingId + ", Err Msg => " + wer.Message);
                            ////        }
                            ////    }
                            ////}
                            //////Extra CC email from Front end
                            ////if (ds.Tables[8].Rows[0][2].ToString() != "")
                            ////{
                            ////    string ExtraCC = ds.Tables[8].Rows[0][2].ToString();
                            ////    var ExtraCCEmail = ExtraCC.Split(',');
                            ////    int cnt = ExtraCCEmail.Length;
                            ////    for (int i = 0; i < cnt; i++)
                            ////    {
                            ////        if (ExtraCCEmail[i].ToString() != "")
                            ////        {
                            ////            try
                            ////            {
                            ////                message.CC.Add(new System.Net.Mail.MailAddress(ExtraCCEmail[i].ToString()));
                            ////            }
                            ////            catch (Exception wer)
                            ////            {
                            ////                CreateLogFiles log = new CreateLogFiles();
                            ////                log.ErrorLog("=> Confirmation Email API => Room Email => Invalid Email => Extra CC email from Front end => " + ExtraCCEmail[i].ToString() +
                            ////                    " => BookingId => " + All.BookingId + ", Err Msg => " + wer.Message);
                            ////            }
                            ////        }
                            ////    }
                            ////}
                            ////if (ds.Tables[2].Rows[0][4].ToString() != "")
                            ////{
                            ////    try
                            ////    {
                            ////        message.Bcc.Add(new System.Net.Mail.MailAddress(ds.Tables[2].Rows[0][4].ToString()));
                            ////    }
                            ////    catch (Exception wer)
                            ////    {
                            ////        CreateLogFiles log = new CreateLogFiles();
                            ////        log.ErrorLog("=> Confirmation Email API => Room Email => Invalid Email => Bcc => " + ds.Tables[2].Rows[0][4].ToString() +
                            ////            " => BookingId => " + All.BookingId + ", Err Msg => " + wer.Message);
                            ////    }
                            ////}
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][0].ToString()));
                            ////if (ds.Tables[10].Rows[0][0].ToString() != "stay@hummingbirdindia.com")
                            ////{
                            ////    message.Bcc.Add(new System.Net.Mail.MailAddress("stay@hummingbirdindia.com"));
                            ////}
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                        }
                        message.Bcc.Add(new System.Net.Mail.MailAddress("prabakaran@warblerit.com"));

                        message.Subject = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString();
                        string typeofpty = ds.Tables[4].Rows[0][8].ToString();
                        string Imagelocation = "";
                        string Imagealt = "";
                        if (typeofpty == "MGH")
                        {
                            Imagelocation = ds.Tables[6].Rows[0][4].ToString();
                            Imagealt = ds.Tables[6].Rows[0][5].ToString();
                            if (Imagelocation == "")
                            {
                                Imagelocation = ds.Tables[6].Rows[0][0].ToString();
                                Imagealt = ds.Tables[6].Rows[0][1].ToString();
                            }
                        }
                        else
                        {
                            Imagelocation = ds.Tables[6].Rows[0][0].ToString();
                            Imagealt = ds.Tables[6].Rows[0][1].ToString();
                        }

                        // Contact Email And Phone
                        string ContactEmail = "";
                        string DeskNo = "";
                        DeskNo = ds.Tables[2].Rows[0][14].ToString();
                        ContactEmail = ds.Tables[2].Rows[0][16].ToString();


                        // Map Link
                        string MapLink = "";
                        if (ds.Tables[1].Rows[0][13].ToString() != "")
                        {
                            MapLink = "https://www.google.co.in/maps/place/" + ds.Tables[1].Rows[0][13].ToString();
                        }
                        else
                        {
                            MapLink = "#";
                        }

                        // View in browser Link
                        string id = ds.Tables[2].Rows[0][12].ToString();
                        string link = "http://mybooking.hummingbirdindia.com/?redirect=BookingConfirmation&B=R&R=" + id;

                        // Spl Note
                        string SplNote = ds.Tables[2].Rows[0][8].ToString();
                        if (SplNote == "")
                        {
                            SplNote = "- NA -";
                        }

                        string BOKCreditcardView = "";
                        if (typeofpty == "BOK")
                        {
                            BOKCreditcardView = "<tr class=\"\" style=\"padding:0;text-align:left\">" +
                                            "<th style=\"width: 60%;\"><a style = \"font-size:13px; padding:10px 10px 10px 10px;\" align =\"right\" href=" + link + "&C=BOK#Creditcard" + ">UPDATE YOUR CREDIT CARD INFO</a>" +
                                            "</th><th style=\"font-size:10px;padding:10px 16px 10px 0;line-height:28px;text-align:right;\" >" +
                                            "</th></tsr>";

                        }

                        string style = @"<!DOCTYPE html>
                                    <html lang=""en"">
                                    <head>
                                    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
                                    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">  
                                    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge""> 
                                    <meta name=""format-detection"" content=""telephone=no""> 
                                    <title>Booking Confirmation</title>
                                    <link href=""https://fonts.googleapis.com/css?family=Cabin&display=swap"" rel=""stylesheet"">
                                    <style data-roadie-ignore data-immutable=""true"">
                                    @media only screen and (max-width: 596px) {
                                      .small-float-center {
                                        margin: 0 auto !important;
                                        float: none !important;
                                        text-align: center !important;
                                      }
                                      .small-text-center {
                                        text-align: center !important;
                                      }
                                      .small-text-left {
                                        text-align: left !important;
                                      }
                                      .small-text-right {
                                        text-align: right !important;
                                      }
                                    }
                                    @media only screen and (max-width: 596px) {
                                      table.body table.container .hide-for-large {
                                        display: block !important;
                                        width: auto !important;
                                        overflow: visible !important;
                                        max-height: none !important;
                                      }
                                    }
                                   @media only screen and (max-width: 596px) {
                                      table.body table.container .row.hide-for-large,
                                      table.body table.container .row.hide-for-large {
                                        display: table !important;
                                        width: 100% !important;
                                      }
                                    }
                                    @media only screen and (max-width: 596px) {
                                      table.body table.container .show-for-large {
                                        display: none !important;
                                        width: 0;
                                        mso-hide: all;
                                        overflow: hidden;
                                      }
                                    }
                                    @media only screen and (max-width: 596px) {
                                      table.body img {
                                        width: auto !important;
                                        height: auto !important;
                                      }
                                      table.body center {
                                        min-width: 0 !important;
                                      }
                                      table.body .container {
                                        width: 95% !important;
                                      }
                                      table.body .columns,
                                      table.body .column {
                                        height: auto !important;
                                        -moz-box-sizing: border-box;
                                        -webkit-box-sizing: border-box;
                                        box-sizing: border-box;
                                        padding-left: 16px !important;
                                        padding-right: 16px !important;
                                      }
                                      table.body .columns .column,
                                      table.body .columns .columns,
                                      table.body .column .column,
                                      table.body .column .columns {
                                        padding-left: 0 !important;
                                        padding-right: 0 !important;
                                      }
                                      table.body .collapse .columns,
                                      table.body .collapse .column {
                                        padding-left: 0 !important;
                                        padding-right: 0 !important;
                                      }
                                      td.small-1,
                                      th.small-1 {
                                        display: inline-block !important;
                                        width: 8.33333% !important;
                                      }
                                      td.small-2,
                                      th.small-2 {
                                        display: inline-block !important;
                                        width: 16.66667% !important;
                                      }
                                      td.small-3,
                                      th.small-3 {
                                        display: inline-block !important;
                                        width: 25% !important;
                                      }
                                      td.small-4,
                                      th.small-4 {
                                        display: inline-block !important;
                                        width: 33.33333% !important;
                                      }
                                      td.small-5,
                                      th.small-5 {
                                        display: inline-block !important;
                                        width: 41.66667% !important;
                                      }
                                      td.small-6,
                                      th.small-6 {
                                        display: inline-block !important;
                                        width: 50% !important;
                                      }
                                      td.small-7,
                                      th.small-7 {
                                        display: inline-block !important;
                                        width: 58.33333% !important;
                                      }
                                      td.small-8,
                                      th.small-8 {
                                        display: inline-block !important;
                                        width: 66.66667% !important;
                                      }
                                      td.small-9,
                                      th.small-9 {
                                        display: inline-block !important;
                                        width: 75% !important;
                                      }
                                      td.small-10,
                                      th.small-10 {
                                        display: inline-block !important;
                                        width: 83.33333% !important;
                                      }
                                      td.small-11,
                                      th.small-11 {
                                        display: inline-block !important;
                                        width: 91.66667% !important;
                                      }
                                      td.small-12,
                                      th.small-12 {
                                        display: inline-block !important;
                                        width: 100% !important;
                                      }
                                      .columns td.small-12,
                                      .column td.small-12,
                                      .columns th.small-12,
                                      .column th.small-12 {
                                        display: block !important;
                                        width: 100% !important;
                                      }
                                      .body .columns td.small-1,
                                      .body .column td.small-1,
                                      td.small-1 center,
                                      .body .columns th.small-1,
                                      .body .column th.small-1,
                                      th.small-1 center {
                                        display: inline-block !important;
                                        width: 8.33333% !important;
                                      }
                                      .body .columns td.small-2,
                                      .body .column td.small-2,
                                      td.small-2 center,
                                      .body .columns th.small-2,
                                      .body .column th.small-2,
                                      th.small-2 center {
                                        display: inline-block !important;
                                        width: 16.66667% !important;
                                      }
                                      .body .columns td.small-3,
                                      .body .column td.small-3,
                                      td.small-3 center,
                                      .body .columns th.small-3,
                                      .body .column th.small-3,
                                      th.small-3 center {
                                        display: inline-block !important;
                                        width: 25% !important;
                                      }
                                      .body .columns td.small-4,
                                      .body .column td.small-4,
                                      td.small-4 center,
                                      .body .columns th.small-4,
                                      .body .column th.small-4,
                                      th.small-4 center {
                                        display: inline-block !important;
                                        width: 33.33333% !important;
                                      }
                                      .body .columns td.small-5,
                                      .body .column td.small-5,
                                      td.small-5 center,
                                      .body .columns th.small-5,
                                      .body .column th.small-5,
                                      th.small-5 center {
                                        display: inline-block !important;
                                        width: 41.66667% !important;
                                      }
                                      .body .columns td.small-6,
                                      .body .column td.small-6,
                                      td.small-6 center,
                                      .body .columns th.small-6,
                                      .body .column th.small-6,
                                      th.small-6 center {
                                        display: inline-block !important;
                                        width: 50% !important;
                                      }
                                      .body .columns td.small-7,
                                      .body .column td.small-7,
                                      td.small-7 center,
                                      .body .columns th.small-7,
                                      .body .column th.small-7,
                                      th.small-7 center {
                                        display: inline-block !important;
                                        width: 58.33333% !important;
                                      }
                                      .body .columns td.small-8,
                                      .body .column td.small-8,
                                      td.small-8 center,
                                      .body .columns th.small-8,
                                      .body .column th.small-8,
                                      th.small-8 center {
                                        display: inline-block !important;
                                        width: 66.66667% !important;
                                      }
                                      .body .columns td.small-9,
                                      .body .column td.small-9,
                                      td.small-9 center,
                                      .body .columns th.small-9,
                                      .body .column th.small-9,
                                      th.small-9 center {
                                        display: inline-block !important;
                                        width: 75% !important;
                                      }
                                      .body .columns td.small-10,
                                      .body .column td.small-10,
                                      td.small-10 center,
                                      .body .columns th.small-10,
                                      .body .column th.small-10,
                                      th.small-10 center {
                                        display: inline-block !important;
                                        width: 83.33333% !important;
                                      }
                                      .body .columns td.small-11,
                                      .body .column td.small-11,
                                      td.small-11 center,
                                      .body .columns th.small-11,
                                      .body .column th.small-11,
                                      th.small-11 center {
                                        display: inline-block !important;
                                        width: 91.66667% !important;
                                      }
                                      table.body td.small-offset-1,
                                      table.body th.small-offset-1 {
                                        margin-left: 8.33333% !important;
                                        margin-left: 8.33333% !important;
                                      }
                                      table.body td.small-offset-2,
                                      table.body th.small-offset-2 {
                                        margin-left: 16.66667% !important;
                                        margin-left: 16.66667% !important;
                                      }
                                      table.body td.small-offset-3,
                                      table.body th.small-offset-3 {
                                        margin-left: 25% !important;
                                        margin-left: 25% !important;
                                      }
                                      table.body td.small-offset-4,
                                      table.body th.small-offset-4 {
                                        margin-left: 33.33333% !important;
                                        margin-left: 33.33333% !important;
                                      }
                                      table.body td.small-offset-5,
                                      table.body th.small-offset-5 {
                                        margin-left: 41.66667% !important;
                                        margin-left: 41.66667% !important;
                                      }
                                      table.body td.small-offset-6,
                                      table.body th.small-offset-6 {
                                        margin-left: 50% !important;
                                        margin-left: 50% !important;
                                      }
                                      table.body td.small-offset-7,
                                      table.body th.small-offset-7 {
                                        margin-left: 58.33333% !important;
                                        margin-left: 58.33333% !important;
                                      }
                                      table.body td.small-offset-8,
                                      table.body th.small-offset-8 {
                                        margin-left: 66.66667% !important;
                                        margin-left: 66.66667% !important;
                                      }
                                      table.body td.small-offset-9,
                                      table.body th.small-offset-9 {
                                        margin-left: 75% !important;
                                        margin-left: 75% !important;
                                      }
                                      table.body td.small-offset-10,
                                      table.body th.small-offset-10 {
                                        margin-left: 83.33333% !important;
                                        margin-left: 83.33333% !important;
                                      }
                                      table.body td.small-offset-11,
                                      table.body th.small-offset-11 {
                                        margin-left: 91.66667% !important;
                                        margin-left: 91.66667% !important;
                                      }
                                      table.body table.columns td.expander,
                                      table.body table.columns th.expander {
                                        display: none !important;
                                      }
                                      table.body .right-text-pad,
                                      table.body .text-pad-right {
                                        padding-left: 10px !important;
                                      }
                                      table.body .left-text-pad,
                                      table.body .text-pad-left {
                                        padding-right: 10px !important;
                                      }
                                      table.menu {
                                        width: 100% !important;
                                      }
                                      table.menu td,
                                      table.menu th {
                                        width: auto !important;
                                        display: inline-block !important;
                                      }
                                      table.menu.vertical td,
                                      table.menu.vertical th,
                                      table.menu.small-vertical td,
                                      table.menu.small-vertical th {
                                        display: block !important;
                                      }
                                      table.menu[align=""center""] {
                                        width: auto !important;
                                      }
                                      table.button.expand {
                                        width: 100% !important;
                                      }
                                    }
                                    @media only screen and (max-width: 596px) {
                                      .calendar-content {
                                        padding: 0px !important;
                                        width: 288px !important;
                                      }
                                      .not-available-day,
                                      .calendar-today,
                                      .available-day {
                                        height: 40px !important;
                                        width: 40px !important;
                                      }
                                      .day-label {
                                        margin-left: 10% !important;
                                        margin-top: 0% !important;
                                        font-size: 15px;
                                      }
	                                  .p
	                                  {
	                                  font-size:16px
	                                  font-size:4vw
	                                  }
                                    }
                                  </style></head><body style=""font-size:16px;min-width:100%;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;-moz-box-sizing:border-box;-webkit-box-sizing:border-box;box-sizing:border-box;font-family:'Circular', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;margin:0;line-height:1.3;color:#0a0a0a;text-align:left;width:100% !important"">
                                  <div class=""preheader"" style=""mso-hide:all;visibility:hidden;opacity:0;color:transparent;font-size:0px;width:0;height:0;display:none !important""></div>
                                  <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
                                  <meta name=""viewport"" content=""width=device-width"">";

                        string header = "<table class=\"body\" style=\"border-spacing:0;border-collapse:collapse;vertical-align:top;-webkit-hyphens:none;-moz-hyphens:none;hyphens:none;-ms-hyphens:none;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;margin:0;text-align:left;font-size:16px;line-height:19px;background:#f3f3f3;padding:0;width:100%;height:100%;color:#0a0a0a;margin-bottom:0px !important;background-color: white;\">" +
                                        "<tr style=\"padding: 0; vertical-align:top;text-align:left\" >" +
                                        "<td class=\"center\" align=\"center\" valign=\"top\" style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                        "<center style=\"width:100%;min-width:580px\">" +
                                        "<table class=\"container\" style=\"border-spacing:0;border-collapse:collapse;padding:0;vertical-align:top;background:#fefefe;width:580px;margin:0 auto;text-align:inherit;max-width:580px;\">" +
                                        "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                        "<td style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                        "<div style=\"padding-top:10px\">" +
                                        "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                        "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                        "<th style=\"width: 40%;>" +
                                        "<a href=\"https://www.hummingbirdindia.com\" target=\"_blank\" style=\"padding:0;margin:0;text-align:left;line-height:1.3;color:#2199e8;text-decoration:none\">" +
                                        "<img align =\"center\" alt=\"" + Imagealt + "\" class=\"center standard-header\" src=\"" + Imagelocation + "\" style=\"max-width: 120px\" ></a>";

                        string header_cnt1 = " </th> " +
                                             "<th style=\"font-size:16px;padding:20px 0 20px 0;line-height:28px;\">";

                        if (ds.Tables[2].Rows[0][15].ToString() != "")
                        {
                            header_cnt1 += "<p>Hotel Confirmation #: " + ds.Tables[2].Rows[0][15].ToString() + "</p>";
                        }
                        else if (ds.Tables[2].Rows[0][17].ToString() != "")
                        {
                            header_cnt1 += "<p>Confirmed by: " + ds.Tables[2].Rows[0][17].ToString() + "</p>";
                        }




                        header_cnt1 += "<p>HB Confirmation #: " + ds.Tables[2].Rows[0][2].ToString() + "</p>" +
                                             "</th>" +
                                             "</tr>" +
                                             "</table>" +
                                             "</div>";

                        string HotelName = "<div>" +
                                          "<div class=\"headline-body\" style=\"padding-bottom:15px\">" +
                                          "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                          "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                          "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;width:564px;margin:0 auto;padding-left:16px;padding-right:16px;padding-bottom:0px !important\">" +
                                          "<p class=\"body  body-lg body-link-rausch light text-left   \" style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;line-height:1;font-weight:300;color:#484848;font-size:24px;hyphens:none;-ms-hyphens:none;-webkit-hyphens:none;-moz-hyphens:none;text-align:left;margin-bottom:0px !important;color:#0a0a0a;\">" + ds.Tables[1].Rows[0][5].ToString() +
                                          "<br /><span style=\"float: right;font-size:20px;\">Reservation Confirmed</span>" +
                                          "</p>" +
                                          "</th></tr></table></div></div>";

                        string ChkInOutDate = "";

                        if (All.LTIAPIFlag == true)
                        {
                            ChkInOutDate = "<div>" +
                                    "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                    "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                    "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                    "<p class=\"body-text-lg light\" style=\"margin:0;text-align:left;padding:0;font-weight:300;font-family:'Circular', 'Cabin', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important;\">" + All.CheckinDate + "</p>" +
                                    "<p class=\"body-text light\" style=\"margin:0;text-align:left;padding:0;font-weight:300;font-family:'Circular', 'Cabin', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important;\">Check-in " + All.CheckinTime + "</p>" +
                                    "</th>" +
                                    "<th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                    "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                    "</th>" +
                                    "<th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                    "<p class=\"body-text-lg light text-right\" style=\"padding:0;margin:0;font-size:17px;font-weight:300;font-family:'Circular', 'Helvetica', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important;\">" + All.CheckoutDate + "</p>" +
                                    "<p class=\"body-text light text-right\"  style=\"padding:0;margin:0;font-size:16px;font-weight:300;font-family:'Circular', 'Helvetica', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important;\">Check-out</p>" +
                                    "</th></tr></table></div>" +
                                    "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                    "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                    "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                    "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                    "</th></tr></table></div>" +
                                    "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                    "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                    "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                    "<th class=\"col-pad-left-2 col-pad-right-2\" style=\"padding:0;margin:0;padding-left:16px;padding-right:16px\">" +
                                    "<div style=\"margin:0;-webkit-border-radius:8px;border-radius:8px;display:block;border-color:#d9242c;border-width:2px;border-style:dotted dashed;\">" +
                                    "<p class=\"text-center\" style='font-weight:normal;padding:0;margin:0;text-align:center;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;font-size:24px;line-height:32px;margin-bottom:0px !important'>Guest Details</p>" +
                                    "</div></th></tr></table></div>";
                        }
                        else
                        {
                            ChkInOutDate = "<div>" +
                                    "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                    "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                    "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                    "<p class=\"body-text-lg light\" style=\"margin:0;text-align:left;padding:0;font-weight:300;font-family:'Circular', 'Cabin', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important;\">" + ds.Tables[0].Rows[0][9].ToString() + "</p>" +
                                    "<p class=\"body-text light\" style=\"margin:0;text-align:left;padding:0;font-weight:300;font-family:'Circular', 'Cabin', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important;\">Check-in " + ds.Tables[0].Rows[0][11].ToString() + "</p>" +
                                    "</th>" +
                                    "<th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                    "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                    "</th>" +
                                    "<th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                    "<p class=\"body-text-lg light text-right\" style=\"padding:0;margin:0;font-size:17px;font-weight:300;font-family:'Circular', 'Helvetica', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important;\">" + ds.Tables[0].Rows[0][10].ToString() + "</p>" +
                                    "<p class=\"body-text light text-right\"  style=\"padding:0;margin:0;font-size:16px;font-weight:300;font-family:'Circular', 'Helvetica', Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important;\">Check-out</p>" +
                                    "</th></tr></table></div>" +
                                    "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                    "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                    "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                    "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                    "</th></tr></table></div>" +
                                    "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                    "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                    "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                    "<th class=\"col-pad-left-2 col-pad-right-2\" style=\"padding:0;margin:0;padding-left:16px;padding-right:16px\">" +
                                    "<div style=\"margin:0;-webkit-border-radius:8px;border-radius:8px;display:block;border-color:#d9242c;border-width:2px;border-style:dotted dashed;\">" +
                                    "<p class=\"text-center\" style='font-weight:normal;padding:0;margin:0;text-align:center;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;font-size:24px;line-height:32px;margin-bottom:0px !important'>Guest Details</p>" +
                                    "</div></th></tr></table></div>";

                        }



                        string TablHdr = "<div style=\"padding-top:8px;padding-bottom:8px;padding-left:16px;padding-right:16px;\">" +
                                         "<table rules=\"rows\" style=\"border:#dbdbdb\"><tr>" +
                                         "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\"><strong>Guest Name</strong></td>" +
                                         "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\"><strong>Room No</strong></td>" +
                                         "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\"><strong>Occupancy</strong></td>" +
                                         "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\"><strong>Tariff / <br>Room / Day</strong></td>" +
                                         "</tr><tr></tr>";

                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            TablHdr +=
                                "<tr style=\"font-style:normal;font-weight:normal;\" class=\"ng-scope\">" +
                                "<td class=\"padd ng-binding\" style=\"vertical-align:middle;text-align:center \">" + ds.Tables[0].Rows[i][0].ToString() + " </td>" +
                                "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[0].Rows[i][7].ToString() + "</td>" +
                                "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[0].Rows[i][4].ToString() + "</td>" +
                                "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">INR " + ds.Tables[0].Rows[i][3].ToString() + "</td>" +
                                "</tr>";
                        }

                        TablHdr += "</table><div><div class=\"row-pad-bot-1\" style=\"padding-bottom:8px !important;padding-top:6px;\"></div>" +
                                    "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                    "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                    "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                    "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:18px;margin-bottom:0px !important;'>Tariff Payment: " + ds.Tables[0].Rows[0][5].ToString() + "</p></th>" +
                                    "<th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                    "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\"></th>" +
                                    "<th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                    "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:18px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important;'>Service Payment: " + ds.Tables[0].Rows[0][6].ToString() + "</p>" +
                                    "</th></tr></table></div></div>";

                        string Inclusions = "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                    "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                    "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                    "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                    "</th></tr></table></div>" +
                                    "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                    "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                    "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                    "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:18px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#0a0a0a;word-break:normal;line-height:1.2;\"><strong>Inclusions :</strong> " + ds.Tables[1].Rows[0][12].ToString() + " </p>" +
                                    "</th></tr></table>";

                        string Note = "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                    "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                    "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                    "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                    "</th></tr></table></div>" +
                                    "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                    "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                    "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                    "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-weight:300;font-size:14px;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#0a0a0a;word-break:normal;line-height:1.2;\"><strong>Note : </strong>" + SplNote + " </p>" +
                                    "</th></tr></table>";

                        string Address = "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                           "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                            "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                            "</th></tr></table></div>" +
                                            "<div style =\"padding-top:8px;padding-bottom:8px\">" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:24px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#0a0a0a;;word-break:normal;line-height:1.2;\">Address</p>" +
                                            "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:18px;margin-bottom:0px !important'>" + ds.Tables[1].Rows[0][0].ToString() + "</p>" +
                                            "<p class=\"body-text light\">" + ds.Tables[1].Rows[0][1].ToString() + "</p></th>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<a href=\"" + MapLink + "\" style =\"font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;margin:0;text-align:left;line-height:1.3;color:#2199e8;text-decoration:none\">" +
                                            "<p class=\"body-text-lg light color-rausch text-right\" style='padding:0;margin:0 0 5px 0;word-break:normal;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;line-height:1.2;font-size:24px;text-align:center;color:#d9242c !important;margin-bottom:0px !important'>Get Directions</p>" +
                                            "<p class=\"body-text light\" style='margin:0;text-align:center;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:18px;margin-bottom:0px !important'>" +
                                            "<img style =\"width:36px;height:36px;\" src=\"https://portalvhds4prl9ymlwxnt8.blob.core.windows.net/img/Google_Maps_Icon.png\"/></p></a></th></table></div><div>" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                            "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                            "</th></tr></table></div>";

                        string BookerDtls = "<div style=\"padding-top:8px;padding-bottom:8px\" >" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' > Booked by</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][3].ToString() + " </ p >" +
                                            "</th>" +
                                            "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<hr>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<hr>" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' >Client Request #</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][13].ToString() + " </ p >" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<hr>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<hr>" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' >Issues / feedbacks</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + DeskNo + "<br>" + ContactEmail + " </ p >" +
                                            "</th>" +
                                            "</tr></table></div>";

                        string GSTDtls = "";

                        if (ds.Tables[0].Rows[0][5].ToString() == "Direct<br>(Cash/Card)")
                        {
                            GSTDtls = "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                      "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                      "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:5px;padding-left:16px\">" +
                                      "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:18px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#000;word-break:normal;line-height:1.2;\"> <strong> GSTIN Details for Billing <br> <span style=\"font-size:12px;\">(If GST No. is not mentioned, kindly enquire with the Client) </span></strong> </p>" +
                                      "</th></tr></table>" +
                                      "<table><tr>" +
                                      "<td style=\"font-size:13px; width:25%;\" valign=\"top\" align=\"center\"><strong> GST Number </strong></td>" +
                                      "<td style=\"font-size:13px; width:25%;\" valign=\"top\" align=\"center\"><strong> Legal Name </strong></td>" +
                                      "<td style=\"font-size:13px; width:49%;\" valign=\"top\" align=\"center\"><strong> Address </strong></td>" +
                                      "</tr><tr><td colspan=\"3\"><hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\"></td></tr>" +
                                      "<tr style=\"font-style:normal;font-size:13px;\" class=\"ng-scope\">" +
                                      "<td class=\"padd ng-binding\" style=\"vertical-align:middle;text-align:center \">" + ds.Tables[12].Rows[0][1].ToString() + " </td>" +
                                      "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[12].Rows[0][0].ToString() + "</td>" +
                                      "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[12].Rows[0][2].ToString() + "</td>" +
                                      "</tr></table></div>" +
                                      "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                      "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                      "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                      "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                      "</th></tr></table></div>";






                            ////"<div>" +
                            ////        "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                            ////        "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                            ////        "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                            ////        "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                            ////        "</th></tr></table></div>"+
                            ////        "<div style =\"padding-top:8px;padding-bottom:8px\" >" +
                            ////        "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                            ////        "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                            ////        "<p style = 'margin:0;text-align:left;padding:0;' > GST Number </p></ th >" +
                            ////        "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                            ////        "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[12].Rows[0][0].ToString() + " </ p >" +
                            ////        "</th></table>" +
                            ////        "<hr>" +
                            ////        "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                            ////        "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                            ////        "<p style = 'margin:0;text-align:left;padding:0;' > Legal Name </p></ th >" +
                            ////        "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                            ////        "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[12].Rows[0][1].ToString() + " </ p >" +
                            ////        "</th></table>" +
                            ////        "<hr>" +
                            ////        "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                            ////        "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                            ////        "<p style = 'margin:0;text-align:left;padding:0;' > Address </p></ th >" +
                            ////        "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                            ////        "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[12].Rows[0][2].ToString() + " </ p >" +
                            ////        "</th></table>" +
                            ////        "<hr>" +
                            ////        "</div>";

                        }


                        string FooterDtls = "<div>" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                            "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                            "</th></tr></table></div><div style =\"padding-top:2px\" >" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;padding:0;width:100%;position:relative;display:table\">" +
                                            BOKCreditcardView +
                                            "<tr class=\"\" style=\"padding:0;text-align:left\">" +
                                            "<th style=\"width: 60%;\"><a href=\"" + link + "\" target=\"_blank\"> <span style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:10px 0 10px 16px;margin:0;text-align:left;line-height:1.3;text-decoration:none;font-weight:300;color:#d9242c !important\">Security/Cancellation Policy</span></a>" +
                                            "</th><th style=\"font-size:10px;padding:10px 16px 10px 0;line-height:28px;text-align:right;\" >" +
                                            "<p> Powered by Staysimplyfied.com</p></th></tr></table></div></tr></table></div></td></tr></table></center></td></tr></table>";
                        string EndData = "</body></html>";

                        var PdfContent = "";
                        if (ds.Tables[0].Rows[0][5].ToString() == "Direct<br>(Cash/Card)")
                        {
                            MailContent = style + header + header_cnt1 + HotelName + ChkInOutDate + TablHdr + Inclusions + Note + Address + GSTDtls + BookerDtls + FooterDtls + EndData;
                            PdfContent = header + header_cnt1 + HotelName + ChkInOutDate + TablHdr + Inclusions + Note + Address + GSTDtls + BookerDtls + FooterDtls;
                        }
                        else
                        {
                            MailContent = style + header + header_cnt1 + HotelName + ChkInOutDate + TablHdr + Inclusions + Note + Address + BookerDtls + FooterDtls + EndData;
                            PdfContent = header + header_cnt1 + HotelName + ChkInOutDate  + TablHdr + Inclusions + Note + Address + BookerDtls + FooterDtls;
                        }

                   

                        var htmlContent = String.Format(PdfContent, DateTime.Now);
                        var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
                        var pdfBytes = htmlToPdf.GeneratePdf(htmlContent);
                        string path = @"D:\home\site\wwwroot\Confirmations\";
                        var RFilePathWhatsApp = "";
                        if (Directory.Exists(path))
                        {
                            var FileName = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            if (File.Exists(FileName))
                            {
                                var AttachmentName = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                                long ticks = DateTime.Now.Ticks;
                                byte[] bytes = BitConverter.GetBytes(ticks);
                                Newid = Convert.ToBase64String(bytes).Replace('+', '_').Replace('/', '-').TrimEnd('=');
                                File.WriteAllBytes(path + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                                System.Net.Mail.Attachment att1 = new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf");
                                att1.Name = AttachmentName;
                                RFilePathWhatsApp = path + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                                message.Attachments.Add(att1);

                            }
                            else
                            {
                                File.WriteAllBytes(path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                                message.Attachments.Add(new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf"));
                                RFilePathWhatsApp = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            }
                        }
                        else
                        {
                            DirectoryInfo di = Directory.CreateDirectory(path);
                            File.WriteAllBytes(path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                            RFilePathWhatsApp = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                        }
                        message.Body = MailContent;
                        message.IsBodyHtml = true;
                        if (ds.Tables[2].Rows[0][11].ToString() == "218" && ds.Tables[0].Rows[0][5].ToString() == "Direct<br>(Cash/Card)")
                        {
                            message.Attachments.Add(new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "icici_letter.pdf"));
                        }

                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        CloudConfigurationManager.GetSetting("StorageConnectionString"));
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer container = blobClient.GetContainerReference("bookingconfirmations");
                        var blob = container.GetBlockBlobReference("Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf");
                        try
                        {
                            using (var filestream = File.OpenRead(RFilePathWhatsApp))
                            {
                                blob.Properties.ContentType = "application/pdf";
                                blob.UploadFromStream(filestream);
                            }
                            //var FileNames = @"D:\home\site\wwwroot\Confirmations\" + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            //File.Delete(FileNames);                            
                            AzureBlobPdfURl = blob.SnapshotQualifiedUri.AbsoluteUri;


                        }
                        catch (System.Exception e)
                        {
                            throw e;
                        }
                    }
                    #endregion

                    try
                    {
                        smtp.Send(message);
                        Response1 = "Success";
                    }
                    catch (Exception ex)
                    {
                        CreateLogFiles log = new CreateLogFiles();
                        log.ErrorLog("=> Guest Confirmation Mail => smtp => BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);
                        Response1 = "Failure";
                    }
                }
                else
                {
                    Response1 = "Success";
                }
                #endregion

                #region
                if (All.PropertyMailChk == true)
                {
                    System.Net.Mail.MailMessage message1 = new System.Net.Mail.MailMessage();
                    System.Net.Mail.SmtpClient smtp1 = new System.Net.Mail.SmtpClient();
                    smtp1.Port = Port;
                    smtp1.Host = Host;
                    smtp1.Credentials = new System.Net.NetworkCredential(CredentialsUserName, CredentialsPassword);
                    smtp1.EnableSsl = true;
                    string MailContent = "";

                    #region
                    if (ds.Tables[0].Rows[0][8].ToString() == "Bed")
                    {

                        var ChCnt = 0;
                        var ChCntVal = "txt";

                        if (All.ResendFlag == true)
                        {
                            ChCnt = 1;
                            ChCntVal = "txt";
                        }
                        else
                        {
                            ChCnt = ds.Tables[3].Rows.Count;
                            ChCntVal = ds.Tables[3].Rows[0][4].ToString();
                        }

                        if (ChCnt > 0)
                        {
                            if (ChCntVal != "")
                            {
                                string PropertyMail = ds.Tables[3].Rows[0][4].ToString();
                                var PtyMail = PropertyMail.Split(',');
                                int cnt = PtyMail.Length;

                                if (ds.Tables[10].Rows.Count > 0)
                                {
                                    message1.From = new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][1].ToString(), "", System.Text.Encoding.UTF8);
                                }
                                else
                                {
                                    message1.From = new System.Net.Mail.MailAddress("stay@hummingbirdindia.com", "", System.Text.Encoding.UTF8);
                                }

                                if (All.ResendFlag == true)
                                {
                                    var Mail = All.PropertyGusetEmail.Split(',');
                                    for (int i = 0; i < Mail.Length; i++)
                                    {
                                        try
                                        {
                                            message1.To.Add(new System.Net.Mail.MailAddress(Mail[i].ToString()));
                                        }
                                        catch (Exception ex)
                                        {
                                            CreateLogFiles log = new CreateLogFiles();
                                            log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + Mail[i].ToString());
                                        }
                                    }
                                    if (All.UserEmail != "")
                                    {
                                        try
                                        {
                                            message1.CC.Add(new System.Net.Mail.MailAddress(All.UserEmail));
                                        }
                                        catch (Exception ex)
                                        {
                                            CreateLogFiles log = new CreateLogFiles();
                                            log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + All.UserEmail);
                                        }
                                    }


                                    message1.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));

                                }
                                else
                                {

                                    for (int i = 0; i < cnt; i++)
                                    {
                                        if (PtyMail[i].ToString() != "")
                                        {
                                            try
                                            {
                                                message1.To.Add(new System.Net.Mail.MailAddress(PtyMail[i].ToString()));
                                            }
                                            catch (Exception ex)
                                            {
                                                CreateLogFiles log = new CreateLogFiles();
                                                log.ErrorLog("=> Confirmation Email API => Bed Property Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + PtyMail[i].ToString());
                                            }
                                        }
                                    }
                                    ////for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                                    ////{
                                    ////    if (ds.Tables[3].Rows[i][2].ToString() != "")
                                    ////    {
                                    ////        try
                                    ////        {
                                    ////            message1.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[3].Rows[i][2].ToString()));
                                    ////        }
                                    ////        catch (Exception ex)
                                    ////        {
                                    ////            CreateLogFiles log = new CreateLogFiles();
                                    ////            log.ErrorLog("=> Confirmation Email API => Bed Property Email => BookingId => " + All.BookingId + " => Invaild Email => Cc =>" + ds.Tables[3].Rows[i][2].ToString());
                                    ////        }
                                    ////    }
                                    ////}
                                    ////if (ds.Tables[2].Rows[0][4].ToString() != "")
                                    ////{
                                    ////    try
                                    ////    {
                                    ////        message1.Bcc.Add(ds.Tables[2].Rows[0][4].ToString());
                                    ////    }
                                    ////    catch (Exception ex)
                                    ////    {
                                    ////        CreateLogFiles log = new CreateLogFiles();
                                    ////        log.ErrorLog("=> Confirmation Email API => Bed Property Email => BookingId => " + All.BookingId + " => Invaild Email => Bcc =>" + ds.Tables[2].Rows[0][4].ToString());
                                    ////    }
                                    ////}
                                    ////message1.Bcc.Add(new System.Net.Mail.MailAddress("bookingbcc@staysimplyfied.com"));
                                    ////message1.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));

                                }
                                message1.Bcc.Add(new System.Net.Mail.MailAddress("prabakaran@warblerit.com"));

                                message1.Subject = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString();


                                string Imagelocation1 = "";
                                string Imagealt1 = "";
                                string PtyType1 = ds.Tables[5].Rows[0][1].ToString();
                                if (PtyType1 == "MGH")
                                {
                                    Imagelocation1 = ds.Tables[6].Rows[0][4].ToString();
                                    Imagealt1 = ds.Tables[6].Rows[0][5].ToString();
                                    if (Imagelocation1 == "")
                                    {
                                        Imagelocation1 = ds.Tables[6].Rows[0][0].ToString();
                                        Imagealt1 = ds.Tables[6].Rows[0][3].ToString();
                                    }
                                }
                                else
                                {
                                    Imagelocation1 = ds.Tables[6].Rows[0][0].ToString();
                                    Imagealt1 = ds.Tables[6].Rows[0][3].ToString();
                                }

                                // Desk No
                                string DeskNo = "";
                                DeskNo = ds.Tables[2].Rows[0][13].ToString();


                                // Guest Mobile No.
                                string MobileNo = ds.Tables[4].Rows[0][2].ToString();
                                if (MobileNo == "")
                                {
                                    MobileNo = " - NA - ";
                                }


                                // Spl Note
                                string SplNote = ds.Tables[2].Rows[0][8].ToString();
                                if (SplNote == "")
                                {
                                    SplNote = "- NA -";
                                }

                                // View in browser Link
                                string id = ds.Tables[2].Rows[0][11].ToString();
                                string link = "http://mybooking.hummingbirdindia.com/?redirect=BookingPropertyConfirmation&B=B&R=" + id;


                                string style = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
                                                <html xmlns=""http://www.w3.org/1999/xhtml"" style=""min-height:100%;background:#f3f3f3"">
                                                <head><meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8""></head>
                                                <body style=""font-size:16px;min-width:100%;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;-moz-box-sizing:border-box;-webkit-box-sizing:border-box;box-sizing:border-box;font-family:'Circular', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;margin:0;line-height:1.3;color:#0a0a0a;text-align:left;width:100% !important"">
                                                <div class=""preheader"" style=""mso-hide:all;visibility:hidden;opacity:0;color:transparent;font-size:0px;width:0;height:0;display:none !important""></div>
                                                <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
                                                <meta name=""viewport"" content=""width=device-width"">
                                                <link href=""https://fonts.googleapis.com/css?family=Cabin&display=swap"" rel=""stylesheet"">
                                                <style data-roadie-ignore data-immutable=""true"">
                                                    @media only screen and (max-width: 596px) {
                                                      .small-float-center {
                                                        margin: 0 auto !important;
                                                        float: none !important;
                                                        text-align: center !important;
                                                      }
                                                      .small-text-center {
                                                        text-align: center !important;
                                                      }
                                                      .small-text-left {
                                                        text-align: left !important;
                                                      }
                                                      .small-text-right {
                                                        text-align: right !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .hide-for-large {
                                                        display: block !important;
                                                        width: auto !important;
                                                        overflow: visible !important;
                                                        max-height: none !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .row.hide-for-large,
                                                      table.body table.container .row.hide-for-large {
                                                        display: table !important;
                                                        width: 100% !important;
                                                      }
                                                    }
    
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .show-for-large {
                                                        display: none !important;
                                                        width: 0;
                                                        mso-hide: all;
                                                        overflow: hidden;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body img {
                                                        width: auto !important;
                                                        height: auto !important;
                                                      }
                                                      table.body center {
                                                        min-width: 0 !important;
                                                      }
                                                      table.body .container {
                                                        width: 95% !important;
                                                      }
                                                      table.body .columns,
                                                      table.body .column {
                                                        height: auto !important;
                                                        -moz-box-sizing: border-box;
                                                        -webkit-box-sizing: border-box;
                                                        box-sizing: border-box;
                                                        padding-left: 16px !important;
                                                        padding-right: 16px !important;
                                                      }
                                                      table.body .columns .column,
                                                      table.body .columns .columns,
                                                      table.body .column .column,
                                                      table.body .column .columns {
                                                        padding-left: 0 !important;
                                                        padding-right: 0 !important;
                                                      }
                                                      table.body .collapse .columns,
                                                      table.body .collapse .column {
                                                        padding-left: 0 !important;
                                                        padding-right: 0 !important;
                                                      }
                                                      td.small-1,
                                                      th.small-1 {
                                                        display: inline-block !important;
                                                        width: 8.33333% !important;
                                                      }
                                                      td.small-2,
                                                      th.small-2 {
                                                        display: inline-block !important;
                                                        width: 16.66667% !important;
                                                      }
                                                      td.small-3,
                                                      th.small-3 {
                                                        display: inline-block !important;
                                                        width: 25% !important;
                                                      }
                                                      td.small-4,
                                                      th.small-4 {
                                                        display: inline-block !important;
                                                        width: 33.33333% !important;
                                                      }
                                                      td.small-5,
                                                      th.small-5 {
                                                        display: inline-block !important;
                                                        width: 41.66667% !important;
                                                      }
                                                      td.small-6,
                                                      th.small-6 {
                                                        display: inline-block !important;
                                                        width: 50% !important;
                                                      }
                                                      td.small-7,
                                                      th.small-7 {
                                                        display: inline-block !important;
                                                        width: 58.33333% !important;
                                                      }
                                                      td.small-8,
                                                      th.small-8 {
                                                        display: inline-block !important;
                                                        width: 66.66667% !important;
                                                      }
                                                      td.small-9,
                                                      th.small-9 {
                                                        display: inline-block !important;
                                                        width: 75% !important;
                                                      }
                                                      td.small-10,
                                                      th.small-10 {
                                                        display: inline-block !important;
                                                        width: 83.33333% !important;
                                                      }
                                                      td.small-11,
                                                      th.small-11 {
                                                        display: inline-block !important;
                                                        width: 91.66667% !important;
                                                      }
                                                      td.small-12,
                                                      th.small-12 {
                                                        display: inline-block !important;
                                                        width: 100% !important;
                                                      }
                                                      .columns td.small-12,
                                                      .column td.small-12,
                                                      .columns th.small-12,
                                                      .column th.small-12 {
                                                        display: block !important;
                                                        width: 100% !important;
                                                      }
                                                      .body .columns td.small-1,
                                                      .body .column td.small-1,
                                                      td.small-1 center,
                                                      .body .columns th.small-1,
                                                      .body .column th.small-1,
                                                      th.small-1 center {
                                                        display: inline-block !important;
                                                        width: 8.33333% !important;
                                                      }
                                                      .body .columns td.small-2,
                                                      .body .column td.small-2,
                                                      td.small-2 center,
                                                      .body .columns th.small-2,
                                                      .body .column th.small-2,
                                                      th.small-2 center {
                                                        display: inline-block !important;
                                                        width: 16.66667% !important;
                                                      }
                                                      .body .columns td.small-3,
                                                      .body .column td.small-3,
                                                      td.small-3 center,
                                                      .body .columns th.small-3,
                                                      .body .column th.small-3,
                                                      th.small-3 center {
                                                        display: inline-block !important;
                                                        width: 25% !important;
                                                      }
                                                      .body .columns td.small-4,
                                                      .body .column td.small-4,
                                                      td.small-4 center,
                                                      .body .columns th.small-4,
                                                      .body .column th.small-4,
                                                      th.small-4 center {
                                                        display: inline-block !important;
                                                        width: 33.33333% !important;
                                                      }
                                                      .body .columns td.small-5,
                                                      .body .column td.small-5,
                                                      td.small-5 center,
                                                      .body .columns th.small-5,
                                                      .body .column th.small-5,
                                                      th.small-5 center {
                                                        display: inline-block !important;
                                                        width: 41.66667% !important;
                                                      }
                                                      .body .columns td.small-6,
                                                      .body .column td.small-6,
                                                      td.small-6 center,
                                                      .body .columns th.small-6,
                                                      .body .column th.small-6,
                                                      th.small-6 center {
                                                        display: inline-block !important;
                                                        width: 50% !important;
                                                      }
                                                      .body .columns td.small-7,
                                                      .body .column td.small-7,
                                                      td.small-7 center,
                                                      .body .columns th.small-7,
                                                      .body .column th.small-7,
                                                      th.small-7 center {
                                                        display: inline-block !important;
                                                        width: 58.33333% !important;
                                                      }
                                                      .body .columns td.small-8,
                                                      .body .column td.small-8,
                                                      td.small-8 center,
                                                      .body .columns th.small-8,
                                                      .body .column th.small-8,
                                                      th.small-8 center {
                                                        display: inline-block !important;
                                                        width: 66.66667% !important;
                                                      }
                                                      .body .columns td.small-9,
                                                      .body .column td.small-9,
                                                      td.small-9 center,
                                                      .body .columns th.small-9,
                                                      .body .column th.small-9,
                                                      th.small-9 center {
                                                        display: inline-block !important;
                                                        width: 75% !important;
                                                      }
                                                      .body .columns td.small-10,
                                                      .body .column td.small-10,
                                                      td.small-10 center,
                                                      .body .columns th.small-10,
                                                      .body .column th.small-10,
                                                      th.small-10 center {
                                                        display: inline-block !important;
                                                        width: 83.33333% !important;
                                                      }
                                                      .body .columns td.small-11,
                                                      .body .column td.small-11,
                                                      td.small-11 center,
                                                      .body .columns th.small-11,
                                                      .body .column th.small-11,
                                                      th.small-11 center {
                                                        display: inline-block !important;
                                                        width: 91.66667% !important;
                                                      }
                                                      table.body td.small-offset-1,
                                                      table.body th.small-offset-1 {
                                                        margin-left: 8.33333% !important;
                                                        margin-left: 8.33333% !important;
                                                      }
                                                      table.body td.small-offset-2,
                                                      table.body th.small-offset-2 {
                                                        margin-left: 16.66667% !important;
                                                        margin-left: 16.66667% !important;
                                                      }
                                                      table.body td.small-offset-3,
                                                      table.body th.small-offset-3 {
                                                        margin-left: 25% !important;
                                                        margin-left: 25% !important;
                                                      }
                                                      table.body td.small-offset-4,
                                                      table.body th.small-offset-4 {
                                                        margin-left: 33.33333% !important;
                                                        margin-left: 33.33333% !important;
                                                      }
                                                      table.body td.small-offset-5,
                                                      table.body th.small-offset-5 {
                                                        margin-left: 41.66667% !important;
                                                        margin-left: 41.66667% !important;
                                                      }
                                                      table.body td.small-offset-6,
                                                      table.body th.small-offset-6 {
                                                        margin-left: 50% !important;
                                                        margin-left: 50% !important;
                                                      }
                                                      table.body td.small-offset-7,
                                                      table.body th.small-offset-7 {
                                                        margin-left: 58.33333% !important;
                                                        margin-left: 58.33333% !important;
                                                      }
                                                      table.body td.small-offset-8,
                                                      table.body th.small-offset-8 {
                                                        margin-left: 66.66667% !important;
                                                        margin-left: 66.66667% !important;
                                                      }
                                                      table.body td.small-offset-9,
                                                      table.body th.small-offset-9 {
                                                        margin-left: 75% !important;
                                                        margin-left: 75% !important;
                                                      }
                                                      table.body td.small-offset-10,
                                                      table.body th.small-offset-10 {
                                                        margin-left: 83.33333% !important;
                                                        margin-left: 83.33333% !important;
                                                      }
                                                      table.body td.small-offset-11,
                                                      table.body th.small-offset-11 {
                                                        margin-left: 91.66667% !important;
                                                        margin-left: 91.66667% !important;
                                                      }
                                                      table.body table.columns td.expander,
                                                      table.body table.columns th.expander {
                                                        display: none !important;
                                                      }
                                                      table.body .right-text-pad,
                                                      table.body .text-pad-right {
                                                        padding-left: 10px !important;
                                                      }
                                                      table.body .left-text-pad,
                                                      table.body .text-pad-left {
                                                        padding-right: 10px !important;
                                                      }
                                                      table.menu {
                                                        width: 100% !important;
                                                      }
                                                      table.menu td,
                                                      table.menu th {
                                                        width: auto !important;
                                                        display: inline-block !important;
                                                      }
                                                      table.menu.vertical td,
                                                      table.menu.vertical th,
                                                      table.menu.small-vertical td,
                                                      table.menu.small-vertical th {
                                                        display: block !important;
                                                      }
                                                      table.menu[align=""center""] {
                                                        width: auto !important;
                                                      }
                                                      table.button.expand {
                                                        width: 100% !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      .calendar-content {
                                                        padding: 0px !important;
                                                        width: 288px !important;
                                                      }
                                                      .not-available-day,
                                                      .calendar-today,
                                                      .available-day {
                                                        height: 40px !important;
                                                        width: 40px !important;
                                                      }
                                                      .day-label {
                                                        margin-left: 10% !important;
                                                        margin-top: 0% !important;
                                                        font-size: 15px;
                                                      }
	                                                  .p
	                                                  {
	                                                  font-size:16px
	                                                  font-size:4vw
	                                                  }
                                                    }
                                                  </style>";

                                string header = "<table class=\"body\" style=\"border-spacing:0;border-collapse:collapse;vertical-align:top;-webkit-hyphens:none;-moz-hyphens:none;hyphens:none;-ms-hyphens:none;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;margin:0;text-align:left;font-size:16px;line-height:19px;background:#f3f3f3;padding:0;width:100%;height:100%;color:#0a0a0a;margin-bottom:0px !important;background-color: white\">" +
                                                "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<td class=\"center\" align=\"center\" valign=\"top\" style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                                "<center style=\"width:100%;min-width:580px\">" +
                                                "<table class=\"container\" style=\"border-spacing:0;border-collapse:collapse;padding:0;vertical-align:top;background:#fefefe;width:580px;margin:0 auto;text-align:inherit;max-width:580px;\">" +
                                                "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<td style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                                "<div style=\"padding-top:10px\">" +
                                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<th style=\"width: 40%;>" +
                                                "<a href=\"https://www.hummingbirdindia.com\" target=\"_blank\" style=\"padding:0;margin:0;text-align:left;line-height:1.3;color:#2199e8;text-decoration:none\">" +
                                                "<img align=\"center\" alt=\"" + Imagealt1 + "\" class=\"center standard-header\" src=\"" + Imagelocation1 + "\" style=\"max-width: 120px\" >" +
                                                "</a></th><th style=\"font-size:16px;padding:20px 0 20px 0;line-height:28px\"> " +
                                                "<p>HB Confirmation #: " + ds.Tables[2].Rows[0][2].ToString() + "</p>" +
                                                "</th></tr></table></div><div>" +
                                                "<div class=\"headline-body\" style=\"padding-bottom:15px\">" +
                                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;width:564px;margin:0 auto;padding-left:16px;padding-right:16px;padding-bottom:0px !important\">" +
                                                "<p class=\"body  body-lg body-link-rausch light text-left   \" style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;line-height:1.4;font-weight:300;color:#484848;font-size:24px;hyphens:none;-ms-hyphens:none;-webkit-hyphens:none;-moz-hyphens:none;text-align:left;margin-bottom:0px !important;color:#0a0a0a;\">" + ds.Tables[2].Rows[0][1].ToString();

                                if (All.QReserveFlag == true)
                                {
                                    header += "<br /><span style=\"float: right;font-size:20px;\">Quick Reservation Confirmed</span>";
                                }
                                else
                                {
                                    header += "<br /><span style=\"float: right;font-size:20px;\">Reservation Confirmed</span>";
                                }

                                header += "</p></th></tr></table></div></div>";

                                string ChkInOutDate = "";
                                if (All.LTIAPIFlag == true)
                                {
                                    ChkInOutDate = "<div><div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                     "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                     "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                     "<p class=\"body-text-lg light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important'>‌" + All.CheckinDate + " ‌</p>" +
                                     "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>Check-in " + All.CheckinTime + "‌</p>" +
                                     "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                     "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                     "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                     "<p class=\"body-text-lg light text-right\" style='padding:0;margin:0;font-size:17px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important'>‌" + All.CheckoutDate + "</p>" +
                                     "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:16px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Check-out</p>" +
                                     "</th></tr></table></div><div>" +
                                     "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                     "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                     "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                     "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                     "</th></tr></table></div>";
                                }
                                else
                                {
                                    ChkInOutDate = "<div><div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                     "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                     "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                     "<p class=\"body-text-lg light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important'>‌" + ds.Tables[0].Rows[0][10].ToString() + " ‌</p>" +
                                     "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>Check-in " + ds.Tables[0].Rows[0][9].ToString() + "‌</p>" +
                                     "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                     "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                     "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                     "<p class=\"body-text-lg light text-right\" style='padding:0;margin:0;font-size:17px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important'>‌" + ds.Tables[0].Rows[0][11].ToString() + "</p>" +
                                     "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:16px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Check-out</p>" +
                                     "</th></tr></table></div><div>" +
                                     "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                     "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                     "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                     "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                     "</th></tr></table></div>";
                                }



                                string GuestTbl = "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"col-pad-left-2 col-pad-right-2\" style =\"padding:0;margin:0;padding-left:16px;padding-right:16px\">" +
                                                  "<div style=\"margin:0;-webkit-border-radius:8px;border-radius:8px;display:block;border-color:#d9242c;border-width:2px;border-style:dotted dashed;\">" +
                                                  "<p class=\"text-center\" style='font-weight:normal;padding:0;margin:0;text-align:center;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;font-size:24px;line-height:32px;margin-bottom:0px !important'>Guest Details</p>" +
                                                  "</div></th></tr></table></div>" +
                                                  "<div style=\"padding-top:8px;padding-bottom:8px;padding-left:16px;padding-right:16px;\">" +
                                                  "<table rules=\"rows\" style =\"border:#dbdbdb\">" +
                                                  "<tr><td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Guest Name </strong></td > " +
                                                  "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Room No / Occupancy </strong ></td > " +
                                                  "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Tariff / <br> Room / Day </strong ></td > " +
                                                  "</tr><tr></tr>";
                                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                {
                                    GuestTbl += "<tr style=\"font-style:normal;font-weight:normal;\" class=\"ng-scope\">" +
                                            "<td class=\"padd ng-binding\" style=\"vertical-align:middle;text-align:center\">" + ds.Tables[0].Rows[i][12].ToString() + ". " + ds.Tables[0].Rows[i][0].ToString() + " " + ds.Tables[0].Rows[i][13].ToString() + "</td>" +
                                            "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[0].Rows[i][6].ToString() + " / " + ds.Tables[0].Rows[i][7].ToString() + "</td>" +
                                            "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">INR " + ds.Tables[0].Rows[i][3].ToString() + "</td>" +
                                            "</tr>";
                                }
                                GuestTbl += "</table>";

                                string PayMode = "<div><div class=\"row-pad-bot-1\" style=\"padding-bottom:8px !important;padding-top:6px;\"></div>" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                                  "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:18px;margin-bottom:0px !important'>Tariff Payment: " + ds.Tables[0].Rows[0][4].ToString() + "</p>" +
                                                  "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                                  "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                                  "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                                  "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:18px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Service Payment: " + ds.Tables[0].Rows[0][5].ToString() + "</p>" +
                                                  "</th></tr></table></div></div><div>" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:3px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                  "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                  "</th></tr></table></div>";

                                string TariffDtls = "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                                    "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                    "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                                    "<p class=\"body-text-lg light color-rausch text-right\" style='padding:0;margin:0;word-break:normal;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;line-height:1.2;font-size:18px;text-align:center;color:#d9242c !important;margin-bottom:0px !important'>Guest Contacts</p>" +
                                                    "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>" +
                                                     MobileNo +
                                                    "</p></th></table></div><div>" +
                                                    "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                    "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                    "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                    "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                    "</th></tr></table></div>";


                                string PropertyDtls = "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table;\">" +
                                        "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                        "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                        "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:16px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-weight: bold;\">Property Name : " + ds.Tables[1].Rows[0][5].ToString() + " </p>" +
                                        "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:13px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;\">" + ds.Tables[1].Rows[0][0].ToString() + " </p>" +
                                        "</th></tr></table>" +
                                        "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                        "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                        "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                        "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                        "</th></tr></table></div>";

                                string Note = "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                            "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:14px;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;\"><strong>Note : </strong>" + SplNote + " </p>" +
                                            "</th></tr></table>" +
                                            "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                            "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                            "</th></tr></table></div>";

                                string ContactDtls = "<div style=\"padding-top:8px;padding-bottom:8px\" >" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' > Booked by</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][3].ToString() + " </ p >" +
                                            "</th>" +
                                            "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<hr>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<hr>" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' >Client Request #</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][12].ToString() + " </ p >" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<hr>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<hr>" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' >Issues / feedbacks</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + DeskNo + "<br>" + ds.Tables[10].Rows[0][1].ToString() + " </ p >" +
                                            "</th>" +
                                            "</tr></table></div>";



                                ContactDtls += "<div>" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                            "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                            "</th></tr></table></div><div style=\"padding-top:2px\">" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;text-align:left\"><th style=\"width: 60%;\">" +
                                            "<a href=\"" + link + "\" target=\"_blank\"><span style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:10px 0 10px 16px;margin:0;text-align:left;line-height:1.3;text-decoration:none;font-weight:300;color:#d9242c !important\">Security/Cancellation Policy</span></a>" +
                                            "</th><th style=\"font-size:10px;padding:10px 16px 10px 0;line-height:28px;text-align:right;\">" +
                                            "<p>Powered by Staysimplyfied.com</p>" +
                                            "</th></tr></table></div></tr></table></div>" +
                                            "</td></tr></table></center></td></tr></table>";

                                string EndData = "</body></html>";
                                MailContent = style + header + ChkInOutDate + GuestTbl + PayMode + TariffDtls + PropertyDtls + Note + ContactDtls + EndData;
                                message1.Body = MailContent;
                                message1.IsBodyHtml = true;


                            }
                        }
                    }
                    #endregion
                    #region
                    else
                    {

                        var ChCnt = 0;
                        var ChCntVal = "txt";

                        if (All.ResendFlag == true)
                        {
                            ChCnt = 1;
                            ChCntVal = "txt";
                        }
                        else
                        {
                            ChCnt = ds.Tables[3].Rows.Count;
                            ChCntVal = ds.Tables[3].Rows[0][4].ToString();
                        }

                        if (ChCntVal != "")
                        {
                            if (ds.Tables[10].Rows.Count > 0)
                            {
                                message1.From = new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][1].ToString(), "", System.Text.Encoding.UTF8);
                            }
                            else
                            {
                                message1.From = new System.Net.Mail.MailAddress("stay@hummingbirdindia.com", "", System.Text.Encoding.UTF8);
                            }
                            if (All.ResendFlag == true)
                            {
                                var Mail = All.PropertyGusetEmail.Split(',');
                                for (int i = 0; i < Mail.Length; i++)
                                {
                                    try
                                    {
                                        message1.To.Add(new System.Net.Mail.MailAddress(Mail[i].ToString()));
                                    }
                                    catch (Exception ex)
                                    {
                                        CreateLogFiles log = new CreateLogFiles();
                                        log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + Mail[i].ToString());
                                    }
                                }
                                if (All.UserEmail != "")
                                {
                                    try
                                    {
                                        message1.CC.Add(new System.Net.Mail.MailAddress(All.UserEmail));
                                    }
                                    catch (Exception ex)
                                    {
                                        CreateLogFiles log = new CreateLogFiles();
                                        log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + All.UserEmail);
                                    }
                                }


                                message1.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));

                            }
                            else
                            {

                                string PropertyMail = ds.Tables[3].Rows[0][4].ToString();
                                var PtyMail = PropertyMail.Split(',');
                                int cnt = PtyMail.Length;
                                for (int i = 0; i < cnt; i++)
                                {
                                    if (PtyMail[i].ToString() != "")
                                    {
                                        try
                                        {
                                            message1.To.Add(new System.Net.Mail.MailAddress(PtyMail[i].ToString()));
                                        }
                                        catch (Exception ex)
                                        {
                                            CreateLogFiles log = new CreateLogFiles();
                                            log.ErrorLog("=> Confirmation Email API => Room Level Confirmation Property Mail => To => BookingId => " + All.BookingId + " => Invalid Email => " + PtyMail[i].ToString());
                                        }
                                    }
                                }
                                ////for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                                ////{
                                ////    if (ds.Tables[3].Rows[i][2].ToString() != "")
                                ////    {
                                ////        try
                                ////        {
                                ////            message1.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[3].Rows[i][2].ToString()));
                                ////        }
                                ////        catch (Exception ex)
                                ////        {
                                ////            CreateLogFiles log = new CreateLogFiles();
                                ////            log.ErrorLog("=> Confirmation Email API => Room Level Confirmation Property Mail => Cc => BookingId => " + All.BookingId + " => Invalid Email => " + ds.Tables[3].Rows[i][2].ToString());
                                ////        }
                                ////    }
                                ////}
                                ////if (ds.Tables[2].Rows[0][4].ToString() != "")
                                ////{
                                ////    try
                                ////    {
                                ////        message1.Bcc.Add(new System.Net.Mail.MailAddress(ds.Tables[2].Rows[0][4].ToString()));
                                ////    }
                                ////    catch (Exception ex)
                                ////    {
                                ////        CreateLogFiles log = new CreateLogFiles();
                                ////        log.ErrorLog("=> Confirmation Email API => Room Level Confirmation Property Mail => Bcc => BookingId => " + All.BookingId + " => Invalid Email => " + ds.Tables[2].Rows[0][4].ToString());
                                ////    }
                                ////}

                                ////if (ds.Tables[10].Rows[0][1].ToString() != "stay@hummingbirdindia.com")
                                ////{
                                ////    message1.Bcc.Add(new System.Net.Mail.MailAddress("stay@hummingbirdindia.com"));
                                ////}
                                ////message1.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                            }
                            message1.Bcc.Add(new System.Net.Mail.MailAddress("prabakaran@warblerit.com"));

                            message1.Subject = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString();

                            string typeofpty1 = ds.Tables[4].Rows[0][8].ToString();
                            string Imagelocation1 = "";
                            string Imagealt1 = "";
                            if (typeofpty1 == "MGH")
                            {
                                Imagelocation1 = ds.Tables[6].Rows[0][4].ToString();
                                Imagealt1 = ds.Tables[6].Rows[0][5].ToString();
                                if (Imagelocation1 == "")
                                {
                                    Imagelocation1 = ds.Tables[4].Rows[0][10].ToString();
                                    Imagealt1 = ds.Tables[4].Rows[0][11].ToString();
                                }
                            }
                            else
                            {
                                Imagelocation1 = ds.Tables[4].Rows[0][10].ToString();
                                Imagealt1 = ds.Tables[4].Rows[0][11].ToString();
                            }

                            // Contact Email 
                            string DeskNo = "";
                            DeskNo = ds.Tables[2].Rows[0][14].ToString();

                            // Guest Contact No.
                            string MobileNo = ds.Tables[4].Rows[0][4].ToString();
                            if (MobileNo == "")
                            {
                                MobileNo = " - NA - ";
                            }

                            // Spl Note
                            string SplNote = ds.Tables[2].Rows[0][8].ToString();
                            if (SplNote == "")
                            {
                                SplNote = "- NA -";
                            }


                            // View in browser Link
                            string id = ds.Tables[2].Rows[0][12].ToString();
                            string link = "http://mybooking.hummingbirdindia.com/?redirect=BookingPropertyConfirmation&B=R&R=" + id;


                            string style = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
                                                <html xmlns=""http://www.w3.org/1999/xhtml"" style=""min-height:100%;background:#f3f3f3"">
                                                <head><meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8""></head>
                                                <body style=""font-size:16px;min-width:100%;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;-moz-box-sizing:border-box;-webkit-box-sizing:border-box;box-sizing:border-box;font-family:'Circular', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;margin:0;line-height:1.3;color:#0a0a0a;text-align:left;width:100% !important"">
                                                <div class=""preheader"" style=""mso-hide:all;visibility:hidden;opacity:0;color:transparent;font-size:0px;width:0;height:0;display:none !important""></div>
                                                <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
                                                <meta name=""viewport"" content=""width=device-width"">
                                                <link href=""https://fonts.googleapis.com/css?family=Cabin&display=swap"" rel=""stylesheet"">
                                                <style data-roadie-ignore data-immutable=""true"">
                                                    @media only screen and (max-width: 596px) {
                                                      .small-float-center {
                                                        margin: 0 auto !important;
                                                        float: none !important;
                                                        text-align: center !important;
                                                      }
                                                      .small-text-center {
                                                        text-align: center !important;
                                                      }
                                                      .small-text-left {
                                                        text-align: left !important;
                                                      }
                                                      .small-text-right {
                                                        text-align: right !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .hide-for-large {
                                                        display: block !important;
                                                        width: auto !important;
                                                        overflow: visible !important;
                                                        max-height: none !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .row.hide-for-large,
                                                      table.body table.container .row.hide-for-large {
                                                        display: table !important;
                                                        width: 100% !important;
                                                      }
                                                    }
    
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .show-for-large {
                                                        display: none !important;
                                                        width: 0;
                                                        mso-hide: all;
                                                        overflow: hidden;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body img {
                                                        width: auto !important;
                                                        height: auto !important;
                                                      }
                                                      table.body center {
                                                        min-width: 0 !important;
                                                      }
                                                      table.body .container {
                                                        width: 95% !important;
                                                      }
                                                      table.body .columns,
                                                      table.body .column {
                                                        height: auto !important;
                                                        -moz-box-sizing: border-box;
                                                        -webkit-box-sizing: border-box;
                                                        box-sizing: border-box;
                                                        padding-left: 16px !important;
                                                        padding-right: 16px !important;
                                                      }
                                                      table.body .columns .column,
                                                      table.body .columns .columns,
                                                      table.body .column .column,
                                                      table.body .column .columns {
                                                        padding-left: 0 !important;
                                                        padding-right: 0 !important;
                                                      }
                                                      table.body .collapse .columns,
                                                      table.body .collapse .column {
                                                        padding-left: 0 !important;
                                                        padding-right: 0 !important;
                                                      }
                                                      td.small-1,
                                                      th.small-1 {
                                                        display: inline-block !important;
                                                        width: 8.33333% !important;
                                                      }
                                                      td.small-2,
                                                      th.small-2 {
                                                        display: inline-block !important;
                                                        width: 16.66667% !important;
                                                      }
                                                      td.small-3,
                                                      th.small-3 {
                                                        display: inline-block !important;
                                                        width: 25% !important;
                                                      }
                                                      td.small-4,
                                                      th.small-4 {
                                                        display: inline-block !important;
                                                        width: 33.33333% !important;
                                                      }
                                                      td.small-5,
                                                      th.small-5 {
                                                        display: inline-block !important;
                                                        width: 41.66667% !important;
                                                      }
                                                      td.small-6,
                                                      th.small-6 {
                                                        display: inline-block !important;
                                                        width: 50% !important;
                                                      }
                                                      td.small-7,
                                                      th.small-7 {
                                                        display: inline-block !important;
                                                        width: 58.33333% !important;
                                                      }
                                                      td.small-8,
                                                      th.small-8 {
                                                        display: inline-block !important;
                                                        width: 66.66667% !important;
                                                      }
                                                      td.small-9,
                                                      th.small-9 {
                                                        display: inline-block !important;
                                                        width: 75% !important;
                                                      }
                                                      td.small-10,
                                                      th.small-10 {
                                                        display: inline-block !important;
                                                        width: 83.33333% !important;
                                                      }
                                                      td.small-11,
                                                      th.small-11 {
                                                        display: inline-block !important;
                                                        width: 91.66667% !important;
                                                      }
                                                      td.small-12,
                                                      th.small-12 {
                                                        display: inline-block !important;
                                                        width: 100% !important;
                                                      }
                                                      .columns td.small-12,
                                                      .column td.small-12,
                                                      .columns th.small-12,
                                                      .column th.small-12 {
                                                        display: block !important;
                                                        width: 100% !important;
                                                      }
                                                      .body .columns td.small-1,
                                                      .body .column td.small-1,
                                                      td.small-1 center,
                                                      .body .columns th.small-1,
                                                      .body .column th.small-1,
                                                      th.small-1 center {
                                                        display: inline-block !important;
                                                        width: 8.33333% !important;
                                                      }
                                                      .body .columns td.small-2,
                                                      .body .column td.small-2,
                                                      td.small-2 center,
                                                      .body .columns th.small-2,
                                                      .body .column th.small-2,
                                                      th.small-2 center {
                                                        display: inline-block !important;
                                                        width: 16.66667% !important;
                                                      }
                                                      .body .columns td.small-3,
                                                      .body .column td.small-3,
                                                      td.small-3 center,
                                                      .body .columns th.small-3,
                                                      .body .column th.small-3,
                                                      th.small-3 center {
                                                        display: inline-block !important;
                                                        width: 25% !important;
                                                      }
                                                      .body .columns td.small-4,
                                                      .body .column td.small-4,
                                                      td.small-4 center,
                                                      .body .columns th.small-4,
                                                      .body .column th.small-4,
                                                      th.small-4 center {
                                                        display: inline-block !important;
                                                        width: 33.33333% !important;
                                                      }
                                                      .body .columns td.small-5,
                                                      .body .column td.small-5,
                                                      td.small-5 center,
                                                      .body .columns th.small-5,
                                                      .body .column th.small-5,
                                                      th.small-5 center {
                                                        display: inline-block !important;
                                                        width: 41.66667% !important;
                                                      }
                                                      .body .columns td.small-6,
                                                      .body .column td.small-6,
                                                      td.small-6 center,
                                                      .body .columns th.small-6,
                                                      .body .column th.small-6,
                                                      th.small-6 center {
                                                        display: inline-block !important;
                                                        width: 50% !important;
                                                      }
                                                      .body .columns td.small-7,
                                                      .body .column td.small-7,
                                                      td.small-7 center,
                                                      .body .columns th.small-7,
                                                      .body .column th.small-7,
                                                      th.small-7 center {
                                                        display: inline-block !important;
                                                        width: 58.33333% !important;
                                                      }
                                                      .body .columns td.small-8,
                                                      .body .column td.small-8,
                                                      td.small-8 center,
                                                      .body .columns th.small-8,
                                                      .body .column th.small-8,
                                                      th.small-8 center {
                                                        display: inline-block !important;
                                                        width: 66.66667% !important;
                                                      }
                                                      .body .columns td.small-9,
                                                      .body .column td.small-9,
                                                      td.small-9 center,
                                                      .body .columns th.small-9,
                                                      .body .column th.small-9,
                                                      th.small-9 center {
                                                        display: inline-block !important;
                                                        width: 75% !important;
                                                      }
                                                      .body .columns td.small-10,
                                                      .body .column td.small-10,
                                                      td.small-10 center,
                                                      .body .columns th.small-10,
                                                      .body .column th.small-10,
                                                      th.small-10 center {
                                                        display: inline-block !important;
                                                        width: 83.33333% !important;
                                                      }
                                                      .body .columns td.small-11,
                                                      .body .column td.small-11,
                                                      td.small-11 center,
                                                      .body .columns th.small-11,
                                                      .body .column th.small-11,
                                                      th.small-11 center {
                                                        display: inline-block !important;
                                                        width: 91.66667% !important;
                                                      }
                                                      table.body td.small-offset-1,
                                                      table.body th.small-offset-1 {
                                                        margin-left: 8.33333% !important;
                                                        margin-left: 8.33333% !important;
                                                      }
                                                      table.body td.small-offset-2,
                                                      table.body th.small-offset-2 {
                                                        margin-left: 16.66667% !important;
                                                        margin-left: 16.66667% !important;
                                                      }
                                                      table.body td.small-offset-3,
                                                      table.body th.small-offset-3 {
                                                        margin-left: 25% !important;
                                                        margin-left: 25% !important;
                                                      }
                                                      table.body td.small-offset-4,
                                                      table.body th.small-offset-4 {
                                                        margin-left: 33.33333% !important;
                                                        margin-left: 33.33333% !important;
                                                      }
                                                      table.body td.small-offset-5,
                                                      table.body th.small-offset-5 {
                                                        margin-left: 41.66667% !important;
                                                        margin-left: 41.66667% !important;
                                                      }
                                                      table.body td.small-offset-6,
                                                      table.body th.small-offset-6 {
                                                        margin-left: 50% !important;
                                                        margin-left: 50% !important;
                                                      }
                                                      table.body td.small-offset-7,
                                                      table.body th.small-offset-7 {
                                                        margin-left: 58.33333% !important;
                                                        margin-left: 58.33333% !important;
                                                      }
                                                      table.body td.small-offset-8,
                                                      table.body th.small-offset-8 {
                                                        margin-left: 66.66667% !important;
                                                        margin-left: 66.66667% !important;
                                                      }
                                                      table.body td.small-offset-9,
                                                      table.body th.small-offset-9 {
                                                        margin-left: 75% !important;
                                                        margin-left: 75% !important;
                                                      }
                                                      table.body td.small-offset-10,
                                                      table.body th.small-offset-10 {
                                                        margin-left: 83.33333% !important;
                                                        margin-left: 83.33333% !important;
                                                      }
                                                      table.body td.small-offset-11,
                                                      table.body th.small-offset-11 {
                                                        margin-left: 91.66667% !important;
                                                        margin-left: 91.66667% !important;
                                                      }
                                                      table.body table.columns td.expander,
                                                      table.body table.columns th.expander {
                                                        display: none !important;
                                                      }
                                                      table.body .right-text-pad,
                                                      table.body .text-pad-right {
                                                        padding-left: 10px !important;
                                                      }
                                                      table.body .left-text-pad,
                                                      table.body .text-pad-left {
                                                        padding-right: 10px !important;
                                                      }
                                                      table.menu {
                                                        width: 100% !important;
                                                      }
                                                      table.menu td,
                                                      table.menu th {
                                                        width: auto !important;
                                                        display: inline-block !important;
                                                      }
                                                      table.menu.vertical td,
                                                      table.menu.vertical th,
                                                      table.menu.small-vertical td,
                                                      table.menu.small-vertical th {
                                                        display: block !important;
                                                      }
                                                      table.menu[align=""center""] {
                                                        width: auto !important;
                                                      }
                                                      table.button.expand {
                                                        width: 100% !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      .calendar-content {
                                                        padding: 0px !important;
                                                        width: 288px !important;
                                                      }
                                                      .not-available-day,
                                                      .calendar-today,
                                                      .available-day {
                                                        height: 40px !important;
                                                        width: 40px !important;
                                                      }
                                                      .day-label {
                                                        margin-left: 10% !important;
                                                        margin-top: 0% !important;
                                                        font-size: 15px;
                                                      }
	                                                  .p
	                                                  {
	                                                  font-size:16px
	                                                  font-size:4vw
	                                                  }
                                                    }
                                                  </style>";

                            string header = "<table class=\"body\" style=\"border-spacing:0;border-collapse:collapse;vertical-align:top;-webkit-hyphens:none;-moz-hyphens:none;hyphens:none;-ms-hyphens:none;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;margin:0;text-align:left;font-size:16px;line-height:19px;background:#f3f3f3;padding:0;width:100%;height:100%;color:#0a0a0a;margin-bottom:0px !important;background-color: white\">" +
                                            "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<td class=\"center\" align=\"center\" valign=\"top\" style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                            "<center style=\"width:100%;min-width:580px\">" +
                                            "<table class=\"container\" style=\"border-spacing:0;border-collapse:collapse;padding:0;vertical-align:top;background:#fefefe;width:580px;margin:0 auto;text-align:inherit;max-width:580px;\">" +
                                            "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<td style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                            "<div style=\"padding-top:10px\">" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th style=\"width: 40%;>" +
                                            "<a href=\"https://www.hummingbirdindia.com\" target=\"_blank\" style=\"padding:0;margin:0;text-align:left;line-height:1.3;color:#2199e8;text-decoration:none\">" +
                                            "<img align=\"center\" alt=\"" + Imagealt1 + "\" class=\"center standard-header\" src=\"" + Imagelocation1 + "\" style=\"max-width: 120px\" >" +
                                            "</a></th><th style=\"font-size:16px;padding:20px 0 20px 0;line-height:28px\"> " +
                                            "<p>HB Confirmation #: " + ds.Tables[2].Rows[0][2].ToString() + "</p>";

                            if (ds.Tables[4].Rows[0][19].ToString() != "")
                            {
                                header += "<p>Confirmed by: " + ds.Tables[4].Rows[0][19].ToString() + "</p>";
                            }



                            header += "</th></tr></table></div><div>" +
                                "<div class=\"headline-body\" style=\"padding-bottom:15px\">" +
                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;width:564px;margin:0 auto;padding-left:16px;padding-right:16px;padding-bottom:0px !important\">" +
                                "<p class=\"body  body-lg body-link-rausch light text-left   \" style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;line-height:1.4;font-weight:300;color:#484848;font-size:24px;hyphens:none;-ms-hyphens:none;-webkit-hyphens:none;-moz-hyphens:none;text-align:left;margin-bottom:0px !important;color:#0a0a0a;\">" + ds.Tables[2].Rows[0][1].ToString();

                            if (All.QReserveFlag == true)
                            {
                                header += "<br /><span style=\"float: right;font-size:20px;\">Quick Reservation Confirmed</span>";
                            }
                            else
                            {
                                header += "<br /><span style=\"float: right;font-size:20px;\">Reservation Confirmed</span>";
                            }



                            header += "</p>" +
                               "</th></tr></table></div></div>";

                            string ChkInOutDate = "";
                            if (All.LTIAPIFlag == true)
                            {

                                ChkInOutDate = "<div><div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                                  "<p class=\"body-text-lg light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important'>‌" + All.CheckinDate + " ‌</p>" +
                                                  "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>Check-in " + All.CheckinTime + "‌</p>" +
                                                  "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                                  "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                                  "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                                  "<p class=\"body-text-lg light text-right\" style='padding:0;margin:0;font-size:17px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important'>‌" + All.CheckoutDate + "</p>" +
                                                  "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:16px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Check-out</p>" +
                                                  "</th></tr></table></div><div>" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                  "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                  "</th></tr></table></div>";
                            }
                            else
                            {
                                ChkInOutDate = "<div><div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                                  "<p class=\"body-text-lg light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important'>‌" + ds.Tables[0].Rows[0][9].ToString() + " ‌</p>" +
                                                  "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>Check-in " + ds.Tables[0].Rows[0][11].ToString() + "‌</p>" +
                                                  "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                                  "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                                  "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                                  "<p class=\"body-text-lg light text-right\" style='padding:0;margin:0;font-size:17px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important'>‌" + ds.Tables[0].Rows[0][10].ToString() + "</p>" +
                                                  "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:16px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Check-out</p>" +
                                                  "</th></tr></table></div><div>" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                  "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                  "</th></tr></table></div>";
                            }






                            string GuestTbl = "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                              "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                              "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                              "<th class=\"col-pad-left-2 col-pad-right-2\" style =\"padding:0;margin:0;padding-left:16px;padding-right:16px\">" +
                                              "<div style=\"margin:0;-webkit-border-radius:8px;border-radius:8px;display:block;border-color:#d9242c;border-width:2px;border-style:dotted dashed;\">" +
                                              "<p class=\"text-center\" style='font-weight:normal;padding:0;margin:0;text-align:center;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;font-size:24px;line-height:32px;margin-bottom:0px !important'>Guest Details</p>" +
                                              "</div></th></tr></table></div>" +
                                              "<div style=\"padding-top:8px;padding-bottom:8px;padding-left:16px;padding-right:16px;\">" +
                                              "<table rules=\"rows\" style =\"border:#dbdbdb\">" +
                                              "<tr><td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Guest Name </strong></td > " +
                                              "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Room No / Occupancy </strong ></td > " +
                                              "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Tariff / <br> Room / Day </strong ></td > " +
                                              "</tr><tr></tr>";
                            for (int i = 0; i < ds.Tables[11].Rows.Count; i++)
                            {

                                if ((typeofpty1 == "MGH") || (typeofpty1 == "DdP"))
                                {
                                    GuestTbl += "<tr style=\"font-style:normal;font-weight:normal;\" class=\"ng-scope\">" +
                                                "<td class=\"padd ng-binding\" style=\"vertical-align:middle;text-align:center\">" + ds.Tables[11].Rows[i][11].ToString() + "</td>" +
                                                "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[11].Rows[i][10].ToString() + " / " + ds.Tables[11].Rows[i][4].ToString() + "</td>" +
                                                "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">INR " + ds.Tables[11].Rows[i][3].ToString() + "</td>" +
                                                "</tr>";
                                }
                                else
                                {
                                    GuestTbl += "<tr style=\"font-style:normal;font-weight:normal;\" class=\"ng-scope\">" +
                                     "<td class=\"padd ng-binding\" style=\"vertical-align:middle;text-align:center\">" + ds.Tables[11].Rows[i][11].ToString() + "</td>" +
                                     "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[4].Rows[0][12].ToString() + " / " + ds.Tables[11].Rows[i][4].ToString() + "</td>" +
                                     "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">INR " + ds.Tables[11].Rows[i][3].ToString() + "</td>" +
                                     "</tr>";
                                }



                            }
                            GuestTbl += "</table>";

                            string PayMode = "<div><div class=\"row-pad-bot-1\" style=\"padding-bottom:8px !important;padding-top:6px;\"></div>" +
                                              "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                              "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                              "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                              "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:18px;margin-bottom:0px !important'>Tariff Payment: " + ds.Tables[11].Rows[0][5].ToString() + "</p>" +
                                              "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                              "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                              "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                              "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:18px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Service Payment: " + ds.Tables[11].Rows[0][6].ToString() + "</p>" +
                                              "</th></tr></table></div></div><div>" +
                                              "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                              "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                              "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:3px;width:564px;padding-left:16px;padding-right:16px\">" +
                                              "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                              "</th></tr></table></div>";

                            string TariffDtls = "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">";
                            string Stng = ds.Tables[11].Rows[0][8].ToString();
                            if (ds.Tables[11].Rows[0][7].ToString() == "NOTBTC")
                            {
                                if (Stng != "")
                                {
                                    TariffDtls +=
                                                "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px\">" +
                                                "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0;text-align:center;font-size:18px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#d9242c !important;;word-break:normal;line-height:1.2;\">Agreed Tariff</p>" +
                                    "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>" + Stng + "</p>" +

                                                                                                        "</th>";
                                }


                            }
                            else
                            {
                                try
                                {
                                    string file = ds.Tables[4].Rows[0][1].ToString();
                                    System.Net.Mail.Attachment att = new System.Net.Mail.Attachment(file);
                                    att.Name = ds.Tables[4].Rows[0][2].ToString();
                                    message1.Attachments.Add(att);
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog(" => Room Level Property Mail => BookingId => " + All.BookingId + " => PDF Attachment => Err Msg => " + ex.Message);
                                }
                                TariffDtls += "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px\">" +
                                              "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0;text-align:center;font-size:18px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#d9242c !important;;word-break:normal;line-height:1.2;\">Agreed Tariff</p>" +
                                              "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>" + Stng + "</p>" +
                                              "</th>";
                            }
                            TariffDtls += "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                                "<p class=\"body-text-lg light color-rausch text-right\" style='padding:0;margin:0;word-break:normal;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;line-height:1.2;font-size:18px;text-align:center;color:#d9242c !important;margin-bottom:0px !important'>Guest Contacts</p>" +
                                                "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>" +
                                                 MobileNo +
                                                "</p></th></table></div><div>" +
                                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                "</th></tr></table></div>";


                            string PropertyDtls = "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table;\">" +
                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                  "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                  "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:16px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-weight: bold;\">Property Name : " + ds.Tables[1].Rows[0][5].ToString() + " </p>" +
                                   "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:13px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;\">" + ds.Tables[1].Rows[0][0].ToString() + " </p>" +
                                  "</th></tr></table>" +
                                  "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                  "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                  "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                  "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                  "</th></tr></table></div>";

                            string Note = "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                         "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                         "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                                         "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:14px;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;\"><strong>Note : </strong>" + SplNote + " </p>" +
                                                         "</th></tr></table>" +
                                                         "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                         "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                         "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                         "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                         "</th></tr></table></div>";



                            string ContactDtls = "<div style=\"padding-top:8px;padding-bottom:8px\" >" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' > Booked by</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][3].ToString() + " </ p >" +
                                            "</th>" +
                                            "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<hr>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<hr>" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' >Client Request #</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][13].ToString() + " </ p >" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<hr>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<hr>" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' >Issues / feedbacks</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + DeskNo + "<br>" + ds.Tables[10].Rows[0][0].ToString() + " </ p >" +
                                            "</th>" +
                                            "</tr></table></div>";

                            ContactDtls += "<div>" +
                                                 "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                 "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                 "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                 "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                 "</th></tr></table></div><div style=\"padding-top:2px\">" +
                                                 "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;padding:0;width:100%;position:relative;display:table\">" +
                                                 "<tr class=\"\" style=\"padding:0;text-align:left\"><th style=\"width: 60%;\">" +
                                                 "<a href=\"" + link + "\" target=\"_blank\"><span style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:10px 0 10px 16px;margin:0;text-align:left;line-height:1.3;text-decoration:none;font-weight:300;color:#d9242c !important\">Security/Cancellation Policy</span></a>" +
                                                 "</th><th style=\"font-size:10px;padding:10px 16px 10px 0;line-height:28px;text-align:right;\">" +
                                                 "<p>Powered by Staysimplyfied.com</p>" +
                                                 "</th></tr></table></div></tr></table></div>" +
                                                 "</td></tr></table></center></td></tr></table>";

                            string EndData = "</body></html>";
                            MailContent = style + header + ChkInOutDate + GuestTbl + PayMode + TariffDtls + PropertyDtls + Note + ContactDtls + EndData;
                            message1.Body = MailContent;
                            message1.IsBodyHtml = true;
                        }

                    }
                    #endregion

                    try
                    {
                        smtp1.Send(message1);
                        Response2 = "Success";
                    }
                    catch (Exception ex)
                    {
                        CreateLogFiles log = new CreateLogFiles();
                        log.ErrorLog("=> Property Confirmation Mail => smtp => BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);
                        Response2 = "Failure";
                    }


                }
                else
                {
                    Response2 = "Success";
                }
                #endregion

                #region
                if (All.SmsChk == true)
                {
                    string PaymentMode = "";
                    string Maplink = "";
                    string WhatsappFileName = "Booking Confirmation -" + ds.Tables[2].Rows[0][2].ToString();
                    string paths123 = @"D:\home\site\wwwroot\Confirmations\";
                    var FileName = paths123 + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                    string WhatsappPdfUrl = AzureBlobPdfURl;
                    if (ds.Tables[0].Rows[0][8].ToString() == "Bed")
                    {
                        PaymentMode = ds.Tables[0].Rows[0][4].ToString();
                        Maplink = ds.Tables[1].Rows[0][13].ToString();

                    }
                    else
                    {
                        PaymentMode = ds.Tables[0].Rows[0][5].ToString();
                        Maplink = ds.Tables[1].Rows[0][13].ToString();
                    }

                    string FinalAPIUrl = "";
                    FinalAPIUrl = System.Configuration.ConfigurationManager.AppSettings["UrlShortner"] + "/API/UrlShortner/urlshort";
                    List<ConfirmationEMail> Msg = new List<ConfirmationEMail>();
                    try
                    {
                        SqlCommand command3 = new SqlCommand();
                        DataSet ds3 = new DataSet();
                        command3.CommandText = "SP_MMTBooking_Help";
                        command3.CommandType = CommandType.StoredProcedure;
                        command3.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "BookingConfirmedSMS";
                        command3.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = All.BookingId;
                        command3.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = "";
                        command3.Parameters.Add("@Str2", SqlDbType.NVarChar).Value = "";
                        command3.Parameters.Add("@Id1", SqlDbType.BigInt).Value = 0;
                        command3.Parameters.Add("@Id2", SqlDbType.BigInt).Value = 0;
                        ds3 = new DBconnection().ExecuteDataSet(command3, "");
                        var myData = ds3.Tables[0].AsEnumerable().Select(r => new ConfirmationEMail
                        {
                            CityCode = r.Field<string>("CityCode"),
                            Bookingcode = r.Field<string>("PropertyId"),
                            RowId = r.Field<string>("RatePlanCode"),
                            Caretaker = r.Field<long>("Caretaker"),
                            MobileNo = r.Field<string>("MobileNo"),
                            WhatsAppMsg = r.Field<string>("WhatsAppMsg")

                        });
                        Msg = myData.ToList();
                    }
                    catch (Exception ex)
                    {
                        CreateLogFiles log = new CreateLogFiles();
                        log.ErrorLog(" => Confirmation Email API => Booking Confirmation SMS => Get data from Procedure => BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);
                    }
                    string MapLink = "";
                    String FinalMap = "";
                    if (Maplink != "" && Maplink != null && Maplink != " ")
                    {
                        MapLink = "https://www.google.co.in/maps/place/" + Maplink;
                    }

                    try
                    {
                        if (Msg.Count > 0)
                        {
                            for (int i = 0; i < Msg.Count; i++)
                            {

                                //Firebase URL Start for Cancel
                                WebClient client = new WebClient();
                                client.Headers.Add("Content-Type", "application/json");
                                string LongPath = "http://mybooking.hummingbirdindia.com/" + "?B=" + Msg[i].Bookingcode + "$R=" + Msg[i].RowId;
                                string body = "{\"longUrl\":\"" + LongPath + "\"}";
                                try
                                {
                                    string ShortURl = client.UploadString(FinalAPIUrl, "POST", body);
                                    Msg[i].ShortPath = ShortURl;
                                    Msg[i].ShortPath = Msg[i].ShortPath.Replace("\"", "");
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog(" => Confirmation Email API => Booking Confirmation SMS => Cancel Link - BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);

                                }
                                //Firebase URL End For Cancel
                                //Firebase URL Start for Map
                                WebClient client1 = new WebClient();
                                client.Headers.Add("Content-Type", "application/json");
                                string LongPath1 = MapLink;
                                string body1 = "{\"longUrl\":\"" + LongPath1 + "\"}";
                                try
                                {
                                    string ShortURl1 = client.UploadString(FinalAPIUrl, "POST", body1);
                                    FinalMap = ShortURl1;
                                    FinalMap = FinalMap.Replace("\"", "");
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog(" => Confirmation Email API => Booking Confirmation SMS => Map Link - BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);

                                }
                                //Short URL End0

                                if (Msg[i].Caretaker == 0)
                                {
                                    if (PaymentMode == "Bill to Company (BTC)")
                                    {
                                        if (FinalMap != "")
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ".Map:" + " " + FinalMap + " .");
                                        }
                                    }
                                    else
                                    {
                                        if (FinalMap != "")
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ".Map:" + " " + FinalMap + ". To view you Booking / cancel your Booking,click the below link" + " " + Msg[i].ShortPath + " .");
                                        }
                                        else
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ". To view you Booking / cancel your Booking,click the below link" + " " + Msg[i].ShortPath + " .");
                                        }
                                    }
                                }
                                else
                                {
                                    if (PaymentMode == "Bill to Company (BTC)")
                                    {
                                        if (FinalMap != "")
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ".Map:" + " " + FinalMap + " .");
                                        }
                                    }
                                    else
                                    {
                                        if (FinalMap != "")
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ".Map:" + " " + FinalMap + " ."); //+ Msg[i].ShortPath removed by Pooranam
                                        }
                                    }
                                }
                                ////if (Msg[i].MobileNo != "" && All.GuestMailChk == true)
                                ////{
                                ////    try
                                ////    {
                                ////        WhatsappObj WhatsappData = new WhatsappObj();
                                ////        WhatsappData.MobileNo = Msg[i].MobileNo;
                                ////        WhatsappData.Msg = Msg[i].WhatsAppMsg;
                                ////        WhatsappData.WhatsappFileName = WhatsappFileName;
                                ////        WhatsappData.WhatsappPdfUrl = WhatsappPdfUrl;
                                ////        Task.Factory.StartNew(() => WhatsappAPI(WhatsappData));
                                ////    }
                                ////    catch (Exception Ex)
                                ////    {
                                ////        CreateLogFiles log = new CreateLogFiles();
                                ////        log.ErrorLog(" => Confirmation WhatsAPP API => Booking Confirmation WhatsApp => BookingId => " + All.BookingId + " => Err Msg => " + Ex.Message);

                                ////    }


                                ////}
                                WebRequest request = HttpWebRequest.Create(Msg[i].CityCode);
                                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                Stream s = (Stream)response.GetResponseStream();
                                StreamReader readStream = new StreamReader(s);
                                string dataString = readStream.ReadToEnd();
                                CreateLogFiles lognew = new CreateLogFiles();
                                lognew.ErrorLog("BookingId => " + All.BookingId + " => Status => " + dataString + " => Link " + Msg[i].CityCode);
                                response.Close();
                                s.Close();
                                readStream.Close();
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        CreateLogFiles log = new CreateLogFiles();
                        log.ErrorLog(" => Confirmation Email API => Booking Confirmation SMS => BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);

                    }
                }
                #endregion

                if (Response1 == "Success" && Response2 == "Failure")
                {
                    Response = "Confirmation Email not Sent to Property.";
                }
                else if (Response1 == "Failure" && Response2 == "Success")
                {
                    Response = "Confirmation Email not Sent to Guest.";
                }
                else if (Response1 == "Failure" && Response2 == "Failure")
                {
                    Response = "Confirmation Email not Sent to Guest & Property.";
                }
                else
                {
                    Response = "Confirmation Email Sent Successfully";
                }


                if (All.ResendFlag != true && ds.Tables[4].Rows[0][8].ToString() == "ExP" && ds.Tables[0].Rows[0][5].ToString() == "Bill to Company (BTC)")
                {
                    ZohoObj ZohoObjData = new ZohoObj();
                    ZohoObjData.BookingId = All.BookingId; 
                    ZohoObjData.PropertyName = ds.Tables[1].Rows[0][5].ToString();
                    ZohoObjData.BookingCode = ds.Tables[2].Rows[0][2].ToString();
                    Task.Factory.StartNew(() => ZohoPOAPI(ZohoObjData));
                }
                
                return Json(new { Code = "200", EmailResponse = Response });
            }
            catch (Exception Ex)
            {
                log = new CreateLogFiles();
                log.ErrorLog(" => Confirmation Email API => BookingId => " + All.BookingId + "=>" + Ex.Message);
                return Json(new { Code = "400", EmailResponse = "Confirmation Email not Sent - " + Ex.Message });
            }
        }

        //public string ZohoPOAPI(ZohoObj All)
        //{
        //    string body = "";
        //    try
        //    {

        //        SqlCommand command1 = new SqlCommand();
        //        DataSet ds1 = new DataSet();
        //        command1.CommandText = "SP_Zoho_PO_Help";
        //        command1.CommandType = CommandType.StoredProcedure;
        //        command1.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "GetData";
        //        command1.Parameters.Add("@Id", SqlDbType.BigInt).Value = All.BookingId;
        //        command1.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = "";
        //        ds1 = new DBconnection().ExecuteDataSet(command1, "");
        //        string ClientId = ds1.Tables[0].Rows[0][0].ToString();
        //        string ClientSecret = ds1.Tables[0].Rows[0][1].ToString();
        //        string AccessToken = ds1.Tables[0].Rows[0][2].ToString();
        //        string OrganizationId = ds1.Tables[0].Rows[0][3].ToString();

        //        string URL = "https://books.zoho.in/api/v3/purchaseorders?organization_id=" + OrganizationId;

        //        ServicePointManager.Expect100Continue = true;
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
        //               | SecurityProtocolType.Tls11
        //               | SecurityProtocolType.Tls12
        //               | SecurityProtocolType.Ssl3;
        //        WebRequest webReq = WebRequest.Create(URL);
        //        webReq.Proxy = null;
        //        HttpWebRequest httpReq = (HttpWebRequest)webReq;
        //        httpReq.ContentType = "application/json";
        //        httpReq.Method = "POST";
        //        httpReq.Headers.Add("Authorization", "Zoho-oauthtoken " + AccessToken);
        //        httpReq.ProtocolVersion = HttpVersion.Version11;
        //        httpReq.Credentials = CredentialCache.DefaultCredentials;
        //        Stream reqStream = httpReq.GetRequestStream();
        //        StreamWriter streamWrite = new StreamWriter(reqStream);

        //        var myData = ds1.Tables[1].AsEnumerable().Select(r => new PoData
        //        {

        //            item_id = r.Field<string>("ItemId"),
        //            rate = r.Field<decimal>("Tariff"),
        //            quantity = r.Field<int>("NoOfDays"),
        //            tax_id = r.Field<string>("TaxId"),
        //            vendor_id = r.Field<string>("VendorId"),
        //            purchaseorder_number = r.Field<string>("PONo"),
        //            reference_number = r.Field<string>("BookingCode"),
        //            date = r.Field<string>("BookedDt"),
        //            delivery_date = r.Field<string>("CheckInDate"),
        //            payment_terms = r.Field<int>("PaymentTerms"),
        //            payment_terms_label = r.Field<string>("PaymentTermsLabel"),
        //            is_inclusive_tax = r.Field<bool>("InclusiveTax"),
        //            notes = r.Field<string>("Notes"),
        //            terms = r.Field<string>("Terms"),
        //            Zoho_Branch_Id = r.Field<string>("Zoho_Branch_Id"), 
        //        }).ToList();

        //        if (myData[0].vendor_id != "0")
        //        {
        //            var line_items = new List<LineItemDt>();
        //            int Tbl1RCount = myData.Count;
        //            for (var j = 0; j < Tbl1RCount; j++)
        //            {
        //                line_items.Add(new LineItemDt
        //                {
        //                    item_id = myData[j].item_id,
        //                    rate = myData[j].rate,
        //                    quantity = myData[j].quantity,
        //                    tax_id = myData[j].tax_id,
        //                });
        //            }

        //            body = new JavaScriptSerializer().Serialize(new
        //            {
        //                vendor_id = myData[0].vendor_id,
        //                purchaseorder_number = myData[0].purchaseorder_number,
        //                reference_number = myData[0].reference_number,
        //                date = myData[0].date,
        //                delivery_date = myData[0].delivery_date,
        //                payment_terms = myData[0].payment_terms,
        //                payment_terms_label = myData[0].payment_terms_label,
        //                is_inclusive_tax = myData[0].is_inclusive_tax,
        //                notes = myData[0].notes,
        //                terms = myData[0].terms,
        //                branch_id = myData[0].Zoho_Branch_Id,
        //                line_items

        //            });

        //            streamWrite.Write(body);
        //            streamWrite.Close();
        //            HttpWebResponse wrres = (HttpWebResponse)httpReq.GetResponse();
        //            StreamReader strmReader = new StreamReader(wrres.GetResponseStream(), Encoding.Default, true);
        //            string Resobj1 = strmReader.ReadToEnd();
        //            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
        //            PORootRes POResponse = jsonSerializer.Deserialize<PORootRes>(Resobj1);

        //            if (POResponse.code == 0)
        //            {

        //                SqlCommand command2 = new SqlCommand();
        //                DataSet ds2 = new DataSet();
        //                command2.CommandText = "SP_Zoho_PO_Help";
        //                command2.CommandType = CommandType.StoredProcedure;
        //                command2.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "POUpdate";
        //                command2.Parameters.Add("@Id", SqlDbType.BigInt).Value = All.BookingId;
        //                command2.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = POResponse.purchaseorder.purchaseorder_id;
        //                ds2 = new DBconnection().ExecuteDataSet(command2, "");
        //            }
        //            else
        //            {
        //                CreateLogFiles log = new CreateLogFiles();
        //                log.ErrorLog(" => Confirmation Email API => PO Insert => Err msg => Booking Id -" + All.BookingId + " => " + POResponse.message);
        //            }
        //        }
        //        else
        //        {
        //            SqlCommand command5 = new SqlCommand();
        //            DataSet ds5 = new DataSet();
        //            command5.CommandText = "SP_SMTPMailSetting_Help";
        //            command5.CommandType = CommandType.StoredProcedure;
        //            command5.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "SMTP";
        //            command5.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = "";
        //            command5.Parameters.Add("@Id", SqlDbType.BigInt).Value = 0;
        //            ds5 = new DBconnection().ExecuteDataSet(command5, "");
        //            string Host = ds5.Tables[0].Rows[0][0].ToString();
        //            string CredentialsUserName = ds5.Tables[0].Rows[0][1].ToString();
        //            string CredentialsPassword = ds5.Tables[0].Rows[0][2].ToString();
        //            int Port = Convert.ToInt16(ds5.Tables[0].Rows[0][3]);

        //            System.Net.Mail.MailMessage message2 = new System.Net.Mail.MailMessage();
        //            message2.From = new System.Net.Mail.MailAddress("noreply@hummingbirdindia.com", "noreply", System.Text.Encoding.UTF8);
        //            message2.To.Add(new System.Net.Mail.MailAddress("prabakaran@warblerit.com"));
        //            //message2.To.Add(new System.Net.Mail.MailAddress("nandhu@warblerit.com"));
        //            message2.To.Add(new System.Net.Mail.MailAddress("vivek@warblerit.com"));
        //            message2.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
        //            message2.Subject = "PO Creation Status - " + All.BookingCode;
        //            string Imagebody = "<div style=\"text-align: center;  padding: 10px 0 10px 0;\">" +
        //                   "<img src=\"https://portalvhds4prl9ymlwxnt8.blob.core.windows.net/img/HB_logo_small.png\" alt=\"logo\">" +
        //                   "</div>";
        //            string header = "<div  style=\"width: 700px;margin: 10px auto;\">" +
        //                     "<p style=\"margin: 0; padding: 10px 0 10px 0; color: #556DE5; letter-spacing: 125%; word-spacing: 150%;font-weight: bold; font-family: 'Open Sans', sans-serif\">Hello ,</p>" +
        //                     "<p style=\"margin: 0; padding: 10px 0 10px 0;  letter-spacing: 125%; word-spacing: 150%;font-weight: bold; font-family: 'Open Sans', sans-serif\">Purchase order not created because of vendor id is not exists. Please find the details below for your reference.</p>" +
        //                     "<table style=\"background-color: #ffffff;border-radius: 4px 4px 0 0;overflow: hidden; width: 100%;margin-bottom: 1rem;background-color: transparent;border-collapse: collapse;font-family: ''Open Sans', sans-serif';\">" +
        //                     "<thead style=\"text-align: left;\">" +
        //                     "<tr>" +
        //                     "<th style=\"padding-left: 5px;background-color: #ebebeb;padding: 20px;border-bottom: transparent;color: #000000;font-weight: bold;border-color: #dee2e6;  vertical-align: bottom;border: 1px solid #dee2e6;padding: .75rem;vertical-align: top;width: 30%\">Booking Code</th>" +
        //                     "<th style=\"padding-left: 5px;background-color: #ebebeb;padding: 20px;border-bottom: transparent;color: #000000;font-weight: bold;border-color: #dee2e6;  vertical-align: bottom;border: 1px solid #dee2e6;padding: .75rem;vertical-align: top;width: 50%\">Property Name</th>" +
        //                     "</tr>" +
        //                     "</thead>" +
        //                     "<tbody>" +
        //                     "<tr>" +
        //                     "<td style=\"padding-left: 5px;padding: .75rem;vertical-align: top;border: 1px solid #dee2e6;width: 30%\">" + All.BookingCode + "</td>" +
        //                     "<td style=\"padding-left: 5px;padding: .75rem;vertical-align: top;border: 1px solid #dee2e6;width: 50%\">" + All.PropertyName + "</td>" +
        //                     "<tr/>" +
        //                     "</tbody>" +
        //                     "</table>";

        //            string footer = "<br /><br />" +
        //                "<p style=\"margin: 0; padding: 10px 0 10px 0; color: #556DE5; letter-spacing: 125%; word-spacing: 150%;font-weight: bold; font-family: 'Open Sans', sans-serif\"> Warm Regards,</p>" +
        //                "<p style=\"margin: 0; padding: 0px 0 10px 0; color: #556DE5; letter-spacing: 125%; word-spacing: 150%;font-weight: bold; font-family: 'Open Sans', sans-serif\"> Technical Team</p><br /></div>";


        //            message2.Body = Imagebody + header + footer;
        //            message2.IsBodyHtml = true;
        //            System.Net.Mail.SmtpClient smtp2 = new System.Net.Mail.SmtpClient();
        //            smtp2.EnableSsl = true;
        //            smtp2.Port = Port;
        //            smtp2.Host = Host;
        //            smtp2.Credentials = new System.Net.NetworkCredential(CredentialsUserName, CredentialsPassword);
        //            try
        //            {
        //                smtp2.Send(message2);
        //            }
        //            catch (Exception ex)
        //            {
        //                CreateLogFiles log = new CreateLogFiles();
        //                log.ErrorLog(" => Confirmation Email API => Error => Vendor id not exists =>  Mail Block =>  msg => " + ex.Message);

        //            }

        //        }

        //        return All.BookingId.ToString();
        //    }
        //    catch (Exception Ex)
        //    {
        //        log = new CreateLogFiles();
        //        log.ErrorLog(" => Confirmation Email API => BookingId => " + All.BookingId + "=> Zoho PO Insert" + Ex.Message + "=>" + body);
        //        return All.BookingId.ToString();
        //    }

        //}

        public string ZohoPOAPI(ZohoObj All)
        {
            string body = "";
            try
            {

                SqlCommand command1 = new SqlCommand();
                DataSet ds1 = new DataSet();
                command1.CommandText = "SP_Zoho_PO_Help";
                command1.CommandType = CommandType.StoredProcedure;
                command1.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "GetData";
                command1.Parameters.Add("@Id", SqlDbType.BigInt).Value = All.BookingId;
                command1.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = "";
                ds1 = new DBconnection().ExecuteDataSet(command1, "");
                string ClientId = ds1.Tables[0].Rows[0][0].ToString();
                string ClientSecret = ds1.Tables[0].Rows[0][1].ToString();
                string AccessToken = ds1.Tables[0].Rows[0][2].ToString();
                string OrganizationId = ds1.Tables[0].Rows[0][3].ToString();

                string URL = "https://books.zoho.in/api/v3/purchaseorders?organization_id=" + OrganizationId;

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;
                WebRequest webReq = WebRequest.Create(URL);
                webReq.Proxy = null;
                HttpWebRequest httpReq = (HttpWebRequest)webReq;
                httpReq.ContentType = "application/json";
                httpReq.Method = "POST";
                httpReq.Headers.Add("Authorization", "Zoho-oauthtoken " + AccessToken);
                httpReq.ProtocolVersion = HttpVersion.Version11;
                httpReq.Credentials = CredentialCache.DefaultCredentials;
                Stream reqStream = httpReq.GetRequestStream();
                StreamWriter streamWrite = new StreamWriter(reqStream);

                var myData = ds1.Tables[1].AsEnumerable().Select(r => new PoData
                {

                    item_id = r.Field<string>("ItemId"),
                    rate = r.Field<decimal>("Tariff"),
                    quantity = r.Field<int>("NoOfDays"),
                    tax_id = r.Field<string>("TaxId"),
                    vendor_id = r.Field<string>("VendorId"),
                    purchaseorder_number = r.Field<string>("PONo"),
                    reference_number = r.Field<string>("BookingCode"),
                    date = r.Field<string>("BookedDt"),
                    delivery_date = r.Field<string>("CheckInDate"),
                    payment_terms = r.Field<int>("PaymentTerms"),
                    payment_terms_label = r.Field<string>("PaymentTermsLabel"),
                    is_inclusive_tax = r.Field<bool>("InclusiveTax"),
                    notes = r.Field<string>("Notes"),
                    terms = r.Field<string>("Terms"),
                    Zoho_Branch_Id = r.Field<string>("Zoho_Branch_Id"),
                    RoomCaptured = r.Field<int>("RoomCaptured"),
                }).ToList();

                if (myData[0].vendor_id != "0")
                {
                    var line_items = new List<LineItemDt>();
                    int Tbl1RCount = myData.Count;
                    for (var j = 0; j < Tbl1RCount; j++)
                    {
                        line_items.Add(new LineItemDt
                        {
                            item_id = myData[j].item_id,
                            rate = myData[j].rate,
                            quantity = myData[j].quantity,
                            tax_id = myData[j].tax_id,
                            item_order = myData[j].RoomCaptured
                        });
                    }

                    body = new JavaScriptSerializer().Serialize(new
                    {
                        vendor_id = myData[0].vendor_id,
                        purchaseorder_number = myData[0].purchaseorder_number,
                        reference_number = myData[0].reference_number,
                        date = myData[0].date,
                        delivery_date = myData[0].delivery_date,
                        payment_terms = myData[0].payment_terms,
                        payment_terms_label = myData[0].payment_terms_label,
                        is_inclusive_tax = myData[0].is_inclusive_tax,
                        notes = myData[0].notes,
                        terms = myData[0].terms,
                        branch_id = myData[0].Zoho_Branch_Id,
                        line_items

                    });

                    streamWrite.Write(body);
                    streamWrite.Close();
                    HttpWebResponse wrres = (HttpWebResponse)httpReq.GetResponse();
                    StreamReader strmReader = new StreamReader(wrres.GetResponseStream(), Encoding.Default, true);
                    string Resobj1 = strmReader.ReadToEnd();
                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                    PORootRes POResponse = jsonSerializer.Deserialize<PORootRes>(Resobj1);

                    if (POResponse.code == 0)
                    {

                        SqlCommand command2 = new SqlCommand();
                        DataSet ds2 = new DataSet();
                        command2.CommandText = "SP_Zoho_PO_Help";
                        command2.CommandType = CommandType.StoredProcedure;
                        command2.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "POUpdate";
                        command2.Parameters.Add("@Id", SqlDbType.BigInt).Value = All.BookingId;
                        command2.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = POResponse.purchaseorder.purchaseorder_id;
                        ds2 = new DBconnection().ExecuteDataSet(command2, "");

                        foreach (var item in POResponse.purchaseorder.line_items)
                        {

                            SqlCommand command3 = new SqlCommand();
                            DataSet ds3 = new DataSet();
                            command3.CommandText = "SP_Zoho_PO_LineItem_Update";
                            command3.CommandType = CommandType.StoredProcedure;
                            command3.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "Update";
                            command3.Parameters.Add("@Id", SqlDbType.BigInt).Value = All.BookingId;
                            command3.Parameters.Add("@OrderId", SqlDbType.Int).Value = item.item_order;
                            command3.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = item.line_item_id;
                            ds3 = new DBconnection().ExecuteDataSet(command3, "");

                        }


                        }
                    else
                    {
                        CreateLogFiles log = new CreateLogFiles();
                        log.ErrorLog(" => Confirmation Email API => PO Insert => Err msg => Booking Id -" + All.BookingId + " => " + POResponse.message);
                    }
                }
                else
                {
                    //SqlCommand command5 = new SqlCommand();
                    //DataSet ds5 = new DataSet();
                    //command5.CommandText = "SP_SMTPMailSetting_Help";
                    //command5.CommandType = CommandType.StoredProcedure;
                    //command5.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "SMTP";
                    //command5.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = "";
                    //command5.Parameters.Add("@Id", SqlDbType.BigInt).Value = 0;
                    //ds5 = new DBconnection().ExecuteDataSet(command5, "");
                    //string Host = ds5.Tables[0].Rows[0][0].ToString();
                    //string CredentialsUserName = ds5.Tables[0].Rows[0][1].ToString();
                    //string CredentialsPassword = ds5.Tables[0].Rows[0][2].ToString();
                    //int Port = Convert.ToInt16(ds5.Tables[0].Rows[0][3]);

                    //System.Net.Mail.MailMessage message2 = new System.Net.Mail.MailMessage();
                    //message2.From = new System.Net.Mail.MailAddress("noreply@hummingbirdindia.com", "noreply", System.Text.Encoding.UTF8);
                    //message2.To.Add(new System.Net.Mail.MailAddress("prabakaran@warblerit.com"));
                    ////message2.To.Add(new System.Net.Mail.MailAddress("nandhu@warblerit.com"));
                    ////message2.To.Add(new System.Net.Mail.MailAddress("vivek@warblerit.com"));
                    ////message2.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                    //message2.Subject = "PO Creation Status - " + All.BookingCode;
                    //string Imagebody = "<div style=\"text-align: center;  padding: 10px 0 10px 0;\">" +
                    //       "<img src=\"https://portalvhds4prl9ymlwxnt8.blob.core.windows.net/img/HB_logo_small.png\" alt=\"logo\">" +
                    //       "</div>";
                    //string header = "<div  style=\"width: 700px;margin: 10px auto;\">" +
                    //         "<p style=\"margin: 0; padding: 10px 0 10px 0; color: #556DE5; letter-spacing: 125%; word-spacing: 150%;font-weight: bold; font-family: 'Open Sans', sans-serif\">Hello ,</p>" +
                    //         "<p style=\"margin: 0; padding: 10px 0 10px 0;  letter-spacing: 125%; word-spacing: 150%;font-weight: bold; font-family: 'Open Sans', sans-serif\">Purchase order not created because of vendor id is not exists. Please find the details below for your reference.</p>" +
                    //         "<table style=\"background-color: #ffffff;border-radius: 4px 4px 0 0;overflow: hidden; width: 100%;margin-bottom: 1rem;background-color: transparent;border-collapse: collapse;font-family: ''Open Sans', sans-serif';\">" +
                    //         "<thead style=\"text-align: left;\">" +
                    //         "<tr>" +
                    //         "<th style=\"padding-left: 5px;background-color: #ebebeb;padding: 20px;border-bottom: transparent;color: #000000;font-weight: bold;border-color: #dee2e6;  vertical-align: bottom;border: 1px solid #dee2e6;padding: .75rem;vertical-align: top;width: 30%\">Booking Code</th>" +
                    //         "<th style=\"padding-left: 5px;background-color: #ebebeb;padding: 20px;border-bottom: transparent;color: #000000;font-weight: bold;border-color: #dee2e6;  vertical-align: bottom;border: 1px solid #dee2e6;padding: .75rem;vertical-align: top;width: 50%\">Property Name</th>" +
                    //         "</tr>" +
                    //         "</thead>" +
                    //         "<tbody>" +
                    //         "<tr>" +
                    //         "<td style=\"padding-left: 5px;padding: .75rem;vertical-align: top;border: 1px solid #dee2e6;width: 30%\">" + All.BookingCode + "</td>" +
                    //         "<td style=\"padding-left: 5px;padding: .75rem;vertical-align: top;border: 1px solid #dee2e6;width: 50%\">" + All.PropertyName + "</td>" +
                    //         "<tr/>" +
                    //         "</tbody>" +
                    //         "</table>";

                    //string footer = "<br /><br />" +
                    //    "<p style=\"margin: 0; padding: 10px 0 10px 0; color: #556DE5; letter-spacing: 125%; word-spacing: 150%;font-weight: bold; font-family: 'Open Sans', sans-serif\"> Warm Regards,</p>" +
                    //    "<p style=\"margin: 0; padding: 0px 0 10px 0; color: #556DE5; letter-spacing: 125%; word-spacing: 150%;font-weight: bold; font-family: 'Open Sans', sans-serif\"> Technical Team</p><br /></div>";


                    //message2.Body = Imagebody + header + footer;
                    //message2.IsBodyHtml = true;
                    //System.Net.Mail.SmtpClient smtp2 = new System.Net.Mail.SmtpClient();
                    //smtp2.EnableSsl = true;
                    //smtp2.Port = Port;
                    //smtp2.Host = Host;
                    //smtp2.Credentials = new System.Net.NetworkCredential(CredentialsUserName, CredentialsPassword);
                    //try
                    //{
                    //    smtp2.Send(message2);
                    //}
                    //catch (Exception ex)
                    //{
                    //    CreateLogFiles log = new CreateLogFiles();
                    //    log.ErrorLog(" => Confirmation Email API => Error => Vendor id not exists =>  Mail Block =>  msg => " + ex.Message);

                    //}

                    SqlCommand command = new SqlCommand();
                    DataSet ds = new DataSet();
                    command.CommandText = "SP_ZohoContactVendor_PO_Help";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "GetHotelDetails";
                    command.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = "";
                    command.Parameters.Add("@Id", SqlDbType.BigInt).Value = All.BookingId;
                    ds = new DBconnection().ExecuteDataSet(command, "");
                    var myDatas = ds.Tables[0].AsEnumerable().Select(r => new ZohoPropertyDtls
                    {

                        PropertyName = r.Field<string>("PropertyName"),
                        LegalName = r.Field<string>("LegalName"),
                        LegalAddress = r.Field<string>("LegalAddress"),
                        City = r.Field<string>("City"),
                        State = r.Field<string>("State"),
                        Postal = r.Field<string>("Postal"),
                        CreditPeriod = r.Field<int>("CreditPeriod"),
                        GSTNumber = r.Field<string>("GSTNumber"),
                        PropertyId = r.Field<Int64>("PropertyId")
                    }).ToList();
                    //Zoho Vendor Creation Insert Start

                    try
                        {
                            string FinalAPIUrl = "";
                            FinalAPIUrl = System.Configuration.ConfigurationManager.AppSettings["UrlShortner"] + "/API/zohocontact/zohohotelcontactcreate";

                            WebRequest webReq1 = WebRequest.Create("https://zohoapi.staysimplyfied.com/API/zohocontact/zohohotelcontactcreate");
                            webReq1.Proxy = null;
                            HttpWebRequest httpReq1 = (HttpWebRequest)webReq1;
                            httpReq1.ContentType = "application/json";
                            httpReq1.Method = "POST";
                            httpReq1.ProtocolVersion = HttpVersion.Version11;
                            httpReq1.Credentials = CredentialCache.DefaultCredentials;
                            Stream reqStream1 = httpReq1.GetRequestStream();
                            StreamWriter streamWrite1 = new StreamWriter(reqStream1);
                            var billing_address = new
                            {
                                attention = "",
                                address = myDatas[0].LegalAddress,
                                street2 = "",
                                state_code = "",
                                city = myDatas[0].City,
                                state = myDatas[0].State,
                                zip = myDatas[0].Postal,
                                country = "India",
                                fax = "",
                                phone = ""
                            };
                            var shipping_address = new
                            {
                                attention = "",
                                address = myDatas[0].LegalAddress,
                                street2 = "",
                                state_code = "",
                                city = myDatas[0].City,
                                state = myDatas[0].State,
                                zip = myDatas[0].Postal,
                                country = "India",
                                fax = "",
                                phone = ""
                            };
                            var custom_fields = new
                            {
                               
                            };



                            string body1 = new JavaScriptSerializer().Serialize(new
                            {

                                contact_name = myDatas[0].PropertyName,
                                place_of_contact = "", //Source of Display
                                currency_id = "", //currency Code
                                company_name = myDatas[0].LegalName, //Legal Name
                                website = "",
                                contact_type = "vendor",
                                customer_sub_type = "",
                                is_portal_enabled = true,
                                custom_fields = custom_fields,
                                billing_address = billing_address,
                                shipping_address = shipping_address,
                                payment_terms = myDatas[0].CreditPeriod,
                                payment_terms_label = "Net" + " " + myDatas[0].CreditPeriod,
                                notes = "Hotel",
                                gst_no = myDatas[0].GSTNumber,
                                gst_treatment = "",
                                PropertyId = myDatas[0].PropertyId

                            });
                            streamWrite1.Write(body1);
                            streamWrite1.Close();
                            HttpWebResponse wrres = (HttpWebResponse)httpReq1.GetResponse();
                            StreamReader strmReader = new StreamReader(wrres.GetResponseStream(), Encoding.Default, true);
                            string Resobj2 = strmReader.ReadToEnd();
                            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            RootObjNew Response = jsonSerializer.Deserialize<RootObjNew>(Resobj2);
                        //Zoho Vendor Creation Insert End


                        //Get VendorId
                            SqlCommand commands = new SqlCommand();
                            DataSet dss = new DataSet();
                            commands.CommandText = "SP_ZohoContactVendor_PO_Help";
                            commands.CommandType = CommandType.StoredProcedure;
                            commands.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "GetVendorId";
                            commands.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = "";
                            commands.Parameters.Add("@Id", SqlDbType.BigInt).Value = All.BookingId;
                            dss = new DBconnection().ExecuteDataSet(commands, "");
                            string Vendor_Id = dss.Tables[0].Rows[0][0].ToString();

                        string URL1 = "https://books.zoho.in/api/v3/purchaseorders?organization_id=" + OrganizationId;

                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                               | SecurityProtocolType.Tls11
                               | SecurityProtocolType.Tls12
                               | SecurityProtocolType.Ssl3;
                        WebRequest webReq3 = WebRequest.Create(URL1);
                        webReq3.Proxy = null;
                        HttpWebRequest httpReq3 = (HttpWebRequest)webReq3;
                        httpReq3.ContentType = "application/json";
                        httpReq3.Method = "POST";
                        httpReq3.Headers.Add("Authorization", "Zoho-oauthtoken " + AccessToken);
                        httpReq3.ProtocolVersion = HttpVersion.Version11;
                        httpReq3.Credentials = CredentialCache.DefaultCredentials;
                        Stream reqStream3 = httpReq3.GetRequestStream();
                        StreamWriter streamWrite3 = new StreamWriter(reqStream3);
                        var line_items = new List<LineItemDt>();
                        int Tbl1RCount = myData.Count;
                        for (var j = 0; j < Tbl1RCount; j++)
                        {
                            line_items.Add(new LineItemDt
                            {
                                item_id = myData[j].item_id,
                                rate = myData[j].rate,
                                quantity = myData[j].quantity,
                                tax_id = myData[j].tax_id,
                                item_order = myData[j].RoomCaptured
                            });
                        }

                        string body3 = new JavaScriptSerializer().Serialize(new
                        {
                            vendor_id = Vendor_Id,
                            purchaseorder_number = myData[0].purchaseorder_number,
                            reference_number = myData[0].reference_number,
                            date = myData[0].date,
                            delivery_date = myData[0].delivery_date,
                            payment_terms = myData[0].payment_terms,
                            payment_terms_label = myData[0].payment_terms_label,
                            is_inclusive_tax = myData[0].is_inclusive_tax,
                            notes = myData[0].notes,
                            terms = myData[0].terms,
                            branch_id = myData[0].Zoho_Branch_Id,
                            line_items

                        });

                        streamWrite3.Write(body3);
                        streamWrite3.Close();
                        HttpWebResponse wrres3 = (HttpWebResponse)httpReq3.GetResponse();
                        StreamReader strmReader3 = new StreamReader(wrres3.GetResponseStream(), Encoding.Default, true);
                        string Resobj3 = strmReader3.ReadToEnd();
                        JavaScriptSerializer jsonSerializer3 = new JavaScriptSerializer();
                        PORootRes POResponses = jsonSerializer3.Deserialize<PORootRes>(Resobj3);

                        if (POResponses.code == 0)
                        {

                            SqlCommand command2 = new SqlCommand();
                            DataSet ds2 = new DataSet();
                            command2.CommandText = "SP_Zoho_PO_Help";
                            command2.CommandType = CommandType.StoredProcedure;
                            command2.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "POUpdate";
                            command2.Parameters.Add("@Id", SqlDbType.BigInt).Value = All.BookingId;
                            command2.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = POResponses.purchaseorder.purchaseorder_id;
                            ds2 = new DBconnection().ExecuteDataSet(command2, "");

                            foreach (var item in POResponses.purchaseorder.line_items)
                            {

                                SqlCommand command3 = new SqlCommand();
                                DataSet ds3 = new DataSet();
                                command3.CommandText = "SP_Zoho_PO_LineItem_Update";
                                command3.CommandType = CommandType.StoredProcedure;
                                command3.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "Update";
                                command3.Parameters.Add("@Id", SqlDbType.BigInt).Value = All.BookingId;
                                command3.Parameters.Add("@OrderId", SqlDbType.Int).Value = item.item_order;
                                command3.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = item.line_item_id;
                                ds3 = new DBconnection().ExecuteDataSet(command3, "");

                            }


                        }
                    }
                        catch (Exception ex)
                        {
                            log = new CreateLogFiles();
                            log.ErrorLog("Property Management Zoho Insert => Err msg => Missed Property Add => " + ex.Message);

                        }
                    
                }

                return All.BookingId.ToString();
            }
            catch (Exception Ex)
            {
                log = new CreateLogFiles();
                log.ErrorLog(" => Confirmation Email API => BookingId => " + All.BookingId + "=> Zoho PO Insert" + Ex.Message + "=>" + body);
                return All.BookingId.ToString();
            }

        }


        [HttpPost]
        [Route("ConfirmNewEMail")]
        public IHttpActionResult ConfirmNewEMail(ConfirmationEMail All)
        {
            try
            {
                string Response = "";
                string Response1 = "Failure";
                string Response2 = "Failure";
                string Newid = "";
                String AzureBlobPdfURl = "";
                SqlCommand command5 = new SqlCommand();
                DataSet ds5 = new DataSet();
                command5.CommandText = "SP_SMTPMailSetting_Help";
                command5.CommandType = CommandType.StoredProcedure;
                command5.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "SMTP";
                command5.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = "";
                command5.Parameters.Add("@Id", SqlDbType.BigInt).Value = 0;
                ds5 = new DBconnection().ExecuteDataSet(command5, "");
                string Host = ds5.Tables[0].Rows[0][0].ToString();
                string CredentialsUserName = ds5.Tables[0].Rows[0][1].ToString();
                string CredentialsPassword = ds5.Tables[0].Rows[0][2].ToString();
                int Port = Convert.ToInt16(ds5.Tables[0].Rows[0][3]);


                SqlCommand command = new SqlCommand();
                DataSet ds = new DataSet();
                command.CommandText = "SP_ConfirmationEMail_Help";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@Str", SqlDbType.NVarChar).Value = "";
                command.Parameters.Add("@Id", SqlDbType.BigInt).Value = All.BookingId;
                ds = new DBconnection().ExecuteDataSet(command, "");


                #region
                if (All.GuestMailChk == true)
                {
                    System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
                    smtp.Port = Port;
                    smtp.Host = Host; smtp.Credentials = new System.Net.NetworkCredential(CredentialsUserName, CredentialsPassword);
                    smtp.EnableSsl = true;
                    string MailContent = "";

                    #region
                    if (ds.Tables[0].Rows[0][8].ToString() == "Bed")
                    {
                        if (ds.Tables[10].Rows.Count > 0)
                        {
                            message.From = new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][0].ToString(), "", System.Text.Encoding.UTF8);
                        }
                        else
                        {
                            message.From = new System.Net.Mail.MailAddress("stay@hummingbirdindia.com", "", System.Text.Encoding.UTF8);
                        }

                        if (All.ResendFlag == true)
                        {
                            var Mail = All.PropertyGusetEmail.Split(',');
                            for (int i = 0; i < Mail.Length; i++)
                            {
                                try
                                {
                                    message.To.Add(new System.Net.Mail.MailAddress(Mail[i].ToString()));
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + Mail[i].ToString());
                                }
                            }
                            if (All.UserEmail != "")
                            {
                                try
                                {
                                    message.CC.Add(new System.Net.Mail.MailAddress(All.UserEmail));
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + All.UserEmail);
                                }
                            }
                            message.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                        }
                        else
                        {
                            if (ds.Tables[4].Rows[0][0].ToString() == "0")
                            {
                                if (ds.Tables[8].Rows[0][0].ToString() != "")
                                {
                                    message.To.Add(new System.Net.Mail.MailAddress(ds.Tables[8].Rows[0][0].ToString()));
                                }
                            }
                            else
                            {
                                for (int i = 0; i < ds.Tables[5].Rows.Count; i++)
                                {
                                    if (i <= 40)
                                    {
                                        if (ds.Tables[5].Rows[i][0].ToString() != "")
                                        {
                                            try
                                            {
                                                message.To.Add(new System.Net.Mail.MailAddress(ds.Tables[5].Rows[i][0].ToString()));
                                            }
                                            catch (Exception ex)
                                            {
                                                CreateLogFiles log = new CreateLogFiles();
                                                log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + ds.Tables[5].Rows[i][0].ToString());
                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                ////if (ds.Tables[8].Rows[0][0].ToString() != "")
                                ////{
                                ////    try
                                ////    {
                                ////        message.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[8].Rows[0][0].ToString()));
                                ////    }
                                ////    catch (Exception ex)
                                ////    {
                                ////        CreateLogFiles log = new CreateLogFiles();
                                ////        log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => CC =>" + ds.Tables[8].Rows[0][0].ToString());
                                ////    }
                                ////}
                            }
                            //////Extra CC
                            ////for (int i = 0; i < ds.Tables[7].Rows.Count; i++)
                            ////{
                            ////    if (ds.Tables[7].Rows[i][0].ToString() != "")
                            ////    {
                            ////        try
                            ////        {
                            ////            message.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[7].Rows[i][0].ToString()));
                            ////        }
                            ////        catch (Exception ex)
                            ////        {
                            ////            CreateLogFiles log = new CreateLogFiles();
                            ////            log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => Extra CC =>" + ds.Tables[7].Rows[i][0].ToString());
                            ////        }
                            ////    }
                            ////}
                            ////// Extra CC email from Front end
                            ////if (ds.Tables[8].Rows[0][1].ToString() != "")
                            ////{
                            ////    string ExtraCC = ds.Tables[8].Rows[0][1].ToString();
                            ////    var ExtraCCEmail = ExtraCC.Split(',');
                            ////    int cnt = ExtraCCEmail.Length;
                            ////    for (int i = 0; i < cnt; i++)
                            ////    {
                            ////        if (ExtraCCEmail[i].ToString() != "")
                            ////        {
                            ////            try
                            ////            {
                            ////                message.CC.Add(new System.Net.Mail.MailAddress(ExtraCCEmail[i].ToString()));
                            ////            }
                            ////            catch (Exception ex)
                            ////            {
                            ////                CreateLogFiles log = new CreateLogFiles();
                            ////                log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => Extra CC From Front End =>" + ExtraCCEmail[i].ToString());
                            ////            }
                            ////        }
                            ////    }
                            ////}
                            ////if (ds.Tables[2].Rows[0][4].ToString() != "")
                            ////{
                            ////    try
                            ////    {
                            ////        message.Bcc.Add(new System.Net.Mail.MailAddress(ds.Tables[2].Rows[0][4].ToString()));
                            ////    }
                            ////    catch (Exception ex)
                            ////    {
                            ////        CreateLogFiles log = new CreateLogFiles();
                            ////        log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => BCc =>" + ds.Tables[2].Rows[0][4].ToString());
                            ////    }
                            ////}
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress("booking_confirmation@staysimplyfied.com"));
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress("bookingbcc@staysimplyfied.com"));
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                        }
                        message.Bcc.Add(new System.Net.Mail.MailAddress("anbu@warblerit.com"));
                        

                        message.Subject = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString();

                        // Client Logo
                        string Imagelocation = "";
                        string Imagealt = "";
                        string PtyType = ds.Tables[5].Rows[0][1].ToString();
                        if (PtyType == "MGH")
                        {
                            Imagelocation = ds.Tables[6].Rows[0][4].ToString();
                            Imagealt = ds.Tables[6].Rows[0][5].ToString();
                            if (Imagelocation == "")
                            {
                                Imagelocation = ds.Tables[6].Rows[0][0].ToString();
                                Imagealt = ds.Tables[6].Rows[0][1].ToString();
                            }
                        }
                        else
                        {
                            Imagelocation = ds.Tables[6].Rows[0][0].ToString();
                            Imagealt = ds.Tables[6].Rows[0][1].ToString();
                        }

                        // Contact Email And Phone
                        string ContactEmail = "";
                        string DeskNo = "";
                        DeskNo = ds.Tables[2].Rows[0][13].ToString();
                        ContactEmail = ds.Tables[2].Rows[0][14].ToString();

                        // Map Link
                        string MapLink = "";
                        if (ds.Tables[1].Rows[0][13].ToString() != "")
                        {
                            MapLink = "https://www.google.co.in/maps/place/" + ds.Tables[1].Rows[0][13].ToString();
                        }
                        else
                        {
                            MapLink = "#";
                        }

                        // View in browser Link
                        string id = ds.Tables[2].Rows[0][11].ToString();
                        string link = "http://mybooking.hummingbirdindia.com/?redirect=BookingConfirmation&B=B&R=" + id;

                        // Spl Note
                        string SplNote = ds.Tables[2].Rows[0][8].ToString();
                        if (SplNote == "")
                        {
                            SplNote = "- NA -";
                        }
                        string header = "";

                        if (ds.Tables[2].Rows[0][15].ToString() != "")
                        {
                            header = "<div style =\"background - color:#f9fafc\" >" +
                            "<div style =\"background-color:#ffffff;width:800px;margin:0 auto\" >" +
                            "<div style =\"padding:10px 40px\">" +
                            "<div style =\"width:60%;display:inline-block\">" +
                            "<div style =\"border-radius:20px;padding:10px 10px;\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:25px;padding:10px 0\">Confirmation Voucher</h3>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > Hummingbird Booking Code - <b>" + ds.Tables[2].Rows[0][2].ToString() + " </b></p>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > Hotel Confirmation / Ref No -<b>" + ds.Tables[2].Rows[0][15].ToString() + "</b></p>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:13px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > <b>" + ds.Tables[2].Rows[0][7].ToString() + "</b></p>" +
                            "</div></div>" +
                            "<div style =\"width:39%;display:inline-block;text-align:right;vertical-align:text-bottom\"><img align =\"center\" alt=\"" + Imagealt + "\" class=\"center standard-header\" src=\"" + Imagelocation + "\" style=\"max-width: 200px\" ></a></div>" +
                            "</div></div>";
                        }
                        else
                        {
                            header = "<div style =\"background - color:#f9fafc\" >" +
                            "<div style =\"background-color:#ffffff;width:800px;margin:0 auto\" >" +
                            "<div style =\"padding:10px 40px\">" +
                            "<div style =\"width:60%;display:inline-block\">" +
                            "<div style =\"border-radius:20px;padding:10px 10px;\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:25px;padding:10px 0\">Confirmation Voucher</h3>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > Hummingbird Booking Code - <b>" + ds.Tables[2].Rows[0][2].ToString() + " </b></p>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:13px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > <b>" + ds.Tables[2].Rows[0][7].ToString() + "</b></p>" +
                            "</div></div>" +
                            "<div style =\"width:39%;display:inline-block;text-align:right;vertical-align:text-bottom\"><img align =\"center\" alt=\"" + Imagealt + "\" class=\"center standard-header\" src=\"" + Imagelocation + "\" style=\"max-width: 200px\" ></a></div>" +
                            "</div></div>";
                        }

                        

                        string BookingDetails = "";
                        if (All.LTIAPIFlag == true)
                        {
                            BookingDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                        "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\">Booking Details<u></u></h3>" +
                        "</div>" +
                        "<table style =\"border-collapse:collapse\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Guest Name </strong></td>" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Room Type </strong></td >" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Check In </strong></td>" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Check Out </strong></td >" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Tariff (RS) </strong></td >" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Tariff Payment </strong></td>" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Services Payment </strong></td>" +
                        "</tr>";

                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                BookingDetails += "<tr style =\"font-style:normal;font-weight:normal;border-bottom:1px solid #ebebeb\">" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][0].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][7].ToString() + "/" + ds.Tables[0].Rows[i][4].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][9].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][10].ToString() + "</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][3].ToString() + "</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][5].ToString() + "</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][6].ToString() + "</td>" +
                                "</tr></tbody>";

                            }
                            BookingDetails += "</table>" +
                                "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                                "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u></u></h3>" +
                                "</div>" +
                                 "<table style =\"border-collapse:collapse;width:100%;\">" +
                                 "<tbody style=\"width:100%\">" +
                                 "<tr><td style=\"width:50%;text-align:left; \">" +
                                 "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;font-size:15px;padding:10px 0\"> Note  : <span style =\"font-weight:normal\">" + ds.Tables[2].Rows[0][8].ToString() + "</span></h3></td>" +
                                 "<tr><td style=\"width:50%;text-align:center; \">" +
                                 "</td></tr>"+
                                 "</tr></tbody></table>";
                        }
                        else
                        {
                            BookingDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                        "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\">Booking Details<u></u></h3>" +
                        "</div>" +
                        "<table style =\"border-collapse:collapse\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Guest Name </strong></td>" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Room Type </strong></td >" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Check In </strong></td>" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Check Out </strong></td >" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Tariff (RS) </strong></td >" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Tariff Payment </strong></td>" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Services Payment </strong></td>" +
                        "</tr>";

                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                BookingDetails += "<tr style =\"font-style:normal;font-weight:normal;border-bottom:1px solid #ebebeb\">" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][0].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][7].ToString() + "/" + ds.Tables[0].Rows[i][4].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][1].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][2].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][3].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][5].ToString() + "</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][6].ToString() + "</td>" +
                                "</tr></tbody>";

                            }
                            BookingDetails += "</table>" +
                               "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                               "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u></u></h3>" +
                               "</div>" +
                                "<table style =\"border-collapse:collapse;width:100%;\">" +
                                "<tbody style=\"width:100%\">" +
                                "<tr><td style=\"width:50%;text-align:left; \">" +
                                "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;font-size:15px;padding:10px 0\"> Note  : <span style =\"font-weight:normal\">" + ds.Tables[2].Rows[0][8].ToString() + "</span></h3></td>" +
                                "<tr><td style=\"width:50%;text-align:center; \">" +
                                 "</td></tr>" +
                                "</tr></tbody></table>";
                        }
                        string HotelDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u>Hotel Information</u></h3>" +
                            "</div>" +
                            "<table style =\"border:#dbdbdb\"><tbody><tr>" +
                            "<td style =\"font-size:13px;width:14%\" valign = \"top\" align =\"center\"><strong></strong></td>" +
                            "<td style =\"font-size:13px;width:18%\" valign =\"top\" align =\"center\"><strong></strong></td>" +
                            "</tr><tr></tr>" +
                            "<tr style =\"font-style:normal;font-weight:normal\">" +
                            "<td style =\"vertical-align:middle;text-align:left\"><strong> Hotel Name:</strong>" + ds.Tables[1].Rows[0][5].ToString() + "<br/><strong> Address : </strong> " + ds.Tables[1].Rows[0][0].ToString() + "<b><br/> " + ds.Tables[1].Rows[0][1].ToString() + " </b> </ td >" +
                            "<td style =\"vertical-align:middle;text-align:center\" ><a href =" + MapLink + " target =\"_blank\" ><img src =\"https://portalvhds4prl9ymlwxnt8.blob.core.windows.net/img/Google_Maps_Icon.png\" ></a><a href = " + link + " target =\"_blank\"><span style =\"font-family:&#39;Cabin&#39;,Helvetica,Arial,sans-serif;padding:10px 0 10px 16px;margin:0;text-align:left;line-height:1.3;text-decoration:none;font-weight:300;color:#d9242c!important\"> Security / Cancellation Policy </span></a></td>" +
                            "</tr></tbody></table>";

                        string GSTDetails = "";

                        //string GSTDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                        //    "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"> GST Details<u></u></h3>" +
                        //    "</div>" +
                        //    "<table style =\"border-collapse:collapse\">" +
                        //    "<tbody>" +
                        //    "<tr style =\"border-bottom:1px solid #808080;width:100%;\">" +
                        //    "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong> GST Number </strong></td>" +
                        //    "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong> Legal Name </strong></td>" +
                        //    "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong> Address </strong></td>" +
                        //    "</tr>" +
                        //    "<tr style =\"font-style:normal;font-weight:normal;border-bottom:1px solid #ebebeb\">" +
                        //    "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[12].Rows[0][1].ToString() + "</td>" +
                        //    "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[12].Rows[0][0].ToString() + "</td>" +
                        //    "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[12].Rows[0][2].ToString() + "</td>" +
                        //    "</tr></tbody></table>";
                        string OtherDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u>HB Reference</u></h3>" +
                            "</div>" +
                            "<table style =\"border-collapse:collapse;\">" +
                            "<tbody>" +
                            "<tr style =\"border-bottom:1px solid #808080;width:100%;\">" +
                            "<td style =\"font-size:13px;width:33%\" valign =\"top\" align =\"center\"><strong> Client Ref No</strong></td>" +
                            "<td style =\"font-size:13px;width:33%\" valign =\"top\" align =\"center\"><strong> Booker </strong></td>" +
                            "<td style =\"font-size:13px;width:33%\" valign =\"top\" align =\"center\"><strong> Issues / Feedback </strong></td>" +
                            "</tr>" +
                            "<tr style =\"font-style:normal;font-weight:normal;border-bottom:1px solid #ebebeb\">" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[2].Rows[0][13].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[2].Rows[0][3].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[2].Rows[0][14].ToString() + " </td>" +
                            "</tr></tbody></table>" +
                            "<table><tbody><tr>" +
                            "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong></strong></td>" +
                            "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"right\" ><strong> Powered by <a href =\"http://hummingbirdindia.com\" target =\"_blank\">hummingbirdindia.com</a><u></u></strong></td>" +
                            "</tr></tbody></table></div></div>";

                        var PdfContent = "";

                        MailContent = header + BookingDetails + HotelDetails + OtherDetails;
                        PdfContent = header + BookingDetails + HotelDetails + OtherDetails;

                        var htmlContent = String.Format(PdfContent, DateTime.Now);
                        var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
                        var pdfBytes = htmlToPdf.GeneratePdf(htmlContent);
                        string path = @"D:\home\site\wwwroot\Confirmations\";

                        var BFilePathWhatsApp = "";

                        if (Directory.Exists(path))
                        {
                            var FileName = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            if (File.Exists(FileName))
                            {
                                var AttachmentName = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                                long ticks = DateTime.Now.Ticks;
                                byte[] bytes = BitConverter.GetBytes(ticks);
                                Newid = Convert.ToBase64String(bytes).Replace('+', '_').Replace('/', '-').TrimEnd('=');
                                File.WriteAllBytes(path + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                                System.Net.Mail.Attachment att1 = new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf");
                                att1.Name = AttachmentName;
                                message.Attachments.Add(att1);
                                BFilePathWhatsApp = path + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            }
                            else
                            {
                                File.WriteAllBytes(path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                                message.Attachments.Add(new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf"));
                                BFilePathWhatsApp = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            }
                        }
                        else
                        {
                            DirectoryInfo di = Directory.CreateDirectory(path);
                            File.WriteAllBytes(path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                            BFilePathWhatsApp = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                        }

                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        CloudConfigurationManager.GetSetting("StorageConnectionString"));
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer container = blobClient.GetContainerReference("bookingconfirmations");
                        var blob = container.GetBlockBlobReference("Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf");
                        try
                        {
                            using (var filestream = File.OpenRead(BFilePathWhatsApp))
                            {
                                blob.Properties.ContentType = "application/pdf";
                                blob.UploadFromStream(filestream);
                            }
                            //File.Delete(path);

                            AzureBlobPdfURl = blob.SnapshotQualifiedUri.AbsoluteUri;


                        }
                        catch (System.Exception e)
                        {
                            throw e;
                        }
                        message.Body = MailContent;
                        message.IsBodyHtml = true;


                    }
                    #endregion
                    #region
                    else
                    {
                        if (ds.Tables[10].Rows.Count > 0)
                        {
                            message.From = new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][0].ToString(), "", System.Text.Encoding.UTF8);
                        }
                        else
                        {
                            message.From = new System.Net.Mail.MailAddress("stay@hummingbirdindia.com", "", System.Text.Encoding.UTF8);
                        }
                        if (All.ResendFlag == true)
                        {
                            var Mail = All.PropertyGusetEmail.Split(',');
                            for (int i = 0; i < Mail.Length; i++)
                            {
                                try
                                {
                                    message.To.Add(new System.Net.Mail.MailAddress(Mail[i].ToString()));
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + Mail[i].ToString());
                                }
                            }
                            if (All.UserEmail != "")
                            {
                                try
                                {
                                    message.CC.Add(new System.Net.Mail.MailAddress(All.UserEmail));
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + All.UserEmail);
                                }
                            }
                            message.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                        }
                        else
                        {
                            if (ds.Tables[4].Rows[0][0].ToString() == "0")
                            {
                                if (ds.Tables[8].Rows[0][0].ToString() != "")
                                {
                                    message.To.Add(new System.Net.Mail.MailAddress(ds.Tables[8].Rows[0][0].ToString()));
                                }
                            }
                            else
                            {
                                for (int i = 0; i < ds.Tables[5].Rows.Count; i++)
                                {
                                    if (i <= 40)
                                    {
                                        if (ds.Tables[5].Rows[i][0].ToString() != "")
                                        {
                                            message.To.Add(new System.Net.Mail.MailAddress(ds.Tables[5].Rows[i][0].ToString()));
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                ////if (ds.Tables[8].Rows[0][0].ToString() != "")
                                ////{
                                ////    try
                                ////    {
                                ////        message.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[8].Rows[0][0].ToString()));
                                ////    }
                                ////    catch (Exception wer)
                                ////    {
                                ////        CreateLogFiles log = new CreateLogFiles();
                                ////        log.ErrorLog("=> Confirmation Email API => Room Email => Invalid Email => CC => " + ds.Tables[8].Rows[0][0].ToString() +
                                ////            " => BookingId => " + All.BookingId + ", Err Msg => " + wer.Message);
                                ////    }

                                ////}
                            }
                            //////Extra CC
                            ////for (int i = 0; i < ds.Tables[7].Rows.Count; i++)
                            ////{
                            ////    if (ds.Tables[7].Rows[i][0].ToString() != "")
                            ////    {
                            ////        try
                            ////        {
                            ////            message.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[7].Rows[i][0].ToString()));
                            ////        }
                            ////        catch (Exception wer)
                            ////        {
                            ////            CreateLogFiles log = new CreateLogFiles();
                            ////            log.ErrorLog("=> Confirmation Email API => Room Email => Invalid Email => Extra CC => " + ds.Tables[7].Rows[i][0].ToString() +
                            ////                " => BookingId => " + All.BookingId + ", Err Msg => " + wer.Message);
                            ////        }
                            ////    }
                            ////}
                            //////Extra CC email from Front end
                            ////if (ds.Tables[8].Rows[0][2].ToString() != "")
                            ////{
                            ////    string ExtraCC = ds.Tables[8].Rows[0][2].ToString();
                            ////    var ExtraCCEmail = ExtraCC.Split(',');
                            ////    int cnt = ExtraCCEmail.Length;
                            ////    for (int i = 0; i < cnt; i++)
                            ////    {
                            ////        if (ExtraCCEmail[i].ToString() != "")
                            ////        {
                            ////            try
                            ////            {
                            ////                message.CC.Add(new System.Net.Mail.MailAddress(ExtraCCEmail[i].ToString()));
                            ////            }
                            ////            catch (Exception wer)
                            ////            {
                            ////                CreateLogFiles log = new CreateLogFiles();
                            ////                log.ErrorLog("=> Confirmation Email API => Room Email => Invalid Email => Extra CC email from Front end => " + ExtraCCEmail[i].ToString() +
                            ////                    " => BookingId => " + All.BookingId + ", Err Msg => " + wer.Message);
                            ////            }
                            ////        }
                            ////    }
                            ////}
                            ////if (ds.Tables[2].Rows[0][4].ToString() != "")
                            ////{
                            ////    try
                            ////    {
                            ////        message.Bcc.Add(new System.Net.Mail.MailAddress(ds.Tables[2].Rows[0][4].ToString()));
                            ////    }
                            ////    catch (Exception wer)
                            ////    {
                            ////        CreateLogFiles log = new CreateLogFiles();
                            ////        log.ErrorLog("=> Confirmation Email API => Room Email => Invalid Email => Bcc => " + ds.Tables[2].Rows[0][4].ToString() +
                            ////            " => BookingId => " + All.BookingId + ", Err Msg => " + wer.Message);
                            ////    }
                            ////}
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][0].ToString()));
                            ////if (ds.Tables[10].Rows[0][0].ToString() != "stay@hummingbirdindia.com")
                            ////{
                            ////    message.Bcc.Add(new System.Net.Mail.MailAddress("stay@hummingbirdindia.com"));
                            ////}
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                        }
                        message.Bcc.Add(new System.Net.Mail.MailAddress("anbu@warblerit.com"));

                        message.Subject = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString();
                        string typeofpty = ds.Tables[4].Rows[0][8].ToString();
                        string Imagelocation = "";
                        string Imagealt = "";
                        if (typeofpty == "MGH")
                        {
                            Imagelocation = ds.Tables[6].Rows[0][4].ToString();
                            Imagealt = ds.Tables[6].Rows[0][5].ToString();
                            if (Imagelocation == "")
                            {
                                Imagelocation = ds.Tables[6].Rows[0][0].ToString();
                                Imagealt = ds.Tables[6].Rows[0][1].ToString();
                            }
                        }
                        else
                        {
                            Imagelocation = ds.Tables[6].Rows[0][0].ToString();
                            Imagealt = ds.Tables[6].Rows[0][1].ToString();
                        }

                        // Contact Email And Phone
                        string ContactEmail = "";
                        string DeskNo = "";
                        DeskNo = ds.Tables[2].Rows[0][14].ToString();
                        ContactEmail = ds.Tables[2].Rows[0][16].ToString();


                        // Map Link
                        string MapLink = "";
                        if (ds.Tables[1].Rows[0][13].ToString() != "")
                        {
                            MapLink = "https://www.google.co.in/maps/place/" + ds.Tables[1].Rows[0][13].ToString();
                        }
                        else
                        {
                            MapLink = "#";
                        }

                        // View in browser Link
                        string id = ds.Tables[2].Rows[0][12].ToString();
                        string link = "http://mybooking.hummingbirdindia.com/?redirect=BookingConfirmation&B=R&R=" + id;

                        // Spl Note
                        string SplNote = ds.Tables[2].Rows[0][8].ToString();
                        if (SplNote == "")
                        {
                            SplNote = "- NA -";
                        }

                        string BOKCreditcardView = "";
                        if (typeofpty == "BOK")
                        {
                            BOKCreditcardView = "<tr class=\"\" style=\"padding:0;text-align:left\">" +
                                            "<th style=\"width: 60%;\"><a style = \"font-size:13px; padding:10px 10px 10px 10px;\" align =\"right\" href=" + link + "&C=BOK#Creditcard" + ">UPDATE YOUR CREDIT CARD INFO</a>" +
                                            "</th><th style=\"font-size:10px;padding:10px 16px 10px 0;line-height:28px;text-align:right;\" >" +
                                            "</th></tsr>";

                        }
                        string header = "<div style =\"background - color:#f9fafc\" >" +
                    "<div style =\"background-color:#ffffff;width:800px;margin:0 auto\" >" +
                    "<div style =\"padding:10px 40px\">" +
                    "<div style =\"width:60%;display:inline-block\">" +
                    "<div style =\"border-radius:20px;padding:10px 10px;\">" +
                    "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:25px;padding:10px 0\">Confirmation Voucher</h3>" +
                    "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > Hummingbird Booking ID - <b>" + ds.Tables[2].Rows[0][2].ToString() + " </b></p>" +
                    "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > Hotel Confirmation / Ref No -<b>" + ds.Tables[2].Rows[0][15].ToString() + "</b></p>" +
                    "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:13px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > " + ds.Tables[2].Rows[0][7].ToString() + "</p>" +
                    "</div></div>" +
                    "<div style =\"width:39%;display:inline-block;text-align:right;vertical-align:text-bottom\"><img align =\"center\" alt=\"" + Imagealt + "\" class=\"center standard-header\" src=\"" + Imagelocation + "\" style=\"max-width: 200px\"></a></div>" +
                    "</div></div>";

                        string BookingDetails = "";
                        if (All.LTIAPIFlag == true)
                        {
                            BookingDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                        "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\">Booking Details<u></u></h3>" +
                        "</div>" +
                        "<table style =\"border-collapse:collapse\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Guest Name </strong></td>" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Room Type </strong></td >" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Check In </strong></td>" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Check Out </strong></td >" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Tariff (RS) </strong></td >" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Tariff Payment </strong></td>" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Services Payment </strong></td>" +
                        "</tr>";
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                BookingDetails += "<tr style =\"font-style:normal;font-weight:normal;\">" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;padding:5px 0;\"> " + ds.Tables[i].Rows[0][0].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;padding:5px 0;\"> " + ds.Tables[i].Rows[0][7].ToString() + "/" + ds.Tables[0].Rows[i][4].ToString() +" </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;padding:5px 0;\"> " + ds.Tables[i].Rows[0][9].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;padding:5px 0;\"> " + ds.Tables[i].Rows[0][10].ToString() +"</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;padding:5px 0;\"> " + ds.Tables[i].Rows[0][3].ToString() + "</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;padding:5px 0;\"> " + ds.Tables[i].Rows[0][5].ToString() + "</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;padding:5px 0;\"> " + ds.Tables[i].Rows[0][6].ToString() + "</td>" +
                                "</tr></tbody>";
                            }
                            BookingDetails += "</table>" +
                           "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                           "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u></u></h3>" +
                           "</div>" +
                            "<table style =\"border-collapse:collapse;width:100%;\">" +
                            "<tbody style=\"width:100%\">" +
                            "<tr><td style=\"width:50%;text-align:left; \">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;font-size:15px;padding:10px 0\"> Inclusions : <span style =\"font-weight:normal\">" + ds.Tables[1].Rows[0][12].ToString() + "</span></h3></td>" +
                            "<td style=\"width:50%;text-align:left; \"><h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:15px;padding:10px 0\"> Note : <span style =\"font-weight:normal\">" + ds.Tables[2].Rows[0][8].ToString() + "</span></h3></td>" +
                            "</tr></tbody></table>";
                        }
                        else
                        {
                            BookingDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:font-weight:bold\">Booking Details<u></u></h3>" +
                            "</div>" +
                            "<table style =\"border-collapse:collapse\">" +
                            "<tbody>" +
                            "<tr>" +
                            "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Guest Name </strong></td>" +
                            "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Room Type </strong></td >" +
                            "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Check In </strong></td>" +
                            "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Check Out </strong></td >" +
                            "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Tariff (RS) </strong></td >" +
                            "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Tariff Payment </strong></td>" +
                            "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Services Payment </strong></td>" +
                            "</tr>";
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                BookingDetails += "<tr style =\"font-style:normal;font-weight:normal;border-bottom:1px solid #ebebeb\">" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][0].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][7].ToString() + "/" + ds.Tables[0].Rows[i][4].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;borde:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][1].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][2].ToString() + "</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][3].ToString() + "</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][5].ToString() + "</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][6].ToString() + "</td>" +
                                "</tr></tbody>";
                            }
                            BookingDetails += "</table>" +
                               "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                               "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u></u></h3>" +
                               "</div>" +
                                "<table style =\"border-collapse:collapse;width:100%;\">" +
                                "<tbody style=\"width:100%\">" +
                                "<tr><td style=\"width:50%;text-align:left; \">" +
                                "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;font-size:15px;padding:10px 0\"> Inclusions : <span style =\"font-weight:normal\">" + ds.Tables[1].Rows[0][12].ToString() + "</span></h3></td>" +
                                "<td style=\"width:50%;text-align:left; \"><h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:15px;padding:10px 0\"> Note : <span style =\"font-weight:normal\">" + ds.Tables[2].Rows[0][8].ToString() + "</span></h3></td>" +
                                "</tr></tbody></table>";
                        }
                        string HotelDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u>Hotel Information</u></h3>" +
                            "</div>" +
                            "<table style =\"border:#dbdbdb\"><tbody><tr>" +
                            "<td style =\"font-size:13px;width:14%\" valign = \"top\" align =\"center\"><strong></strong></td>" +
                            "<td style =\"font-size:13px;width:18%\" valign =\"top\" align =\"center\"><strong></strong></td>" +
                            "</tr><tr></tr>" +
                            "<tr style =\"font-style:normal;font-weight:normal\">" +
                            "<td style =\"vertical-align:middle;text-align:left\"><strong> Hotel Name:</strong>" + ds.Tables[1].Rows[0][5].ToString() + "<br/><strong> Address : </strong> " + ds.Tables[1].Rows[0][0].ToString() + "<b><br/> " + ds.Tables[1].Rows[0][1].ToString() + " </b> </ td >" +
                            "<td style =\"vertical-align:middle;text-align:center\" ><a href =" + MapLink + " target =\"_blank\" ><img src =\"https://portalvhds4prl9ymlwxnt8.blob.core.windows.net/img/Google_Maps_Icon.png\" ></a><a href = " + link + " target =\"_blank\"><span style =\"font-family:&#39;Cabin&#39;,Helvetica,Arial,sans-serif;padding:10px 0 10px 16px;margin:0;text-align:left;line-height:1.3;text-decoration:none;font-weight:300;color:#d9242c!important\"> Security / Cancellation Policy </span></a></td>" +
                            "</tr></tbody></table>";

                        string GSTDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\">GST Reference<u></u></h3>" +
                            "</div>" +
                            "<table style =\"border-collapse:collapse;width:100%;\">" +
                            "<tbody>" +
                            "<tr style =\"border-bottom:1px solid #808080\">" +
                            "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong> GST Number </strong></td>" +
                            "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong> Legal Name </strong></td>" +
                            "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong> Address </strong></td>" +
                            "</tr>" +
                            "<tr style =\"font-style:normal;font-weight:normal;border-bottom:1px solid #ebebeb\">" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[12].Rows[0][1].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[12].Rows[0][0].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[12].Rows[0][2].ToString() + "</td>" +
                            "</tr></tbody></table>";
                        string OtherDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u>HB Reference</u></h3>" +
                            "</div>" +
                            "<table style =\"border-collapse:collapse;width:100%;\">" +
                            "<tbody>" +
                            "<tr style =\"border-bottom:1px solid #808080;\">" +
                            "<td style =\"font-size:13px;width:33%\" valign =\"top\" align =\"center\"><strong> Client Ref No</strong></td>" +
                            "<td style =\"font-size:13px;width:33%\" valign =\"top\" align =\"center\"><strong> Booker </strong></td>" +
                            "<td style =\"font-size:13px;width:33%\" valign =\"top\" align =\"center\"><strong> Issues / Feedback </strong></td>" +
                            "</tr>" +
                            "<tr style =\"font-style:normal;font-weight:normal;border-bottom:1px solid #ebebeb\">" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[2].Rows[0][13].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[2].Rows[0][3].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[2].Rows[0][14].ToString() + " </td>" +
                            "</tr></tbody></table>" +
                            "<table><tbody><tr>" +
                            "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong></strong></td>" +
                            "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"right\" ><strong> Powered by <a href =\"http://hummingbirdindia.com\" target =\"_blank\">hummingbirdindia.com</a><u></u></strong></td>" +
                            "</tr></tbody></table></div></div>";

                        var PdfContent = "";
                        if (ds.Tables[0].Rows[0][5].ToString() == "Direct<br>(Cash/Card)")
                        {
                            MailContent = header + BookingDetails + HotelDetails + GSTDetails + OtherDetails;
                            PdfContent = header + BookingDetails + HotelDetails + GSTDetails + OtherDetails;
                        }
                        else
                        {
                            MailContent = header + BookingDetails + HotelDetails + OtherDetails;
                            PdfContent = header + BookingDetails + HotelDetails + OtherDetails;
                        }

                        var htmlContent = String.Format(PdfContent, DateTime.Now);
                        var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
                        var pdfBytes = htmlToPdf.GeneratePdf(htmlContent);
                        string path = @"D:\home\site\wwwroot\Confirmations\";

                        var RFilePathWhatsApp = "";
                        if (Directory.Exists(path))
                        {
                            var FileName = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            if (File.Exists(FileName))
                            {
                                var AttachmentName = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                                long ticks = DateTime.Now.Ticks;
                                byte[] bytes = BitConverter.GetBytes(ticks);
                                Newid = Convert.ToBase64String(bytes).Replace('+', '_').Replace('/', '-').TrimEnd('=');
                                File.WriteAllBytes(path + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                                System.Net.Mail.Attachment att1 = new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf");
                                att1.Name = AttachmentName;
                                RFilePathWhatsApp = path + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                                message.Attachments.Add(att1);

                            }
                            else
                            {
                                File.WriteAllBytes(path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                                message.Attachments.Add(new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf"));
                                RFilePathWhatsApp = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            }
                        }
                        else
                        {
                            DirectoryInfo di = Directory.CreateDirectory(path);
                            File.WriteAllBytes(path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                            RFilePathWhatsApp = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                        }

                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        CloudConfigurationManager.GetSetting("StorageConnectionString"));
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer container = blobClient.GetContainerReference("bookingconfirmations");
                        var blob = container.GetBlockBlobReference("Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf");
                        try
                        {
                            using (var filestream = File.OpenRead(RFilePathWhatsApp))
                            {
                                blob.Properties.ContentType = "application/pdf";
                                blob.UploadFromStream(filestream);
                            }
                            //File.Delete(path);

                            AzureBlobPdfURl = blob.SnapshotQualifiedUri.AbsoluteUri;


                        }
                        catch (System.Exception e)
                        {
                            throw e;
                        }
                        message.Body = MailContent;
                        message.IsBodyHtml = true;
                        if (ds.Tables[2].Rows[0][11].ToString() == "218" && ds.Tables[0].Rows[0][5].ToString() == "Direct<br>(Cash/Card)")
                        {
                            message.Attachments.Add(new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "icici_letter.pdf"));
                        }

                        //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        //CloudConfigurationManager.GetSetting("StorageConnectionString"));
                    }
                    #endregion

                    try
                    {
                        smtp.Send(message);
                        Response1 = "Success";
                    }
                    catch (Exception ex)
                    {
                        CreateLogFiles log = new CreateLogFiles();
                        log.ErrorLog("=> Guest Confirmation Mail => smtp => BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);
                        Response1 = "Failure";
                    }
                }
                else
                {
                    Response1 = "Success";
                }
                #endregion

                #region
                if (All.PropertyMailChk == true)
                {
                    System.Net.Mail.MailMessage message1 = new System.Net.Mail.MailMessage();
                    System.Net.Mail.SmtpClient smtp1 = new System.Net.Mail.SmtpClient();
                    smtp1.Port = Port;
                    smtp1.Host = Host;
                    smtp1.Credentials = new System.Net.NetworkCredential(CredentialsUserName, CredentialsPassword);
                    smtp1.EnableSsl = true;
                    string MailContent = "";

                    #region
                    if (ds.Tables[0].Rows[0][8].ToString() == "Bed")
                    {

                        var ChCnt = 0;
                        var ChCntVal = "txt";

                        if (All.ResendFlag == true)
                        {
                            ChCnt = 1;
                            ChCntVal = "txt";
                        }
                        else
                        {
                            ChCnt = ds.Tables[3].Rows.Count;
                            ChCntVal = ds.Tables[3].Rows[0][4].ToString();
                        }

                        if (ChCnt > 0)
                        {
                            if (ChCntVal != "")
                            {
                                string PropertyMail = ds.Tables[3].Rows[0][4].ToString();
                                var PtyMail = PropertyMail.Split(',');
                                int cnt = PtyMail.Length;

                                if (ds.Tables[10].Rows.Count > 0)
                                {
                                    message1.From = new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][1].ToString(), "", System.Text.Encoding.UTF8);
                                }
                                else
                                {
                                    message1.From = new System.Net.Mail.MailAddress("stay@hummingbirdindia.com", "", System.Text.Encoding.UTF8);
                                }

                                if (All.ResendFlag == true)
                                {
                                    var Mail = All.PropertyGusetEmail.Split(',');
                                    for (int i = 0; i < Mail.Length; i++)
                                    {
                                        try
                                        {
                                            message1.To.Add(new System.Net.Mail.MailAddress(Mail[i].ToString()));
                                        }
                                        catch (Exception ex)
                                        {
                                            CreateLogFiles log = new CreateLogFiles();
                                            log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + Mail[i].ToString());
                                        }
                                    }
                                    if (All.UserEmail != "")
                                    {
                                        try
                                        {
                                            message1.CC.Add(new System.Net.Mail.MailAddress(All.UserEmail));
                                        }
                                        catch (Exception ex)
                                        {
                                            CreateLogFiles log = new CreateLogFiles();
                                            log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + All.UserEmail);
                                        }
                                    }


                                    message1.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));

                                }
                                else
                                {

                                    for (int i = 0; i < cnt; i++)
                                    {
                                        if (PtyMail[i].ToString() != "")
                                        {
                                            try
                                            {
                                                message1.To.Add(new System.Net.Mail.MailAddress(PtyMail[i].ToString()));
                                            }
                                            catch (Exception ex)
                                            {
                                                CreateLogFiles log = new CreateLogFiles();
                                                log.ErrorLog("=> Confirmation Email API => Bed Property Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + PtyMail[i].ToString());
                                            }
                                        }
                                    }
                                    ////for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                                    ////{
                                    ////    if (ds.Tables[3].Rows[i][2].ToString() != "")
                                    ////    {
                                    ////        try
                                    ////        {
                                    ////            message1.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[3].Rows[i][2].ToString()));
                                    ////        }
                                    ////        catch (Exception ex)
                                    ////        {
                                    ////            CreateLogFiles log = new CreateLogFiles();
                                    ////            log.ErrorLog("=> Confirmation Email API => Bed Property Email => BookingId => " + All.BookingId + " => Invaild Email => Cc =>" + ds.Tables[3].Rows[i][2].ToString());
                                    ////        }
                                    ////    }
                                    ////}
                                    ////if (ds.Tables[2].Rows[0][4].ToString() != "")
                                    ////{
                                    ////    try
                                    ////    {
                                    ////        message1.Bcc.Add(ds.Tables[2].Rows[0][4].ToString());
                                    ////    }
                                    ////    catch (Exception ex)
                                    ////    {
                                    ////        CreateLogFiles log = new CreateLogFiles();
                                    ////        log.ErrorLog("=> Confirmation Email API => Bed Property Email => BookingId => " + All.BookingId + " => Invaild Email => Bcc =>" + ds.Tables[2].Rows[0][4].ToString());
                                    ////    }
                                    ////}
                                    ////message1.Bcc.Add(new System.Net.Mail.MailAddress("bookingbcc@staysimplyfied.com"));
                                    ////message1.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));

                                }
                                message1.Bcc.Add(new System.Net.Mail.MailAddress("anbu@warblerit.com"));

                                message1.Subject = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString();


                                string Imagelocation1 = "";
                                string Imagealt1 = "";
                                string PtyType1 = ds.Tables[5].Rows[0][1].ToString();
                                if (PtyType1 == "MGH")
                                {
                                    Imagelocation1 = ds.Tables[6].Rows[0][4].ToString();
                                    Imagealt1 = ds.Tables[6].Rows[0][5].ToString();
                                    if (Imagelocation1 == "")
                                    {
                                        Imagelocation1 = ds.Tables[6].Rows[0][0].ToString();
                                        Imagealt1 = ds.Tables[6].Rows[0][3].ToString();
                                    }
                                }
                                else
                                {
                                    Imagelocation1 = ds.Tables[6].Rows[0][0].ToString();
                                    Imagealt1 = ds.Tables[6].Rows[0][3].ToString();
                                }

                                // Desk No
                                string DeskNo = "";
                                DeskNo = ds.Tables[2].Rows[0][13].ToString();


                                // Guest Mobile No.
                                string MobileNo = ds.Tables[4].Rows[0][2].ToString();
                                if (MobileNo == "")
                                {
                                    MobileNo = " - NA - ";
                                }


                                // Spl Note
                                string SplNote = ds.Tables[2].Rows[0][8].ToString();
                                if (SplNote == "")
                                {
                                    SplNote = "- NA -";
                                }

                                // View in browser Link
                                string id = ds.Tables[2].Rows[0][11].ToString();
                                string link = "http://mybooking.hummingbirdindia.com/?redirect=BookingPropertyConfirmation&B=B&R=" + id;


                                string style = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
                                                <html xmlns=""http://www.w3.org/1999/xhtml"" style=""min-height:100%;background:#f3f3f3"">
                                                <head><meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8""></head>
                                                <body style=""font-size:16px;min-width:100%;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;-moz-box-sizing:border-box;-webkit-box-sizing:border-box;box-sizing:border-box;font-family:'Circular', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;margin:0;line-height:1.3;color:#0a0a0a;text-align:left;width:100% !important"">
                                                <div class=""preheader"" style=""mso-hide:all;visibility:hidden;opacity:0;color:transparent;font-size:0px;width:0;height:0;display:none !important""></div>
                                                <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
                                                <meta name=""viewport"" content=""width=device-width"">
                                                <link href=""https://fonts.googleapis.com/css?family=Cabin&display=swap"" rel=""stylesheet"">
                                                <style data-roadie-ignore data-immutable=""true"">
                                                    @media only screen and (max-width: 596px) {
                                                      .small-float-center {
                                                        margin: 0 auto !important;
                                                        float: none !important;
                                                        text-align: center !important;
                                                      }
                                                      .small-text-center {
                                                        text-align: center !important;
                                                      }
                                                      .small-text-left {
                                                        text-align: left !important;
                                                      }
                                                      .small-text-right {
                                                        text-align: right !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .hide-for-large {
                                                        display: block !important;
                                                        width: auto !important;
                                                        overflow: visible !important;
                                                        max-height: none !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .row.hide-for-large,
                                                      table.body table.container .row.hide-for-large {
                                                        display: table !important;
                                                        width: 100% !important;
                                                      }
                                                    }
    
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .show-for-large {
                                                        display: none !important;
                                                        width: 0;
                                                        mso-hide: all;
                                                        overflow: hidden;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body img {
                                                        width: auto !important;
                                                        height: auto !important;
                                                      }
                                                      table.body center {
                                                        min-width: 0 !important;
                                                      }
                                                      table.body .container {
                                                        width: 95% !important;
                                                      }
                                                      table.body .columns,
                                                      table.body .column {
                                                        height: auto !important;
                                                        -moz-box-sizing: border-box;
                                                        -webkit-box-sizing: border-box;
                                                        box-sizing: border-box;
                                                        padding-left: 16px !important;
                                                        padding-right: 16px !important;
                                                      }
                                                      table.body .columns .column,
                                                      table.body .columns .columns,
                                                      table.body .column .column,
                                                      table.body .column .columns {
                                                        padding-left: 0 !important;
                                                        padding-right: 0 !important;
                                                      }
                                                      table.body .collapse .columns,
                                                      table.body .collapse .column {
                                                        padding-left: 0 !important;
                                                        padding-right: 0 !important;
                                                      }
                                                      td.small-1,
                                                      th.small-1 {
                                                        display: inline-block !important;
                                                        width: 8.33333% !important;
                                                      }
                                                      td.small-2,
                                                      th.small-2 {
                                                        display: inline-block !important;
                                                        width: 16.66667% !important;
                                                      }
                                                      td.small-3,
                                                      th.small-3 {
                                                        display: inline-block !important;
                                                        width: 25% !important;
                                                      }
                                                      td.small-4,
                                                      th.small-4 {
                                                        display: inline-block !important;
                                                        width: 33.33333% !important;
                                                      }
                                                      td.small-5,
                                                      th.small-5 {
                                                        display: inline-block !important;
                                                        width: 41.66667% !important;
                                                      }
                                                      td.small-6,
                                                      th.small-6 {
                                                        display: inline-block !important;
                                                        width: 50% !important;
                                                      }
                                                      td.small-7,
                                                      th.small-7 {
                                                        display: inline-block !important;
                                                        width: 58.33333% !important;
                                                      }
                                                      td.small-8,
                                                      th.small-8 {
                                                        display: inline-block !important;
                                                        width: 66.66667% !important;
                                                      }
                                                      td.small-9,
                                                      th.small-9 {
                                                        display: inline-block !important;
                                                        width: 75% !important;
                                                      }
                                                      td.small-10,
                                                      th.small-10 {
                                                        display: inline-block !important;
                                                        width: 83.33333% !important;
                                                      }
                                                      td.small-11,
                                                      th.small-11 {
                                                        display: inline-block !important;
                                                        width: 91.66667% !important;
                                                      }
                                                      td.small-12,
                                                      th.small-12 {
                                                        display: inline-block !important;
                                                        width: 100% !important;
                                                      }
                                                      .columns td.small-12,
                                                      .column td.small-12,
                                                      .columns th.small-12,
                                                      .column th.small-12 {
                                                        display: block !important;
                                                        width: 100% !important;
                                                      }
                                                      .body .columns td.small-1,
                                                      .body .column td.small-1,
                                                      td.small-1 center,
                                                      .body .columns th.small-1,
                                                      .body .column th.small-1,
                                                      th.small-1 center {
                                                        display: inline-block !important;
                                                        width: 8.33333% !important;
                                                      }
                                                      .body .columns td.small-2,
                                                      .body .column td.small-2,
                                                      td.small-2 center,
                                                      .body .columns th.small-2,
                                                      .body .column th.small-2,
                                                      th.small-2 center {
                                                        display: inline-block !important;
                                                        width: 16.66667% !important;
                                                      }
                                                      .body .columns td.small-3,
                                                      .body .column td.small-3,
                                                      td.small-3 center,
                                                      .body .columns th.small-3,
                                                      .body .column th.small-3,
                                                      th.small-3 center {
                                                        display: inline-block !important;
                                                        width: 25% !important;
                                                      }
                                                      .body .columns td.small-4,
                                                      .body .column td.small-4,
                                                      td.small-4 center,
                                                      .body .columns th.small-4,
                                                      .body .column th.small-4,
                                                      th.small-4 center {
                                                        display: inline-block !important;
                                                        width: 33.33333% !important;
                                                      }
                                                      .body .columns td.small-5,
                                                      .body .column td.small-5,
                                                      td.small-5 center,
                                                      .body .columns th.small-5,
                                                      .body .column th.small-5,
                                                      th.small-5 center {
                                                        display: inline-block !important;
                                                        width: 41.66667% !important;
                                                      }
                                                      .body .columns td.small-6,
                                                      .body .column td.small-6,
                                                      td.small-6 center,
                                                      .body .columns th.small-6,
                                                      .body .column th.small-6,
                                                      th.small-6 center {
                                                        display: inline-block !important;
                                                        width: 50% !important;
                                                      }
                                                      .body .columns td.small-7,
                                                      .body .column td.small-7,
                                                      td.small-7 center,
                                                      .body .columns th.small-7,
                                                      .body .column th.small-7,
                                                      th.small-7 center {
                                                        display: inline-block !important;
                                                        width: 58.33333% !important;
                                                      }
                                                      .body .columns td.small-8,
                                                      .body .column td.small-8,
                                                      td.small-8 center,
                                                      .body .columns th.small-8,
                                                      .body .column th.small-8,
                                                      th.small-8 center {
                                                        display: inline-block !important;
                                                        width: 66.66667% !important;
                                                      }
                                                      .body .columns td.small-9,
                                                      .body .column td.small-9,
                                                      td.small-9 center,
                                                      .body .columns th.small-9,
                                                      .body .column th.small-9,
                                                      th.small-9 center {
                                                        display: inline-block !important;
                                                        width: 75% !important;
                                                      }
                                                      .body .columns td.small-10,
                                                      .body .column td.small-10,
                                                      td.small-10 center,
                                                      .body .columns th.small-10,
                                                      .body .column th.small-10,
                                                      th.small-10 center {
                                                        display: inline-block !important;
                                                        width: 83.33333% !important;
                                                      }
                                                      .body .columns td.small-11,
                                                      .body .column td.small-11,
                                                      td.small-11 center,
                                                      .body .columns th.small-11,
                                                      .body .column th.small-11,
                                                      th.small-11 center {
                                                        display: inline-block !important;
                                                        width: 91.66667% !important;
                                                      }
                                                      table.body td.small-offset-1,
                                                      table.body th.small-offset-1 {
                                                        margin-left: 8.33333% !important;
                                                        margin-left: 8.33333% !important;
                                                      }
                                                      table.body td.small-offset-2,
                                                      table.body th.small-offset-2 {
                                                        margin-left: 16.66667% !important;
                                                        margin-left: 16.66667% !important;
                                                      }
                                                      table.body td.small-offset-3,
                                                      table.body th.small-offset-3 {
                                                        margin-left: 25% !important;
                                                        margin-left: 25% !important;
                                                      }
                                                      table.body td.small-offset-4,
                                                      table.body th.small-offset-4 {
                                                        margin-left: 33.33333% !important;
                                                        margin-left: 33.33333% !important;
                                                      }
                                                      table.body td.small-offset-5,
                                                      table.body th.small-offset-5 {
                                                        margin-left: 41.66667% !important;
                                                        margin-left: 41.66667% !important;
                                                      }
                                                      table.body td.small-offset-6,
                                                      table.body th.small-offset-6 {
                                                        margin-left: 50% !important;
                                                        margin-left: 50% !important;
                                                      }
                                                      table.body td.small-offset-7,
                                                      table.body th.small-offset-7 {
                                                        margin-left: 58.33333% !important;
                                                        margin-left: 58.33333% !important;
                                                      }
                                                      table.body td.small-offset-8,
                                                      table.body th.small-offset-8 {
                                                        margin-left: 66.66667% !important;
                                                        margin-left: 66.66667% !important;
                                                      }
                                                      table.body td.small-offset-9,
                                                      table.body th.small-offset-9 {
                                                        margin-left: 75% !important;
                                                        margin-left: 75% !important;
                                                      }
                                                      table.body td.small-offset-10,
                                                      table.body th.small-offset-10 {
                                                        margin-left: 83.33333% !important;
                                                        margin-left: 83.33333% !important;
                                                      }
                                                      table.body td.small-offset-11,
                                                      table.body th.small-offset-11 {
                                                        margin-left: 91.66667% !important;
                                                        margin-left: 91.66667% !important;
                                                      }
                                                      table.body table.columns td.expander,
                                                      table.body table.columns th.expander {
                                                        display: none !important;
                                                      }
                                                      table.body .right-text-pad,
                                                      table.body .text-pad-right {
                                                        padding-left: 10px !important;
                                                      }
                                                      table.body .left-text-pad,
                                                      table.body .text-pad-left {
                                                        padding-right: 10px !important;
                                                      }
                                                      table.menu {
                                                        width: 100% !important;
                                                      }
                                                      table.menu td,
                                                      table.menu th {
                                                        width: auto !important;
                                                        display: inline-block !important;
                                                      }
                                                      table.menu.vertical td,
                                                      table.menu.vertical th,
                                                      table.menu.small-vertical td,
                                                      table.menu.small-vertical th {
                                                        display: block !important;
                                                      }
                                                      table.menu[align=""center""] {
                                                        width: auto !important;
                                                      }
                                                      table.button.expand {
                                                        width: 100% !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      .calendar-content {
                                                        padding: 0px !important;
                                                        width: 288px !important;
                                                      }
                                                      .not-available-day,
                                                      .calendar-today,
                                                      .available-day {
                                                        height: 40px !important;
                                                        width: 40px !important;
                                                      }
                                                      .day-label {
                                                        margin-left: 10% !important;
                                                        margin-top: 0% !important;
                                                        font-size: 15px;
                                                      }
	                                                  .p
	                                                  {
	                                                  font-size:16px
	                                                  font-size:4vw
	                                                  }
                                                    }
                                                  </style>";

                                string header = "<table class=\"body\" style=\"border-spacing:0;border-collapse:collapse;vertical-align:top;-webkit-hyphens:none;-moz-hyphens:none;hyphens:none;-ms-hyphens:none;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;margin:0;text-align:left;font-size:16px;line-height:19px;background:#f3f3f3;padding:0;width:100%;height:100%;color:#0a0a0a;margin-bottom:0px !important;background-color: white\">" +
                                                "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<td class=\"center\" align=\"center\" valign=\"top\" style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                                "<center style=\"width:100%;min-width:580px\">" +
                                                "<table class=\"container\" style=\"border-spacing:0;border-collapse:collapse;padding:0;vertical-align:top;background:#fefefe;width:580px;margin:0 auto;text-align:inherit;max-width:580px;\">" +
                                                "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<td style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                                "<div style=\"padding-top:10px\">" +
                                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<th style=\"width: 40%;>" +
                                                "<a href=\"https://www.hummingbirdindia.com\" target=\"_blank\" style=\"padding:0;margin:0;text-align:left;line-height:1.3;color:#2199e8;text-decoration:none\">" +
                                                "<img align=\"center\" alt=\"" + Imagealt1 + "\" class=\"center standard-header\" src=\"" + Imagelocation1 + "\" style=\"max-width: 120px\" >" +
                                                "</a></th><th style=\"font-size:16px;padding:20px 0 20px 0;line-height:28px\"> " +
                                                "<p>HB Confirmation #: " + ds.Tables[2].Rows[0][2].ToString() + "</p>" +
                                                "</th></tr></table></div><div>" +
                                                "<div class=\"headline-body\" style=\"padding-bottom:15px\">" +
                                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;width:564px;margin:0 auto;padding-left:16px;padding-right:16px;padding-bottom:0px !important\">" +
                                                "<p class=\"body  body-lg body-link-rausch light text-left   \" style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;line-height:1.4;font-weight:300;color:#484848;font-size:24px;hyphens:none;-ms-hyphens:none;-webkit-hyphens:none;-moz-hyphens:none;text-align:left;margin-bottom:0px !important;color:#0a0a0a;\">" + ds.Tables[2].Rows[0][1].ToString();

                                if (All.QReserveFlag == true)
                                {
                                    header += "<br /><span style=\"float: right;font-size:20px;\">Quick Reservation Confirmed</span>";
                                }
                                else
                                {
                                    header += "<br /><span style=\"float: right;font-size:20px;\">Reservation Confirmed</span>";
                                }

                                header += "</p></th></tr></table></div></div>";

                                string ChkInOutDate = "";
                                if (All.LTIAPIFlag == true)
                                {
                                    ChkInOutDate = "<div><div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                     "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                     "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                     "<p class=\"body-text-lg light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important'>‌" + All.CheckinDate + " ‌</p>" +
                                     "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>Check-in " + All.CheckinTime + "‌</p>" +
                                     "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                     "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                     "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                     "<p class=\"body-text-lg light text-right\" style='padding:0;margin:0;font-size:17px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important'>‌" + All.CheckoutDate + "</p>" +
                                     "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:16px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Check-out</p>" +
                                     "</th></tr></table></div><div>" +
                                     "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                     "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                     "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                     "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                     "</th></tr></table></div>";
                                }
                                else
                                {
                                    ChkInOutDate = "<div><div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                     "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                     "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                     "<p class=\"body-text-lg light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important'>‌" + ds.Tables[0].Rows[0][10].ToString() + " ‌</p>" +
                                     "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>Check-in " + ds.Tables[0].Rows[0][9].ToString() + "‌</p>" +
                                     "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                     "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                     "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                     "<p class=\"body-text-lg light text-right\" style='padding:0;margin:0;font-size:17px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important'>‌" + ds.Tables[0].Rows[0][11].ToString() + "</p>" +
                                     "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:16px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Check-out</p>" +
                                     "</th></tr></table></div><div>" +
                                     "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                     "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                     "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                     "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                     "</th></tr></table></div>";
                                }



                                string GuestTbl = "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"col-pad-left-2 col-pad-right-2\" style =\"padding:0;margin:0;padding-left:16px;padding-right:16px\">" +
                                                  "<div style=\"margin:0;-webkit-border-radius:8px;border-radius:8px;display:block;border-color:#d9242c;border-width:2px;border-style:dotted dashed;\">" +
                                                  "<p class=\"text-center\" style='font-weight:normal;padding:0;margin:0;text-align:center;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;font-size:24px;line-height:32px;margin-bottom:0px !important'>Guest Details</p>" +
                                                  "</div></th></tr></table></div>" +
                                                  "<div style=\"padding-top:8px;padding-bottom:8px;padding-left:16px;padding-right:16px;\">" +
                                                  "<table rules=\"rows\" style =\"border:#dbdbdb\">" +
                                                  "<tr><td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Guest Name </strong></td > " +
                                                  "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Room No / Occupancy </strong ></td > " +
                                                  "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Tariff / <br> Room / Day </strong ></td > " +
                                                  "</tr><tr></tr>";
                                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                {
                                    GuestTbl += "<tr style=\"font-style:normal;font-weight:normal;\" class=\"ng-scope\">" +
                                            "<td class=\"padd ng-binding\" style=\"vertical-align:middle;text-align:center\">" + ds.Tables[0].Rows[i][12].ToString() + ". " + ds.Tables[0].Rows[i][0].ToString() + " " + ds.Tables[0].Rows[i][13].ToString() + "</td>" +
                                            "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[0].Rows[i][6].ToString() + " / " + ds.Tables[0].Rows[i][7].ToString() + "</td>" +
                                            "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">INR " + ds.Tables[0].Rows[i][3].ToString() + "</td>" +
                                            "</tr>";
                                }
                                GuestTbl += "</table>";

                                string PayMode = "<div><div class=\"row-pad-bot-1\" style=\"padding-bottom:8px !important;padding-top:6px;\"></div>" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                                  "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:18px;margin-bottom:0px !important'>Tariff Payment: " + ds.Tables[0].Rows[0][4].ToString() + "</p>" +
                                                  "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                                  "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                                  "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                                  "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:18px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Service Payment: " + ds.Tables[0].Rows[0][5].ToString() + "</p>" +
                                                  "</th></tr></table></div></div><div>" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:3px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                  "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                  "</th></tr></table></div>";

                                string TariffDtls = "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                                    "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                    "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                                    "<p class=\"body-text-lg light color-rausch text-right\" style='padding:0;margin:0;word-break:normal;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;line-height:1.2;font-size:18px;text-align:center;color:#d9242c !important;margin-bottom:0px !important'>Guest Contacts</p>" +
                                                    "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>" +
                                                     MobileNo +
                                                    "</p></th></table></div><div>" +
                                                    "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                    "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                    "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                    "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                    "</th></tr></table></div>";


                                string PropertyDtls = "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table;\">" +
                                        "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                        "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                        "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:16px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-weight: bold;\">Property Name : " + ds.Tables[1].Rows[0][5].ToString() + " </p>" +
                                        "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:13px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;\">" + ds.Tables[1].Rows[0][0].ToString() + " </p>" +
                                        "</th></tr></table>" +
                                        "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                        "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                        "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                        "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                        "</th></tr></table></div>";

                                string Note = "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                            "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:14px;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;\"><strong>Note : </strong>" + SplNote + " </p>" +
                                            "</th></tr></table>" +
                                            "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                            "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                            "</th></tr></table></div>";

                                string ContactDtls = "<div style=\"padding-top:8px;padding-bottom:8px\" >" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' > Booked by</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][3].ToString() + " </ p >" +
                                            "</th>" +
                                            "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<hr>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<hr>" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' >Client Request #</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][12].ToString() + " </ p >" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<hr>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<hr>" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' >Issues / feedbacks</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + DeskNo + "<br>" + ds.Tables[10].Rows[0][1].ToString() + " </ p >" +
                                            "</th>" +
                                            "</tr></table></div>";



                                ContactDtls += "<div>" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                            "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                            "</th></tr></table></div><div style=\"padding-top:2px\">" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;text-align:left\"><th style=\"width: 60%;\">" +
                                            "<a href=\"" + link + "\" target=\"_blank\"><span style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:10px 0 10px 16px;margin:0;text-align:left;line-height:1.3;text-decoration:none;font-weight:300;color:#d9242c !important\">Security/Cancellation Policy</span></a>" +
                                            "</th><th style=\"font-size:10px;padding:10px 16px 10px 0;line-height:28px;text-align:right;\">" +
                                            "<p>Powered by Staysimplyfied.com</p>" +
                                            "</th></tr></table></div></tr></table></div>" +
                                            "</td></tr></table></center></td></tr></table>";

                                string EndData = "</body></html>";
                                MailContent = style + header + ChkInOutDate + GuestTbl + PayMode + TariffDtls + PropertyDtls + Note + ContactDtls + EndData;
                                message1.Body = MailContent;
                                message1.IsBodyHtml = true;


                            }
                        }
                    }
                    #endregion
                    #region
                    else
                    {

                        var ChCnt = 0;
                        var ChCntVal = "txt";

                        if (All.ResendFlag == true)
                        {
                            ChCnt = 1;
                            ChCntVal = "txt";
                        }
                        else
                        {
                            ChCnt = ds.Tables[3].Rows.Count;
                            ChCntVal = ds.Tables[3].Rows[0][4].ToString();
                        }

                        if (ChCntVal != "")
                        {
                            if (ds.Tables[10].Rows.Count > 0)
                            {
                                message1.From = new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][1].ToString(), "", System.Text.Encoding.UTF8);
                            }
                            else
                            {
                                message1.From = new System.Net.Mail.MailAddress("stay@hummingbirdindia.com", "", System.Text.Encoding.UTF8);
                            }
                            if (All.ResendFlag == true)
                            {
                                var Mail = All.PropertyGusetEmail.Split(',');
                                for (int i = 0; i < Mail.Length; i++)
                                {
                                    try
                                    {
                                        message1.To.Add(new System.Net.Mail.MailAddress(Mail[i].ToString()));
                                    }
                                    catch (Exception ex)
                                    {
                                        CreateLogFiles log = new CreateLogFiles();
                                        log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + Mail[i].ToString());
                                    }
                                }
                                if (All.UserEmail != "")
                                {
                                    try
                                    {
                                        message1.CC.Add(new System.Net.Mail.MailAddress(All.UserEmail));
                                    }
                                    catch (Exception ex)
                                    {
                                        CreateLogFiles log = new CreateLogFiles();
                                        log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + All.UserEmail);
                                    }
                                }


                                message1.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));

                            }
                            else
                            {

                                string PropertyMail = ds.Tables[3].Rows[0][4].ToString();
                                var PtyMail = PropertyMail.Split(',');
                                int cnt = PtyMail.Length;
                                for (int i = 0; i < cnt; i++)
                                {
                                    if (PtyMail[i].ToString() != "")
                                    {
                                        try
                                        {
                                            message1.To.Add(new System.Net.Mail.MailAddress(PtyMail[i].ToString()));
                                        }
                                        catch (Exception ex)
                                        {
                                            CreateLogFiles log = new CreateLogFiles();
                                            log.ErrorLog("=> Confirmation Email API => Room Level Confirmation Property Mail => To => BookingId => " + All.BookingId + " => Invalid Email => " + PtyMail[i].ToString());
                                        }
                                    }
                                }
                                ////for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                                ////{
                                ////    if (ds.Tables[3].Rows[i][2].ToString() != "")
                                ////    {
                                ////        try
                                ////        {
                                ////            message1.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[3].Rows[i][2].ToString()));
                                ////        }
                                ////        catch (Exception ex)
                                ////        {
                                ////            CreateLogFiles log = new CreateLogFiles();
                                ////            log.ErrorLog("=> Confirmation Email API => Room Level Confirmation Property Mail => Cc => BookingId => " + All.BookingId + " => Invalid Email => " + ds.Tables[3].Rows[i][2].ToString());
                                ////        }
                                ////    }
                                ////}
                                ////if (ds.Tables[2].Rows[0][4].ToString() != "")
                                ////{
                                ////    try
                                ////    {
                                ////        message1.Bcc.Add(new System.Net.Mail.MailAddress(ds.Tables[2].Rows[0][4].ToString()));
                                ////    }
                                ////    catch (Exception ex)
                                ////    {
                                ////        CreateLogFiles log = new CreateLogFiles();
                                ////        log.ErrorLog("=> Confirmation Email API => Room Level Confirmation Property Mail => Bcc => BookingId => " + All.BookingId + " => Invalid Email => " + ds.Tables[2].Rows[0][4].ToString());
                                ////    }
                                ////}

                                ////if (ds.Tables[10].Rows[0][1].ToString() != "stay@hummingbirdindia.com")
                                ////{
                                ////    message1.Bcc.Add(new System.Net.Mail.MailAddress("stay@hummingbirdindia.com"));
                                ////}
                                ////message1.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                            }
                            message1.Bcc.Add(new System.Net.Mail.MailAddress("anbu@warblerit.com"));

                            message1.Subject = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString();

                            string typeofpty1 = ds.Tables[4].Rows[0][8].ToString();
                            string Imagelocation1 = "";
                            string Imagealt1 = "";
                            if (typeofpty1 == "MGH")
                            {
                                Imagelocation1 = ds.Tables[6].Rows[0][4].ToString();
                                Imagealt1 = ds.Tables[6].Rows[0][5].ToString();
                                if (Imagelocation1 == "")
                                {
                                    Imagelocation1 = ds.Tables[4].Rows[0][10].ToString();
                                    Imagealt1 = ds.Tables[4].Rows[0][11].ToString();
                                }
                            }
                            else
                            {
                                Imagelocation1 = ds.Tables[4].Rows[0][10].ToString();
                                Imagealt1 = ds.Tables[4].Rows[0][11].ToString();
                            }

                            // Contact Email 
                            string DeskNo = "";
                            DeskNo = ds.Tables[2].Rows[0][14].ToString();

                            // Guest Contact No.
                            string MobileNo = ds.Tables[4].Rows[0][4].ToString();
                            if (MobileNo == "")
                            {
                                MobileNo = " - NA - ";
                            }

                            // Spl Note
                            string SplNote = ds.Tables[2].Rows[0][8].ToString();
                            if (SplNote == "")
                            {
                                SplNote = "- NA -";
                            }


                            // View in browser Link
                            string id = ds.Tables[2].Rows[0][12].ToString();
                            string link = "http://mybooking.hummingbirdindia.com/?redirect=BookingPropertyConfirmation&B=R&R=" + id;


                            string style = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
                                                <html xmlns=""http://www.w3.org/1999/xhtml"" style=""min-height:100%;background:#f3f3f3"">
                                                <head><meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8""></head>
                                                <body style=""font-size:16px;min-width:100%;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;-moz-box-sizing:border-box;-webkit-box-sizing:border-box;box-sizing:border-box;font-family:'Circular', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;margin:0;line-height:1.3;color:#0a0a0a;text-align:left;width:100% !important"">
                                                <div class=""preheader"" style=""mso-hide:all;visibility:hidden;opacity:0;color:transparent;font-size:0px;width:0;height:0;display:none !important""></div>
                                                <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
                                                <meta name=""viewport"" content=""width=device-width"">
                                                <link href=""https://fonts.googleapis.com/css?family=Cabin&display=swap"" rel=""stylesheet"">
                                                <style data-roadie-ignore data-immutable=""true"">
                                                    @media only screen and (max-width: 596px) {
                                                      .small-float-center {
                                                        margin: 0 auto !important;
                                                        float: none !important;
                                                        text-align: center !important;
                                                      }
                                                      .small-text-center {
                                                        text-align: center !important;
                                                      }
                                                      .small-text-left {
                                                        text-align: left !important;
                                                      }
                                                      .small-text-right {
                                                        text-align: right !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .hide-for-large {
                                                        display: block !important;
                                                        width: auto !important;
                                                        overflow: visible !important;
                                                        max-height: none !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .row.hide-for-large,
                                                      table.body table.container .row.hide-for-large {
                                                        display: table !important;
                                                        width: 100% !important;
                                                      }
                                                    }
    
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .show-for-large {
                                                        display: none !important;
                                                        width: 0;
                                                        mso-hide: all;
                                                        overflow: hidden;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body img {
                                                        width: auto !important;
                                                        height: auto !important;
                                                      }
                                                      table.body center {
                                                        min-width: 0 !important;
                                                      }
                                                      table.body .container {
                                                        width: 95% !important;
                                                      }
                                                      table.body .columns,
                                                      table.body .column {
                                                        height: auto !important;
                                                        -moz-box-sizing: border-box;
                                                        -webkit-box-sizing: border-box;
                                                        box-sizing: border-box;
                                                        padding-left: 16px !important;
                                                        padding-right: 16px !important;
                                                      }
                                                      table.body .columns .column,
                                                      table.body .columns .columns,
                                                      table.body .column .column,
                                                      table.body .column .columns {
                                                        padding-left: 0 !important;
                                                        padding-right: 0 !important;
                                                      }
                                                      table.body .collapse .columns,
                                                      table.body .collapse .column {
                                                        padding-left: 0 !important;
                                                        padding-right: 0 !important;
                                                      }
                                                      td.small-1,
                                                      th.small-1 {
                                                        display: inline-block !important;
                                                        width: 8.33333% !important;
                                                      }
                                                      td.small-2,
                                                      th.small-2 {
                                                        display: inline-block !important;
                                                        width: 16.66667% !important;
                                                      }
                                                      td.small-3,
                                                      th.small-3 {
                                                        display: inline-block !important;
                                                        width: 25% !important;
                                                      }
                                                      td.small-4,
                                                      th.small-4 {
                                                        display: inline-block !important;
                                                        width: 33.33333% !important;
                                                      }
                                                      td.small-5,
                                                      th.small-5 {
                                                        display: inline-block !important;
                                                        width: 41.66667% !important;
                                                      }
                                                      td.small-6,
                                                      th.small-6 {
                                                        display: inline-block !important;
                                                        width: 50% !important;
                                                      }
                                                      td.small-7,
                                                      th.small-7 {
                                                        display: inline-block !important;
                                                        width: 58.33333% !important;
                                                      }
                                                      td.small-8,
                                                      th.small-8 {
                                                        display: inline-block !important;
                                                        width: 66.66667% !important;
                                                      }
                                                      td.small-9,
                                                      th.small-9 {
                                                        display: inline-block !important;
                                                        width: 75% !important;
                                                      }
                                                      td.small-10,
                                                      th.small-10 {
                                                        display: inline-block !important;
                                                        width: 83.33333% !important;
                                                      }
                                                      td.small-11,
                                                      th.small-11 {
                                                        display: inline-block !important;
                                                        width: 91.66667% !important;
                                                      }
                                                      td.small-12,
                                                      th.small-12 {
                                                        display: inline-block !important;
                                                        width: 100% !important;
                                                      }
                                                      .columns td.small-12,
                                                      .column td.small-12,
                                                      .columns th.small-12,
                                                      .column th.small-12 {
                                                        display: block !important;
                                                        width: 100% !important;
                                                      }
                                                      .body .columns td.small-1,
                                                      .body .column td.small-1,
                                                      td.small-1 center,
                                                      .body .columns th.small-1,
                                                      .body .column th.small-1,
                                                      th.small-1 center {
                                                        display: inline-block !important;
                                                        width: 8.33333% !important;
                                                      }
                                                      .body .columns td.small-2,
                                                      .body .column td.small-2,
                                                      td.small-2 center,
                                                      .body .columns th.small-2,
                                                      .body .column th.small-2,
                                                      th.small-2 center {
                                                        display: inline-block !important;
                                                        width: 16.66667% !important;
                                                      }
                                                      .body .columns td.small-3,
                                                      .body .column td.small-3,
                                                      td.small-3 center,
                                                      .body .columns th.small-3,
                                                      .body .column th.small-3,
                                                      th.small-3 center {
                                                        display: inline-block !important;
                                                        width: 25% !important;
                                                      }
                                                      .body .columns td.small-4,
                                                      .body .column td.small-4,
                                                      td.small-4 center,
                                                      .body .columns th.small-4,
                                                      .body .column th.small-4,
                                                      th.small-4 center {
                                                        display: inline-block !important;
                                                        width: 33.33333% !important;
                                                      }
                                                      .body .columns td.small-5,
                                                      .body .column td.small-5,
                                                      td.small-5 center,
                                                      .body .columns th.small-5,
                                                      .body .column th.small-5,
                                                      th.small-5 center {
                                                        display: inline-block !important;
                                                        width: 41.66667% !important;
                                                      }
                                                      .body .columns td.small-6,
                                                      .body .column td.small-6,
                                                      td.small-6 center,
                                                      .body .columns th.small-6,
                                                      .body .column th.small-6,
                                                      th.small-6 center {
                                                        display: inline-block !important;
                                                        width: 50% !important;
                                                      }
                                                      .body .columns td.small-7,
                                                      .body .column td.small-7,
                                                      td.small-7 center,
                                                      .body .columns th.small-7,
                                                      .body .column th.small-7,
                                                      th.small-7 center {
                                                        display: inline-block !important;
                                                        width: 58.33333% !important;
                                                      }
                                                      .body .columns td.small-8,
                                                      .body .column td.small-8,
                                                      td.small-8 center,
                                                      .body .columns th.small-8,
                                                      .body .column th.small-8,
                                                      th.small-8 center {
                                                        display: inline-block !important;
                                                        width: 66.66667% !important;
                                                      }
                                                      .body .columns td.small-9,
                                                      .body .column td.small-9,
                                                      td.small-9 center,
                                                      .body .columns th.small-9,
                                                      .body .column th.small-9,
                                                      th.small-9 center {
                                                        display: inline-block !important;
                                                        width: 75% !important;
                                                      }
                                                      .body .columns td.small-10,
                                                      .body .column td.small-10,
                                                      td.small-10 center,
                                                      .body .columns th.small-10,
                                                      .body .column th.small-10,
                                                      th.small-10 center {
                                                        display: inline-block !important;
                                                        width: 83.33333% !important;
                                                      }
                                                      .body .columns td.small-11,
                                                      .body .column td.small-11,
                                                      td.small-11 center,
                                                      .body .columns th.small-11,
                                                      .body .column th.small-11,
                                                      th.small-11 center {
                                                        display: inline-block !important;
                                                        width: 91.66667% !important;
                                                      }
                                                      table.body td.small-offset-1,
                                                      table.body th.small-offset-1 {
                                                        margin-left: 8.33333% !important;
                                                        margin-left: 8.33333% !important;
                                                      }
                                                      table.body td.small-offset-2,
                                                      table.body th.small-offset-2 {
                                                        margin-left: 16.66667% !important;
                                                        margin-left: 16.66667% !important;
                                                      }
                                                      table.body td.small-offset-3,
                                                      table.body th.small-offset-3 {
                                                        margin-left: 25% !important;
                                                        margin-left: 25% !important;
                                                      }
                                                      table.body td.small-offset-4,
                                                      table.body th.small-offset-4 {
                                                        margin-left: 33.33333% !important;
                                                        margin-left: 33.33333% !important;
                                                      }
                                                      table.body td.small-offset-5,
                                                      table.body th.small-offset-5 {
                                                        margin-left: 41.66667% !important;
                                                        margin-left: 41.66667% !important;
                                                      }
                                                      table.body td.small-offset-6,
                                                      table.body th.small-offset-6 {
                                                        margin-left: 50% !important;
                                                        margin-left: 50% !important;
                                                      }
                                                      table.body td.small-offset-7,
                                                      table.body th.small-offset-7 {
                                                        margin-left: 58.33333% !important;
                                                        margin-left: 58.33333% !important;
                                                      }
                                                      table.body td.small-offset-8,
                                                      table.body th.small-offset-8 {
                                                        margin-left: 66.66667% !important;
                                                        margin-left: 66.66667% !important;
                                                      }
                                                      table.body td.small-offset-9,
                                                      table.body th.small-offset-9 {
                                                        margin-left: 75% !important;
                                                        margin-left: 75% !important;
                                                      }
                                                      table.body td.small-offset-10,
                                                      table.body th.small-offset-10 {
                                                        margin-left: 83.33333% !important;
                                                        margin-left: 83.33333% !important;
                                                      }
                                                      table.body td.small-offset-11,
                                                      table.body th.small-offset-11 {
                                                        margin-left: 91.66667% !important;
                                                        margin-left: 91.66667% !important;
                                                      }
                                                      table.body table.columns td.expander,
                                                      table.body table.columns th.expander {
                                                        display: none !important;
                                                      }
                                                      table.body .right-text-pad,
                                                      table.body .text-pad-right {
                                                        padding-left: 10px !important;
                                                      }
                                                      table.body .left-text-pad,
                                                      table.body .text-pad-left {
                                                        padding-right: 10px !important;
                                                      }
                                                      table.menu {
                                                        width: 100% !important;
                                                      }
                                                      table.menu td,
                                                      table.menu th {
                                                        width: auto !important;
                                                        display: inline-block !important;
                                                      }
                                                      table.menu.vertical td,
                                                      table.menu.vertical th,
                                                      table.menu.small-vertical td,
                                                      table.menu.small-vertical th {
                                                        display: block !important;
                                                      }
                                                      table.menu[align=""center""] {
                                                        width: auto !important;
                                                      }
                                                      table.button.expand {
                                                        width: 100% !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      .calendar-content {
                                                        padding: 0px !important;
                                                        width: 288px !important;
                                                      }
                                                      .not-available-day,
                                                      .calendar-today,
                                                      .available-day {
                                                        height: 40px !important;
                                                        width: 40px !important;
                                                      }
                                                      .day-label {
                                                        margin-left: 10% !important;
                                                        margin-top: 0% !important;
                                                        font-size: 15px;
                                                      }
	                                                  .p
	                                                  {
	                                                  font-size:16px
	                                                  font-size:4vw
	                                                  }
                                                    }
                                                  </style>";

                            string header = "<table class=\"body\" style=\"border-spacing:0;border-collapse:collapse;vertical-align:top;-webkit-hyphens:none;-moz-hyphens:none;hyphens:none;-ms-hyphens:none;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;margin:0;text-align:left;font-size:16px;line-height:19px;background:#f3f3f3;padding:0;width:100%;height:100%;color:#0a0a0a;margin-bottom:0px !important;background-color: white\">" +
                                            "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<td class=\"center\" align=\"center\" valign=\"top\" style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                            "<center style=\"width:100%;min-width:580px\">" +
                                            "<table class=\"container\" style=\"border-spacing:0;border-collapse:collapse;padding:0;vertical-align:top;background:#fefefe;width:580px;margin:0 auto;text-align:inherit;max-width:580px;\">" +
                                            "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<td style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                            "<div style=\"padding-top:10px\">" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th style=\"width: 40%;>" +
                                            "<a href=\"https://www.hummingbirdindia.com\" target=\"_blank\" style=\"padding:0;margin:0;text-align:left;line-height:1.3;color:#2199e8;text-decoration:none\">" +
                                            "<img align=\"center\" alt=\"" + Imagealt1 + "\" class=\"center standard-header\" src=\"" + Imagelocation1 + "\" style=\"max-width: 120px\" >" +
                                            "</a></th><th style=\"font-size:16px;padding:20px 0 20px 0;line-height:28px\"> " +
                                            "<p>HB Confirmation #: " + ds.Tables[2].Rows[0][2].ToString() + "</p>";

                            if (ds.Tables[4].Rows[0][19].ToString() != "")
                            {
                                header += "<p>Confirmed by: " + ds.Tables[4].Rows[0][19].ToString() + "</p>";
                            }



                            header += "</th></tr></table></div><div>" +
                                "<div class=\"headline-body\" style=\"padding-bottom:15px\">" +
                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;width:564px;margin:0 auto;padding-left:16px;padding-right:16px;padding-bottom:0px !important\">" +
                                "<p class=\"body  body-lg body-link-rausch light text-left   \" style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;line-height:1.4;font-weight:300;color:#484848;font-size:24px;hyphens:none;-ms-hyphens:none;-webkit-hyphens:none;-moz-hyphens:none;text-align:left;margin-bottom:0px !important;color:#0a0a0a;\">" + ds.Tables[2].Rows[0][1].ToString();

                            if (All.QReserveFlag == true)
                            {
                                header += "<br /><span style=\"float: right;font-size:20px;\">Quick Reservation Confirmed</span>";
                            }
                            else
                            {
                                header += "<br /><span style=\"float: right;font-size:20px;\">Reservation Confirmed</span>";
                            }



                            header += "</p>" +
                               "</th></tr></table></div></div>";

                            string ChkInOutDate = "";
                            if (All.LTIAPIFlag == true)
                            {

                                ChkInOutDate = "<div><div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                                  "<p class=\"body-text-lg light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important'>‌" + All.CheckinDate + " ‌</p>" +
                                                  "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>Check-in " + All.CheckinTime + "‌</p>" +
                                                  "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                                  "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                                  "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                                  "<p class=\"body-text-lg light text-right\" style='padding:0;margin:0;font-size:17px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important'>‌" + All.CheckoutDate + "</p>" +
                                                  "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:16px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Check-out</p>" +
                                                  "</th></tr></table></div><div>" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                  "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                  "</th></tr></table></div>";
                            }
                            else
                            {
                                ChkInOutDate = "<div><div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                                  "<p class=\"body-text-lg light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important'>‌" + ds.Tables[0].Rows[0][9].ToString() + " ‌</p>" +
                                                  "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>Check-in " + ds.Tables[0].Rows[0][11].ToString() + "‌</p>" +
                                                  "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                                  "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                                  "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                                  "<p class=\"body-text-lg light text-right\" style='padding:0;margin:0;font-size:17px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important'>‌" + ds.Tables[0].Rows[0][10].ToString() + "</p>" +
                                                  "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:16px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Check-out</p>" +
                                                  "</th></tr></table></div><div>" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                  "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                  "</th></tr></table></div>";
                            }






                            string GuestTbl = "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                              "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                              "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                              "<th class=\"col-pad-left-2 col-pad-right-2\" style =\"padding:0;margin:0;padding-left:16px;padding-right:16px\">" +
                                              "<div style=\"margin:0;-webkit-border-radius:8px;border-radius:8px;display:block;border-color:#d9242c;border-width:2px;border-style:dotted dashed;\">" +
                                              "<p class=\"text-center\" style='font-weight:normal;padding:0;margin:0;text-align:center;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;font-size:24px;line-height:32px;margin-bottom:0px !important'>Guest Details</p>" +
                                              "</div></th></tr></table></div>" +
                                              "<div style=\"padding-top:8px;padding-bottom:8px;padding-left:16px;padding-right:16px;\">" +
                                              "<table rules=\"rows\" style =\"border:#dbdbdb\">" +
                                              "<tr><td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Guest Name </strong></td > " +
                                              "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Room No / Occupancy </strong ></td > " +
                                              "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Tariff / <br> Room / Day </strong ></td > " +
                                              "</tr><tr></tr>";
                            for (int i = 0; i < ds.Tables[11].Rows.Count; i++)
                            {

                                if ((typeofpty1 == "MGH") || (typeofpty1 == "DdP"))
                                {
                                    GuestTbl += "<tr style=\"font-style:normal;font-weight:normal;\" class=\"ng-scope\">" +
                                                "<td class=\"padd ng-binding\" style=\"vertical-align:middle;text-align:center\">" + ds.Tables[11].Rows[i][11].ToString() + "</td>" +
                                                "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[11].Rows[i][10].ToString() + " / " + ds.Tables[11].Rows[i][4].ToString() + "</td>" +
                                                "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">INR " + ds.Tables[11].Rows[i][3].ToString() + "</td>" +
                                                "</tr>";
                                }
                                else
                                {
                                    GuestTbl += "<tr style=\"font-style:normal;font-weight:normal;\" class=\"ng-scope\">" +
                                     "<td class=\"padd ng-binding\" style=\"vertical-align:middle;text-align:center\">" + ds.Tables[11].Rows[i][11].ToString() + "</td>" +
                                     "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[4].Rows[0][12].ToString() + " / " + ds.Tables[11].Rows[i][4].ToString() + "</td>" +
                                     "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">INR " + ds.Tables[11].Rows[i][3].ToString() + "</td>" +
                                     "</tr>";
                                }



                            }
                            GuestTbl += "</table>";

                            string PayMode = "<div><div class=\"row-pad-bot-1\" style=\"padding-bottom:8px !important;padding-top:6px;\"></div>" +
                                              "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                              "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                              "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                              "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:18px;margin-bottom:0px !important'>Tariff Payment: " + ds.Tables[11].Rows[0][5].ToString() + "</p>" +
                                              "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                              "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                              "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                              "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:18px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Service Payment: " + ds.Tables[11].Rows[0][6].ToString() + "</p>" +
                                              "</th></tr></table></div></div><div>" +
                                              "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                              "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                              "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:3px;width:564px;padding-left:16px;padding-right:16px\">" +
                                              "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                              "</th></tr></table></div>";

                            string TariffDtls = "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">";
                            string Stng = ds.Tables[11].Rows[0][8].ToString();
                            if (ds.Tables[11].Rows[0][7].ToString() == "NOTBTC")
                            {
                                if (Stng != "")
                                {
                                    TariffDtls +=
                                                "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px\">" +
                                                "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0;text-align:center;font-size:18px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#d9242c !important;;word-break:normal;line-height:1.2;\">Agreed Tariff</p>" +
                                    "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>" + Stng + "</p>" +

                                                                                                        "</th>";
                                }


                            }
                            else
                            {
                                try
                                {
                                    string file = ds.Tables[4].Rows[0][1].ToString();
                                    System.Net.Mail.Attachment att = new System.Net.Mail.Attachment(file);
                                    att.Name = ds.Tables[4].Rows[0][2].ToString();
                                    message1.Attachments.Add(att);
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog(" => Room Level Property Mail => BookingId => " + All.BookingId + " => PDF Attachment => Err Msg => " + ex.Message);
                                }
                                TariffDtls += "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px\">" +
                                              "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0;text-align:center;font-size:18px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#d9242c !important;;word-break:normal;line-height:1.2;\">Agreed Tariff</p>" +
                                              "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>" + Stng + "</p>" +
                                              "</th>";
                            }
                            TariffDtls += "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                                "<p class=\"body-text-lg light color-rausch text-right\" style='padding:0;margin:0;word-break:normal;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;line-height:1.2;font-size:18px;text-align:center;color:#d9242c !important;margin-bottom:0px !important'>Guest Contacts</p>" +
                                                "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>" +
                                                 MobileNo +
                                                "</p></th></table></div><div>" +
                                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                "</th></tr></table></div>";


                            string PropertyDtls = "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table;\">" +
                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                  "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                  "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:16px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-weight: bold;\">Property Name : " + ds.Tables[1].Rows[0][5].ToString() + " </p>" +
                                   "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:13px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;\">" + ds.Tables[1].Rows[0][0].ToString() + " </p>" +
                                  "</th></tr></table>" +
                                  "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                  "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                  "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                  "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                  "</th></tr></table></div>";

                            string Note = "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                         "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                         "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                                         "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:14px;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;\"><strong>Note : </strong>" + SplNote + " </p>" +
                                                         "</th></tr></table>" +
                                                         "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                         "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                         "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                         "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                         "</th></tr></table></div>";



                            string ContactDtls = "<div style=\"padding-top:8px;padding-bottom:8px\" >" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' > Booked by</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][3].ToString() + " </ p >" +
                                            "</th>" +
                                            "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<hr>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<hr>" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' >Client Request #</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][13].ToString() + " </ p >" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<hr>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<hr>" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' >Issues / feedbacks</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + DeskNo + "<br>" + ds.Tables[10].Rows[0][0].ToString() + " </ p >" +
                                            "</th>" +
                                            "</tr></table></div>";

                            ContactDtls += "<div>" +
                                                 "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                 "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                 "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                 "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                 "</th></tr></table></div><div style=\"padding-top:2px\">" +
                                                 "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;padding:0;width:100%;position:relative;display:table\">" +
                                                 "<tr class=\"\" style=\"padding:0;text-align:left\"><th style=\"width: 60%;\">" +
                                                 "<a href=\"" + link + "\" target=\"_blank\"><span style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:10px 0 10px 16px;margin:0;text-align:left;line-height:1.3;text-decoration:none;font-weight:300;color:#d9242c !important\">Security/Cancellation Policy</span></a>" +
                                                 "</th><th style=\"font-size:10px;padding:10px 16px 10px 0;line-height:28px;text-align:right;\">" +
                                                 "<p>Powered by Staysimplyfied.com</p>" +
                                                 "</th></tr></table></div></tr></table></div>" +
                                                 "</td></tr></table></center></td></tr></table>";

                            string EndData = "</body></html>";
                            MailContent = style + header + ChkInOutDate + GuestTbl + PayMode + TariffDtls + PropertyDtls + Note + ContactDtls + EndData;
                            message1.Body = MailContent;
                            message1.IsBodyHtml = true;
                        }

                    }
                    #endregion

                    try
                    {
                        smtp1.Send(message1);
                        Response2 = "Success";
                    }
                    catch (Exception ex)
                    {
                        CreateLogFiles log = new CreateLogFiles();
                        log.ErrorLog("=> Property Confirmation Mail => smtp => BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);
                        Response2 = "Failure";
                    }


                }
                else
                {
                    Response2 = "Success";
                }
                #endregion

                #region
                if (All.SmsChk == true)
                {
                    string PaymentMode = "";
                    string Maplink = "";
                    string WhatsappFileName = "Booking Confirmation -" + ds.Tables[2].Rows[0][2].ToString();
                    string paths123 = @"D:\home\site\wwwroot\Confirmations\";
                    var FileName = paths123 + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                    string WhatsappPdfUrl = AzureBlobPdfURl;
                    if (ds.Tables[0].Rows[0][8].ToString() == "Bed")
                    {
                        PaymentMode = ds.Tables[0].Rows[0][4].ToString();
                        Maplink = ds.Tables[1].Rows[0][13].ToString();

                    }
                    else
                    {
                        PaymentMode = ds.Tables[0].Rows[0][5].ToString();
                        Maplink = ds.Tables[1].Rows[0][13].ToString();
                    }

                    string FinalAPIUrl = "";
                    FinalAPIUrl = System.Configuration.ConfigurationManager.AppSettings["UrlShortner"] + "/API/UrlShortner/urlshort";
                    List<ConfirmationEMail> Msg = new List<ConfirmationEMail>();
                    try
                    {
                        SqlCommand command3 = new SqlCommand();
                        DataSet ds3 = new DataSet();
                        command3.CommandText = "SP_MMTBooking_Help";
                        command3.CommandType = CommandType.StoredProcedure;
                        command3.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "BookingConfirmedSMS";
                        command3.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = All.BookingId;
                        command3.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = "";
                        command3.Parameters.Add("@Str2", SqlDbType.NVarChar).Value = "";
                        command3.Parameters.Add("@Id1", SqlDbType.BigInt).Value = 0;
                        command3.Parameters.Add("@Id2", SqlDbType.BigInt).Value = 0;
                        ds3 = new DBconnection().ExecuteDataSet(command3, "");
                        var myData = ds3.Tables[0].AsEnumerable().Select(r => new ConfirmationEMail
                        {
                            CityCode = r.Field<string>("CityCode"),
                            Bookingcode = r.Field<string>("PropertyId"),
                            RowId = r.Field<string>("RatePlanCode"),
                            Caretaker = r.Field<long>("Caretaker"),
                            MobileNo = r.Field<string>("MobileNo"),
                            WhatsAppMsg = r.Field<string>("WhatsAppMsg")

                        });
                        Msg = myData.ToList();
                    }
                    catch (Exception ex)
                    {
                        CreateLogFiles log = new CreateLogFiles();
                        log.ErrorLog(" => Confirmation Email API => Booking Confirmation SMS => Get data from Procedure => BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);
                    }
                    string MapLink = "";
                    String FinalMap = "";
                    if (Maplink != "" && Maplink != null && Maplink != " ")
                    {
                        MapLink = "https://www.google.co.in/maps/place/" + Maplink;
                    }

                    try
                    {
                        if (Msg.Count > 0)
                        {
                            for (int i = 0; i < Msg.Count; i++)
                            {

                                //Firebase URL Start for Cancel
                                WebClient client = new WebClient();
                                client.Headers.Add("Content-Type", "application/json");
                                string LongPath = "http://mybooking.hummingbirdindia.com/" + "?B=" + Msg[i].Bookingcode + "$R=" + Msg[i].RowId;
                                string body = "{\"longUrl\":\"" + LongPath + "\"}";
                                try
                                {
                                    string ShortURl = client.UploadString(FinalAPIUrl, "POST", body);
                                    Msg[i].ShortPath = ShortURl;
                                    Msg[i].ShortPath = Msg[i].ShortPath.Replace("\"", "");
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog(" => Confirmation Email API => Booking Confirmation SMS => Cancel Link - BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);

                                }
                                //Firebase URL End For Cancel
                                //Firebase URL Start for Map
                                WebClient client1 = new WebClient();
                                client.Headers.Add("Content-Type", "application/json");
                                string LongPath1 = MapLink;
                                string body1 = "{\"longUrl\":\"" + LongPath1 + "\"}";
                                try
                                {
                                    string ShortURl1 = client.UploadString(FinalAPIUrl, "POST", body1);
                                    FinalMap = ShortURl1;
                                    FinalMap = FinalMap.Replace("\"", "");
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog(" => Confirmation Email API => Booking Confirmation SMS => Map Link - BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);

                                }
                                //Short URL End0

                                if (Msg[i].Caretaker == 0)
                                {
                                    if (PaymentMode == "Bill to Company (BTC)")
                                    {
                                        if (FinalMap != "")
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ".Map:" + " " + FinalMap + " .");
                                        }
                                    }
                                    else
                                    {
                                        if (FinalMap != "")
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ".Map:" + " " + FinalMap + ". To view you Booking / cancel your Booking,click the below link" + " " + Msg[i].ShortPath + " .");
                                        }
                                        else
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ". To view you Booking / cancel your Booking,click the below link" + " " + Msg[i].ShortPath + " .");
                                        }
                                    }
                                }
                                else
                                {
                                    if (PaymentMode == "Bill to Company (BTC)")
                                    {
                                        if (FinalMap != "")
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ".Map:" + " " + FinalMap + " .");
                                        }
                                    }
                                    else
                                    {
                                        if (FinalMap != "")
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ".Map:" + " " + FinalMap + " ."); //+ Msg[i].ShortPath removed by Pooranam
                                        }
                                    }
                                }
                                if (Msg[i].MobileNo != "" && All.GuestMailChk == true)
                                {
                                    try
                                    {
                                        WhatsappObj WhatsappData = new WhatsappObj();
                                        WhatsappData.MobileNo = Msg[i].MobileNo;
                                        WhatsappData.Msg = Msg[i].WhatsAppMsg;
                                        WhatsappData.WhatsappFileName = WhatsappFileName;
                                        WhatsappData.WhatsappPdfUrl = WhatsappPdfUrl;
                                        Task.Factory.StartNew(() => WhatsappAPI(WhatsappData));
                                    }
                                    catch (Exception Ex)
                                    {
                                        CreateLogFiles log = new CreateLogFiles();
                                        log.ErrorLog(" => Confirmation WhatsAPP API => Booking Confirmation WhatsApp => BookingId => " + All.BookingId + " => Err Msg => " + Ex.Message);

                                    }


                                }
                                WebRequest request = HttpWebRequest.Create(Msg[i].CityCode);
                                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                Stream s = (Stream)response.GetResponseStream();
                                StreamReader readStream = new StreamReader(s);
                                string dataString = readStream.ReadToEnd();
                                CreateLogFiles lognew = new CreateLogFiles();
                                lognew.ErrorLog("BookingId => " + All.BookingId + " => Status => " + dataString + " => Link " + Msg[i].CityCode);
                                response.Close();
                                s.Close();
                                readStream.Close();
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        CreateLogFiles log = new CreateLogFiles();
                        log.ErrorLog(" => Confirmation Email API => Booking Confirmation SMS => BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);

                    }
                }
                #endregion

                if (Response1 == "Success" && Response2 == "Failure")
                {
                    Response = "Confirmation Email not Sent to Property.";
                }
                else if (Response1 == "Failure" && Response2 == "Success")
                {
                    Response = "Confirmation Email not Sent to Guest.";
                }
                else if (Response1 == "Failure" && Response2 == "Failure")
                {
                    Response = "Confirmation Email not Sent to Guest & Property.";
                }
                else
                {
                    Response = "Confirmation Email Sent Successfully";
                }

                return Json(new { Code = "200", EmailResponse = Response });
            }
            catch (Exception Ex)
            {
                log = new CreateLogFiles();
                log.ErrorLog(" => Confirmation Email API => BookingId => " + All.BookingId + "=>" + Ex.Message);
                return Json(new { Code = "400", EmailResponse = "Confirmation Email not Sent - " + Ex.Message });
            }
        }

  

        [HttpPost]
        [Route("ConfirmChatEMail")]
        public IHttpActionResult ConfirmChatEMail(ConfirmationEMail All)
        {
            try
            {
                string Response = "";
                string Response1 = "Failure";
                string Response2 = "Failure";
                string Newid = "";
                String AzureBlobPdfURl = "";
                SqlCommand command5 = new SqlCommand();
                DataSet ds5 = new DataSet();
                command5.CommandText = "SP_SMTPMailSetting_Help";
                command5.CommandType = CommandType.StoredProcedure;
                command5.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "SMTP";
                command5.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = "";
                command5.Parameters.Add("@Id", SqlDbType.BigInt).Value = 0;
                ds5 = new DBconnection().ExecuteDataSet(command5, "");
                string Host = ds5.Tables[0].Rows[0][0].ToString();
                string CredentialsUserName = ds5.Tables[0].Rows[0][1].ToString();
                string CredentialsPassword = ds5.Tables[0].Rows[0][2].ToString();
                int Port = Convert.ToInt16(ds5.Tables[0].Rows[0][3]);


                SqlCommand command = new SqlCommand();
                DataSet ds = new DataSet();
                command.CommandText = "SP_ConfirmationEMail_Help";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@Str", SqlDbType.NVarChar).Value = "";
                command.Parameters.Add("@Id", SqlDbType.BigInt).Value = All.BookingId;
                ds = new DBconnection().ExecuteDataSet(command, "");


                #region
                if (All.GuestMailChk == true)
                {
                    System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
                    smtp.Port = Port;
                    smtp.Host = Host; smtp.Credentials = new System.Net.NetworkCredential(CredentialsUserName, CredentialsPassword);
                    smtp.EnableSsl = true;
                    string MailContent = "";

                    #region
                    if (ds.Tables[0].Rows[0][8].ToString() == "Bed")
                    {
                        if (ds.Tables[10].Rows.Count > 0)
                        {
                            message.From = new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][0].ToString(), "", System.Text.Encoding.UTF8);
                        }
                        else
                        {
                            message.From = new System.Net.Mail.MailAddress("stay@hummingbirdindia.com", "", System.Text.Encoding.UTF8);
                        }

                        if (All.ResendFlag == true)
                        {
                            var Mail = All.PropertyGusetEmail.Split(',');
                            for (int i = 0; i < Mail.Length; i++)
                            {
                                try
                                {
                                    message.To.Add(new System.Net.Mail.MailAddress(Mail[i].ToString()));
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + Mail[i].ToString());
                                }
                            }
                            if (All.UserEmail != "")
                            {
                                try
                                {
                                    message.CC.Add(new System.Net.Mail.MailAddress(All.UserEmail));
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + All.UserEmail);
                                }
                            }
                            message.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                        }
                        else
                        {
                            if (ds.Tables[4].Rows[0][0].ToString() == "0")
                            {
                                if (ds.Tables[8].Rows[0][0].ToString() != "")
                                {
                                    message.To.Add(new System.Net.Mail.MailAddress(ds.Tables[8].Rows[0][0].ToString()));
                                }
                            }
                            else
                            {
                                for (int i = 0; i < ds.Tables[5].Rows.Count; i++)
                                {
                                    if (i <= 40)
                                    {
                                        if (ds.Tables[5].Rows[i][0].ToString() != "")
                                        {
                                            try
                                            {
                                                message.To.Add(new System.Net.Mail.MailAddress(ds.Tables[5].Rows[i][0].ToString()));
                                            }
                                            catch (Exception ex)
                                            {
                                                CreateLogFiles log = new CreateLogFiles();
                                                log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + ds.Tables[5].Rows[i][0].ToString());
                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                ////if (ds.Tables[8].Rows[0][0].ToString() != "")
                                ////{
                                ////    try
                                ////    {
                                ////        message.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[8].Rows[0][0].ToString()));
                                ////    }
                                ////    catch (Exception ex)
                                ////    {
                                ////        CreateLogFiles log = new CreateLogFiles();
                                ////        log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => CC =>" + ds.Tables[8].Rows[0][0].ToString());
                                ////    }
                                ////}
                            }
                            //////Extra CC
                            ////for (int i = 0; i < ds.Tables[7].Rows.Count; i++)
                            ////{
                            ////    if (ds.Tables[7].Rows[i][0].ToString() != "")
                            ////    {
                            ////        try
                            ////        {
                            ////            message.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[7].Rows[i][0].ToString()));
                            ////        }
                            ////        catch (Exception ex)
                            ////        {
                            ////            CreateLogFiles log = new CreateLogFiles();
                            ////            log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => Extra CC =>" + ds.Tables[7].Rows[i][0].ToString());
                            ////        }
                            ////    }
                            ////}
                            ////// Extra CC email from Front end
                            ////if (ds.Tables[8].Rows[0][1].ToString() != "")
                            ////{
                            ////    string ExtraCC = ds.Tables[8].Rows[0][1].ToString();
                            ////    var ExtraCCEmail = ExtraCC.Split(',');
                            ////    int cnt = ExtraCCEmail.Length;
                            ////    for (int i = 0; i < cnt; i++)
                            ////    {
                            ////        if (ExtraCCEmail[i].ToString() != "")
                            ////        {
                            ////            try
                            ////            {
                            ////                message.CC.Add(new System.Net.Mail.MailAddress(ExtraCCEmail[i].ToString()));
                            ////            }
                            ////            catch (Exception ex)
                            ////            {
                            ////                CreateLogFiles log = new CreateLogFiles();
                            ////                log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => Extra CC From Front End =>" + ExtraCCEmail[i].ToString());
                            ////            }
                            ////        }
                            ////    }
                            ////}
                            ////if (ds.Tables[2].Rows[0][4].ToString() != "")
                            ////{
                            ////    try
                            ////    {
                            ////        message.Bcc.Add(new System.Net.Mail.MailAddress(ds.Tables[2].Rows[0][4].ToString()));
                            ////    }
                            ////    catch (Exception ex)
                            ////    {
                            ////        CreateLogFiles log = new CreateLogFiles();
                            ////        log.ErrorLog("=> Confirmation Email API => Bed Email => BookingId => " + All.BookingId + " => Invaild Email => BCc =>" + ds.Tables[2].Rows[0][4].ToString());
                            ////    }
                            ////}
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress("booking_confirmation@staysimplyfied.com"));
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress("bookingbcc@staysimplyfied.com"));
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                        }
                        message.Bcc.Add(new System.Net.Mail.MailAddress("anbu@warblerit.com"));

                        message.Subject = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString();

                        // Client Logo
                        string Imagelocation = "";
                        string Imagealt = "";
                        string PtyType = ds.Tables[5].Rows[0][1].ToString();
                        if (PtyType == "MGH")
                        {
                            Imagelocation = ds.Tables[6].Rows[0][4].ToString();
                            Imagealt = ds.Tables[6].Rows[0][5].ToString();
                            if (Imagelocation == "")
                            {
                                Imagelocation = ds.Tables[6].Rows[0][0].ToString();
                                Imagealt = ds.Tables[6].Rows[0][1].ToString();
                            }
                        }
                        else
                        {
                            Imagelocation = ds.Tables[6].Rows[0][0].ToString();
                            Imagealt = ds.Tables[6].Rows[0][1].ToString();
                        }

                        // Contact Email And Phone
                        string ContactEmail = "";
                        string DeskNo = "";
                        DeskNo = ds.Tables[2].Rows[0][13].ToString();
                        ContactEmail = ds.Tables[2].Rows[0][14].ToString();

                        // Map Link
                        string MapLink = "";
                        if (ds.Tables[1].Rows[0][13].ToString() != "")
                        {
                            MapLink = "https://www.google.co.in/maps/place/" + ds.Tables[1].Rows[0][13].ToString();
                        }
                        else
                        {
                            MapLink = "#";
                        }

                        // View in browser Link
                        string id = ds.Tables[2].Rows[0][11].ToString();
                        string link = "http://mybooking.hummingbirdindia.com/?redirect=BookingConfirmation&B=B&R=" + id;

                        // Spl Note
                        string SplNote = ds.Tables[2].Rows[0][8].ToString();
                        if (SplNote == "")
                        {
                            SplNote = "- NA -";
                        }

                        string header = "";

                        if (ds.Tables[2].Rows[0][15].ToString() != "")
                        {
                            header = "<div style =\"background - color:#f9fafc\" >" +
                            "<div style =\"background-color:#ffffff;width:800px;margin:0 auto\" >" +
                            "<div style =\"padding:10px 40px\">" +
                            "<div style =\"width:60%;display:inline-block\">" +
                            "<div style =\"border-radius:20px;padding:10px 10px;\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:25px;padding:10px 0\">Confirmation Voucher</h3>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > Hummingbird Booking Code - <b>" + ds.Tables[2].Rows[0][2].ToString() + " </b></p>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > Hotel Confirmation / Ref No -<b>" + ds.Tables[2].Rows[0][15].ToString() + "</b></p>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:13px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > <b>" + ds.Tables[2].Rows[0][7].ToString() + "</b></p>" +
                            "</div></div>" +
                            "<div style =\"width:39%;display:inline-block;text-align:right;vertical-align:text-bottom\"><img align =\"center\" alt=\"" + Imagealt + "\" class=\"center standard-header\" src=\"" + Imagelocation + "\" style=\"max-width: 200px\" ></a></div>" +
                            "</div></div>";
                        }
                        else
                        {
                            header = "<div style =\"background - color:#f9fafc\" >" +
                            "<div style =\"background-color:#ffffff;width:800px;margin:0 auto\" >" +
                            "<div style =\"padding:10px 40px\">" +
                            "<div style =\"width:60%;display:inline-block\">" +
                            "<div style =\"border-radius:20px;padding:10px 10px;\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:25px;padding:10px 0\">Confirmation Voucher</h3>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > Hummingbird Booking Code - <b>" + ds.Tables[2].Rows[0][2].ToString() + " </b></p>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:13px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > <b>" + ds.Tables[2].Rows[0][7].ToString() + "</b></p>" +
                            "</div></div>" +
                            "<div style =\"width:39%;display:inline-block;text-align:right;vertical-align:text-bottom\"><img align =\"center\" alt=\"" + Imagealt + "\" class=\"center standard-header\" src=\"" + Imagelocation + "\" style=\"max-width: 200px\" ></a></div>" +
                            "</div></div>";
                        }


                        string BookingDetails = "";

                        BookingDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                        "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\">Roy AI - Confirmation < u ></u></h3>" +
                        "</div>" +
                        "<table style =\"border-collapse:collapse\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Guest Name </strong></td>" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Room Type </strong></td >" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Check In </strong></td>" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Check Out </strong></td >" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Tariff (RS) </strong></td >" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Tariff Payment </strong></td>" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Services Payment </strong></td>" +
                        "</tr>";

                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            BookingDetails += "<tr style =\"font-style:normal;font-weight:normal;border-bottom:1px solid #ebebeb\">" +
                            "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][0].ToString() + " </td>" +
                            "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][7].ToString() + "/" + ds.Tables[0].Rows[i][4].ToString() + " </td>" +
                            "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][1].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][2].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][3].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][5].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][6].ToString() + "</td>" +
                            "</tr></tbody>";

                        }
                        BookingDetails += "</table>" +
                               "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                               "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u></u></h3>" +
                               "</div>" +
                                "<table style =\"border-collapse:collapse;width:100%;\">" +
                                "<tbody style=\"width:100%\">" +
                                "<tr><td style=\"width:50%;text-align:left; \">" +
                                "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;font-size:15px;padding:10px 0\"> Inclusions : <span style =\"font-weight:normal\">" + ds.Tables[1].Rows[0][12].ToString() + "</span></h3></td>" +
                                "<tr><td style=\"width:50%;text-align:center; \">" +
                                 "</td></tr>" +
                                "</tr></tbody></table>";

                        string HotelDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"> Hotel Information<u></u></h3>" +
                            "</div>" +
                            "<table style =\"border:#dbdbdb\"><tbody><tr>" +
                            "<td style =\"font-size:13px;width:14%\" valign = \"top\" align =\"center\"><strong></strong></td>" +
                            "<td style =\"font-size:13px;width:18%\" valign =\"top\" align =\"center\"><strong></strong></td>" +
                            "</tr><tr></tr>" +
                            "<tr style =\"font-style:normal;font-weight:normal\">" +
                            "<td style =\"vertical-align:middle;text-align:left\"><strong> Hotel Name:</strong>" + ds.Tables[1].Rows[0][5].ToString() + "<br/><strong> Address : </strong> " + ds.Tables[1].Rows[0][0].ToString() + "<b><br/> " + ds.Tables[1].Rows[0][1].ToString() + " </b> </ td >" +
                            "<td style =\"vertical-align:middle;text-align:center\" ><a href =" + MapLink + " target =\"_blank\" ><img src =\"https://portalvhds4prl9ymlwxnt8.blob.core.windows.net/img/Google_Maps_Icon.png\" ></a><a href = " + link + " target =\"_blank\"><span style =\"font-family:&#39;Cabin&#39;,Helvetica,Arial,sans-serif;padding:10px 0 10px 16px;margin:0;text-align:left;line-height:1.3;text-decoration:none;font-weight:300;color:#d9242c!important\"> Security / Cancellation Policy </span></a></td>" +
                            "</tr></tbody></table>";

                        string GSTDetails = "";

                        //string GSTDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                        //    "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"> GST Details<u></u></h3>" +
                        //    "</div>" +
                        //    "<table style =\"border-collapse:collapse\">" +
                        //    "<tbody>" +
                        //    "<tr style =\"border-bottom:1px solid #808080;width:100%;\">" +
                        //    "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong> GST Number </strong></td>" +
                        //    "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong> Legal Name </strong></td>" +
                        //    "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong> Address </strong></td>" +
                        //    "</tr>" +
                        //    "<tr style =\"font-style:normal;font-weight:normal;border-bottom:1px solid #ebebeb\">" +
                        //    "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[12].Rows[0][1].ToString() + "</td>" +
                        //    "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[12].Rows[0][0].ToString() + "</td>" +
                        //    "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[12].Rows[0][2].ToString() + "</td>" +
                        //    "</tr></tbody></table>";
                        string OtherDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u>HB Reference</u></h3>" +
                            "</div>" +
                            "<table style =\"border-collapse:collapse;\">" +
                            "<tbody>" +
                            "<tr style =\"border-bottom:1px solid #808080;width:100%;\">" +
                            "<td style =\"font-size:13px;width:33%\" valign =\"top\" align =\"center\"><strong> Client Ref No</strong></td>" +
                            "<td style =\"font-size:13px;width:33%\" valign =\"top\" align =\"center\"><strong> Booker </strong></td>" +
                            "<td style =\"font-size:13px;width:33%\" valign =\"top\" align =\"center\"><strong> Issues / Feedback </strong></td>" +
                            "</tr>" +
                            "<tr style =\"font-style:normal;font-weight:normal;border-bottom:1px solid #ebebeb\">" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[2].Rows[0][13].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[2].Rows[0][3].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[2].Rows[0][14].ToString() + " </td>" +
                            "</tr></tbody></table>" +
                            "<table><tbody><tr>" +
                            "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong></strong></td>" +
                            "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"right\" ><strong> Powered by <a href =\"http://hummingbirdindia.com\" target =\"_blank\">hummingbirdindia.com</a><u></u></strong></td>" +
                            "</tr></tbody></table></div></div>";

                        var PdfContent = "";

                        MailContent = header + BookingDetails + HotelDetails + OtherDetails;
                        PdfContent = header + BookingDetails + HotelDetails + OtherDetails;

                        var htmlContent = String.Format(PdfContent, DateTime.Now);
                        var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
                        var pdfBytes = htmlToPdf.GeneratePdf(htmlContent);
                        string path = @"D:\home\site\wwwroot\Confirmations\";

                        var BFilePathWhatsApp = "";

                        if (Directory.Exists(path))
                        {
                            var FileName = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            if (File.Exists(FileName))
                            {
                                var AttachmentName = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                                long ticks = DateTime.Now.Ticks;
                                byte[] bytes = BitConverter.GetBytes(ticks);
                                Newid = Convert.ToBase64String(bytes).Replace('+', '_').Replace('/', '-').TrimEnd('=');
                                File.WriteAllBytes(path + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                                System.Net.Mail.Attachment att1 = new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf");
                                att1.Name = AttachmentName;
                                message.Attachments.Add(att1);
                                BFilePathWhatsApp = path + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            }
                            else
                            {
                                File.WriteAllBytes(path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                                message.Attachments.Add(new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf"));
                                BFilePathWhatsApp = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            }
                        }
                        else
                        {
                            DirectoryInfo di = Directory.CreateDirectory(path);
                            File.WriteAllBytes(path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                            BFilePathWhatsApp = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                        }

                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        CloudConfigurationManager.GetSetting("StorageConnectionString"));
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer container = blobClient.GetContainerReference("bookingconfirmations");
                        var blob = container.GetBlockBlobReference("Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf");
                        try
                        {
                            using (var filestream = File.OpenRead(BFilePathWhatsApp))
                            {
                                blob.Properties.ContentType = "application/pdf";
                                blob.UploadFromStream(filestream);
                            }
                            //File.Delete(path);

                            AzureBlobPdfURl = blob.SnapshotQualifiedUri.AbsoluteUri;


                        }
                        catch (System.Exception e)
                        {
                            throw e;
                        }
                        message.Body = MailContent;
                        message.IsBodyHtml = true;


                    }
                    #endregion
                    #region
                    else
                    {
                        if (ds.Tables[10].Rows.Count > 0)
                        {
                            message.From = new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][0].ToString(), "", System.Text.Encoding.UTF8);
                        }
                        else
                        {
                            message.From = new System.Net.Mail.MailAddress("stay@hummingbirdindia.com", "", System.Text.Encoding.UTF8);
                        }
                        if (All.ResendFlag == true)
                        {
                            var Mail = All.PropertyGusetEmail.Split(',');
                            for (int i = 0; i < Mail.Length; i++)
                            {
                                try
                                {
                                    message.To.Add(new System.Net.Mail.MailAddress(Mail[i].ToString()));
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + Mail[i].ToString());
                                }
                            }
                            if (All.UserEmail != "")
                            {
                                try
                                {
                                    message.CC.Add(new System.Net.Mail.MailAddress(All.UserEmail));
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + All.UserEmail);
                                }
                            }
                            message.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                        }
                        else
                        {
                            if (ds.Tables[4].Rows[0][0].ToString() == "0")
                            {
                                if (ds.Tables[8].Rows[0][0].ToString() != "")
                                {
                                    message.To.Add(new System.Net.Mail.MailAddress(ds.Tables[8].Rows[0][0].ToString()));
                                }
                            }
                            else
                            {
                                for (int i = 0; i < ds.Tables[5].Rows.Count; i++)
                                {
                                    if (i <= 40)
                                    {
                                        if (ds.Tables[5].Rows[i][0].ToString() != "")
                                        {
                                            message.To.Add(new System.Net.Mail.MailAddress(ds.Tables[5].Rows[i][0].ToString()));
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                ////if (ds.Tables[8].Rows[0][0].ToString() != "")
                                ////{
                                ////    try
                                ////    {
                                ////        message.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[8].Rows[0][0].ToString()));
                                ////    }
                                ////    catch (Exception wer)
                                ////    {
                                ////        CreateLogFiles log = new CreateLogFiles();
                                ////        log.ErrorLog("=> Confirmation Email API => Room Email => Invalid Email => CC => " + ds.Tables[8].Rows[0][0].ToString() +
                                ////            " => BookingId => " + All.BookingId + ", Err Msg => " + wer.Message);
                                ////    }

                                ////}
                            }
                            //////Extra CC
                            ////for (int i = 0; i < ds.Tables[7].Rows.Count; i++)
                            ////{
                            ////    if (ds.Tables[7].Rows[i][0].ToString() != "")
                            ////    {
                            ////        try
                            ////        {
                            ////            message.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[7].Rows[i][0].ToString()));
                            ////        }
                            ////        catch (Exception wer)
                            ////        {
                            ////            CreateLogFiles log = new CreateLogFiles();
                            ////            log.ErrorLog("=> Confirmation Email API => Room Email => Invalid Email => Extra CC => " + ds.Tables[7].Rows[i][0].ToString() +
                            ////                " => BookingId => " + All.BookingId + ", Err Msg => " + wer.Message);
                            ////        }
                            ////    }
                            ////}
                            //////Extra CC email from Front end
                            ////if (ds.Tables[8].Rows[0][2].ToString() != "")
                            ////{
                            ////    string ExtraCC = ds.Tables[8].Rows[0][2].ToString();
                            ////    var ExtraCCEmail = ExtraCC.Split(',');
                            ////    int cnt = ExtraCCEmail.Length;
                            ////    for (int i = 0; i < cnt; i++)
                            ////    {
                            ////        if (ExtraCCEmail[i].ToString() != "")
                            ////        {
                            ////            try
                            ////            {
                            ////                message.CC.Add(new System.Net.Mail.MailAddress(ExtraCCEmail[i].ToString()));
                            ////            }
                            ////            catch (Exception wer)
                            ////            {
                            ////                CreateLogFiles log = new CreateLogFiles();
                            ////                log.ErrorLog("=> Confirmation Email API => Room Email => Invalid Email => Extra CC email from Front end => " + ExtraCCEmail[i].ToString() +
                            ////                    " => BookingId => " + All.BookingId + ", Err Msg => " + wer.Message);
                            ////            }
                            ////        }
                            ////    }
                            ////}
                            ////if (ds.Tables[2].Rows[0][4].ToString() != "")
                            ////{
                            ////    try
                            ////    {
                            ////        message.Bcc.Add(new System.Net.Mail.MailAddress(ds.Tables[2].Rows[0][4].ToString()));
                            ////    }
                            ////    catch (Exception wer)
                            ////    {
                            ////        CreateLogFiles log = new CreateLogFiles();
                            ////        log.ErrorLog("=> Confirmation Email API => Room Email => Invalid Email => Bcc => " + ds.Tables[2].Rows[0][4].ToString() +
                            ////            " => BookingId => " + All.BookingId + ", Err Msg => " + wer.Message);
                            ////    }
                            ////}
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][0].ToString()));
                            ////if (ds.Tables[10].Rows[0][0].ToString() != "stay@hummingbirdindia.com")
                            ////{
                            ////    message.Bcc.Add(new System.Net.Mail.MailAddress("stay@hummingbirdindia.com"));
                            ////}
                            ////message.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                        }
                        message.Bcc.Add(new System.Net.Mail.MailAddress("poorna@warblerit.com"));

                        message.Subject = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString();
                        string typeofpty = ds.Tables[4].Rows[0][8].ToString();
                        string Imagelocation = "";
                        string Imagealt = "";
                        if (typeofpty == "MGH")
                        {
                            Imagelocation = ds.Tables[6].Rows[0][4].ToString();
                            Imagealt = ds.Tables[6].Rows[0][5].ToString();
                            if (Imagelocation == "")
                            {
                                Imagelocation = ds.Tables[6].Rows[0][0].ToString();
                                Imagealt = ds.Tables[6].Rows[0][1].ToString();
                            }
                        }
                        else
                        {
                            Imagelocation = ds.Tables[6].Rows[0][0].ToString();
                            Imagealt = ds.Tables[6].Rows[0][1].ToString();
                        }

                        // Contact Email And Phone
                        string ContactEmail = "";
                        string DeskNo = "";
                        DeskNo = ds.Tables[2].Rows[0][14].ToString();
                        ContactEmail = ds.Tables[2].Rows[0][16].ToString();


                        // Map Link
                        string MapLink = "";
                        if (ds.Tables[1].Rows[0][13].ToString() != "")
                        {
                            MapLink = "https://www.google.co.in/maps/place/" + ds.Tables[1].Rows[0][13].ToString();
                        }
                        else
                        {
                            MapLink = "#";
                        }

                        // View in browser Link
                        string id = ds.Tables[2].Rows[0][12].ToString();
                        string link = "http://mybooking.hummingbirdindia.com/?redirect=BookingConfirmation&B=R&R=" + id;

                        // Spl Note
                        string SplNote = ds.Tables[2].Rows[0][8].ToString();
                        if (SplNote == "")
                        {
                            SplNote = "- NA -";
                        }

                        string BOKCreditcardView = "";
                        if (typeofpty == "BOK")
                        {
                            BOKCreditcardView = "<tr class=\"\" style=\"padding:0;text-align:left\">" +
                                            "<th style=\"width: 60%;\"><a style = \"font-size:13px; padding:10px 10px 10px 10px;\" align =\"right\" href=" + link + "&C=BOK#Creditcard" + ">UPDATE YOUR CREDIT CARD INFO</a>" +
                                            "</th><th style=\"font-size:10px;padding:10px 16px 10px 0;line-height:28px;text-align:right;\" >" +
                                            "</th></tsr>";

                        }
                        string header = "";

                        if (ds.Tables[2].Rows[0][15].ToString() != "")
                        {
                            header = "<div style =\"background - color:#f9fafc\" >" +
                            "<div style =\"background-color:#ffffff;width:800px;margin:0 auto\" >" +
                            "<div style =\"padding:10px 40px\">" +
                            "<div style =\"width:60%;display:inline-block\">" +
                            "<div style =\"border-radius:20px;padding:10px 10px;\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:25px;padding:10px 0\">Confirmation Voucher</h3>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > Hummingbird Booking Code - <b>" + ds.Tables[2].Rows[0][2].ToString() + " </b></p>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > Hotel Confirmation / Ref No -<b>" + ds.Tables[2].Rows[0][15].ToString() + "</b></p>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:13px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > <b>" + ds.Tables[2].Rows[0][7].ToString() + "</b></p>" +
                            "</div></div>" +
                            "<div style =\"width:39%;display:inline-block;text-align:right;vertical-align:text-bottom\"><img align =\"center\" alt=\"" + Imagealt + "\" class=\"center standard-header\" src=\"" + Imagelocation + "\" style=\"max-width: 200px\" ></a></div>" +
                            "</div></div>";
                        }
                        else
                        {
                            header = "<div style =\"background - color:#f9fafc\" >" +
                            "<div style =\"background-color:#ffffff;width:800px;margin:0 auto\" >" +
                            "<div style =\"padding:10px 40px\">" +
                            "<div style =\"width:60%;display:inline-block\">" +
                            "<div style =\"border-radius:20px;padding:10px 10px;\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:25px;padding:10px 0\">Confirmation Voucher</h3>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > Hummingbird Booking Code - <b>" + ds.Tables[2].Rows[0][2].ToString() + " </b></p>" +
                            "<p style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:13px;text-align:justify;line-height:125%;word-spacing:125%;padding:5px 0\" > <b>" + ds.Tables[2].Rows[0][7].ToString() + "</b></p>" +
                            "</div></div>" +
                            "<div style =\"width:39%;display:inline-block;text-align:right;vertical-align:text-bottom\"><img align =\"center\" alt=\"" + Imagealt + "\" class=\"center standard-header\" src=\"" + Imagelocation + "\" style=\"max-width: 200px\" ></a></div>" +
                            "</div></div>";
                        }


                        string BookingDetails = "";
                        if (All.LTIAPIFlag == true)
                        {
                            BookingDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                        "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u>Roy AI - Confirmation</u></h3>" +
                        "</div>" +
                        "<table style =\"border-collapse:collapse\">" +
                        "<tbody>" +
                        "<tr>" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Guest Name </strong></td>" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Room Type </strong></td >" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Check In </strong></td>" +
                        "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Check Out </strong></td >" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Tariff (RS) </strong></td >" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Tariff Payment </strong></td>" +
                        "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Services Payment </strong></td>" +
                        "</tr>";
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                BookingDetails += "<tr style =\"font-style:normal;font-weight:normal;\">" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;padding:5px 0;\"> " + ds.Tables[i].Rows[0][0].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;padding:5px 0;\"> " + ds.Tables[i].Rows[0][7].ToString() + "/" + ds.Tables[0].Rows[i][4].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;padding:5px 0;\"> " + ds.Tables[i].Rows[0][9].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;padding:5px 0;\"> " + ds.Tables[i].Rows[0][10].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;padding:5px 0;\"> " + ds.Tables[i].Rows[0][3].ToString() + " / -</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;padding:5px 0;\"> " + ds.Tables[i].Rows[0][5].ToString() + "</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;padding:5px 0;\"> " + ds.Tables[i].Rows[0][6].ToString() + "</td>" +
                                "</tr></tbody>";
                            }
                            BookingDetails += "</table>" +
                               "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                               "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u></u></h3>" +
                               "</div>" +
                                "<table style =\"border-collapse:collapse;width:100%;\">" +
                                "<tbody style=\"width:100%\">" +
                                "<tr><td style=\"width:50%;text-align:left; \">" +
                                "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;font-size:15px;padding:10px 0\"> Inclusions : <span style =\"font-weight:normal\">" + ds.Tables[1].Rows[0][12].ToString() + "</span></h3></td>" +
                                "<tr><td style=\"width:50%;text-align:center; \">" +
                                 "</td></tr>" +
                                "</tr></tbody></table>";
                        }
                        else
                        {
                            BookingDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\">Roy AI - Confirmation<u></u></h3>" +
                            "</div>" +
                            "<table style =\"border-collapse:collapse\">" +
                            "<tbody>" +
                            "<tr>" +
                            "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Guest Name </strong></td>" +
                            "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Room Type </strong></td >" +
                            "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Check In </strong></td>" +
                            "<td style =\"font-size:13px;width:12%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Check Out </strong></td >" +
                            "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Tariff </strong></td >" +
                            "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Tariff Payment </strong></td>" +
                            "<td style =\"font-size:13px;width:13%;border:1px solid #ebebeb;padding:5px 0;\" valign =\"top\" align =\"center\"><strong> Services Payment </strong></td>" +
                            "</tr>";
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                BookingDetails += "<tr style =\"font-style:normal;font-weight:normal;border-bottom:1px solid #ebebeb\">" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][0].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][7].ToString() + "/" + ds.Tables[0].Rows[i][4].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;borde:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][1].ToString() + " </td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][2].ToString() + "</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][3].ToString() + "</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][5].ToString() + "</td>" +
                                "<td style =\"vertical-align:middle;text-align:center;border:1px solid #ebebeb;\"> " + ds.Tables[0].Rows[i][6].ToString() + "</td>" +
                                "</tr></tbody>";
                            }
                            BookingDetails += "</table>" +
                               "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                               "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u></u></h3>" +
                               "</div>" +
                                "<table style =\"border-collapse:collapse;width:100%;\">" +
                                "<tbody style=\"width:100%\">" +
                                "<tr><td style=\"width:50%;text-align:left; \">" +
                                "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;font-size:15px;padding:10px 0\"> Inclusions : <span style =\"font-weight:normal\">" + ds.Tables[1].Rows[0][12].ToString() + "</span></h3></td>" +
                                "<tr><td style=\"width:50%;text-align:center; \">" +
                                 "</td></tr>" +
                                "</tr></tbody></table>";
                        }
                        string HotelDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u>Hotel Information</u></h3>" +
                            "</div>" +
                            "<table style =\"border:#dbdbdb\"><tbody><tr>" +
                            "<td style =\"font-size:13px;width:14%\" valign = \"top\" align =\"center\"><strong></strong></td>" +
                            "<td style =\"font-size:13px;width:18%\" valign =\"top\" align =\"center\"><strong></strong></td>" +
                            "</tr><tr></tr>" +
                            "<tr style =\"font-style:normal;font-weight:normal\">" +
                            "<td style =\"vertical-align:middle;text-align:left\"><strong> Hotel Name:</strong>" + ds.Tables[1].Rows[0][5].ToString() + "<br/><strong> Address : </strong> " + ds.Tables[1].Rows[0][0].ToString() + "<b><br/> " + ds.Tables[1].Rows[0][1].ToString() + " </b> </ td >" +
                            "<td style =\"vertical-align:middle;text-align:center\" ><a href =" + MapLink + " target =\"_blank\" ><img src =\"https://portalvhds4prl9ymlwxnt8.blob.core.windows.net/img/Google_Maps_Icon.png\" ></a><a href = " + link + " target =\"_blank\"><span style =\"font-family:&#39;Cabin&#39;,Helvetica,Arial,sans-serif;padding:10px 0 10px 16px;margin:0;text-align:left;line-height:1.3;text-decoration:none;font-weight:300;color:#d9242c!important\"> Security / Cancellation Policy </span></a></td>" +
                            "</tr></tbody></table>";

                        string GSTDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u>GST Reference</u></h3>" +
                            "</div>" +
                            "<table style =\"border-collapse:collapse;width:100%;\">" +
                            "<tbody>" +
                            "<tr style =\"border-bottom:1px solid #808080\">" +
                            "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong> GST Number </strong></td>" +
                            "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong> Legal Name </strong></td>" +
                            "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong> Address </strong></td>" +
                            "</tr>" +
                            "<tr style =\"font-style:normal;font-weight:normal;border-bottom:1px solid #ebebeb\">" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[12].Rows[0][1].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[12].Rows[0][0].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[12].Rows[0][2].ToString() + "</td>" +
                            "</tr></tbody></table>";
                        string OtherDetails = "<div style =\"border-bottom:2px solid #808080;margin:5px 0px 20px 0px\">" +
                            "<h3 style =\"margin:0;font-family:&#39;Open Sans&#39;;font-size:16px;padding:10px 0;text-align:center;font-weight:bold\"><u>HB Reference</u></h3>" +
                            "</div>" +
                            "<table style =\"border-collapse:collapse;width:100%;\">" +
                            "<tbody>" +
                            "<tr style =\"border-bottom:1px solid #808080;\">" +
                            "<td style =\"font-size:13px;width:33%\" valign =\"top\" align =\"center\"><strong> Client Ref No</strong></td>" +
                            "<td style =\"font-size:13px;width:33%\" valign =\"top\" align =\"center\"><strong> Booker </strong></td>" +
                            "<td style =\"font-size:13px;width:33%\" valign =\"top\" align =\"center\"><strong> Issues / Feedback </strong></td>" +
                            "</tr>" +
                            "<tr style =\"font-style:normal;font-weight:normal;border-bottom:1px solid #ebebeb\">" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[2].Rows[0][13].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[2].Rows[0][3].ToString() + "</td>" +
                            "<td style =\"vertical-align:middle;text-align:center\">" + ds.Tables[2].Rows[0][14].ToString() + " </td>" +
                            "</tr></tbody></table>" +
                            "<table><tbody><tr>" +
                            "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"center\"><strong></strong></td>" +
                            "<td style =\"font-size:13px;width:16%\" valign =\"top\" align =\"right\" ><strong> Powered by <a href =\"http://hummingbirdindia.com\" target =\"_blank\">hummingbirdindia.com</a><u></u></strong></td>" +
                            "</tr></tbody></table></div></div>";

                        var PdfContent = "";
                        if (ds.Tables[0].Rows[0][5].ToString() == "Direct<br>(Cash/Card)")
                        {
                            if(ds.Tables[2].Rows[0][11].ToString() == "1645")
                            {
                                MailContent = header + BookingDetails + HotelDetails  + OtherDetails;
                                PdfContent = header + BookingDetails + HotelDetails  + OtherDetails;
                            }
                            else
                            {
                                MailContent = header + BookingDetails + HotelDetails + GSTDetails + OtherDetails;
                                PdfContent = header + BookingDetails + HotelDetails + GSTDetails + OtherDetails;
                            }
                            
                        }
                        else
                        {
                            MailContent = header + BookingDetails + HotelDetails + OtherDetails;
                            PdfContent = header + BookingDetails + HotelDetails + OtherDetails;
                        }

                        var htmlContent = String.Format(PdfContent, DateTime.Now);
                        var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
                        var pdfBytes = htmlToPdf.GeneratePdf(htmlContent);
                        string path = @"D:\home\site\wwwroot\Confirmations\";

                        var RFilePathWhatsApp = "";
                        if (Directory.Exists(path))
                        {
                            var FileName = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            if (File.Exists(FileName))
                            {
                                var AttachmentName = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                                long ticks = DateTime.Now.Ticks;
                                byte[] bytes = BitConverter.GetBytes(ticks);
                                Newid = Convert.ToBase64String(bytes).Replace('+', '_').Replace('/', '-').TrimEnd('=');
                                File.WriteAllBytes(path + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                                System.Net.Mail.Attachment att1 = new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf");
                                att1.Name = AttachmentName;
                                RFilePathWhatsApp = path + "Booking Confirmation - " + Newid + " - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                                message.Attachments.Add(att1);

                            }
                            else
                            {
                                File.WriteAllBytes(path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                                message.Attachments.Add(new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf"));
                                RFilePathWhatsApp = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                            }
                        }
                        else
                        {
                            DirectoryInfo di = Directory.CreateDirectory(path);
                            File.WriteAllBytes(path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf", pdfBytes);
                            RFilePathWhatsApp = path + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                        }

                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        CloudConfigurationManager.GetSetting("StorageConnectionString"));
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer container = blobClient.GetContainerReference("bookingconfirmations");
                        var blob = container.GetBlockBlobReference("Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf");
                        try
                        {
                            using (var filestream = File.OpenRead(RFilePathWhatsApp))
                            {
                                blob.Properties.ContentType = "application/pdf";
                                blob.UploadFromStream(filestream);
                            }
                            //File.Delete(path);

                            AzureBlobPdfURl = blob.SnapshotQualifiedUri.AbsoluteUri;


                        }
                        catch (System.Exception e)
                        {
                            throw e;
                        }
                        message.Body = MailContent;
                        message.IsBodyHtml = true;
                        if (ds.Tables[2].Rows[0][11].ToString() == "218" && ds.Tables[0].Rows[0][5].ToString() == "Direct<br>(Cash/Card)")
                        {
                            message.Attachments.Add(new Attachment(@"D:\home\site\wwwroot\Confirmations\" + "icici_letter.pdf"));
                        }

                        //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        //CloudConfigurationManager.GetSetting("StorageConnectionString"));
                    }
                    #endregion

                    try
                    {
                        smtp.Send(message);
                        Response1 = "Success";
                    }
                    catch (Exception ex)
                    {
                        CreateLogFiles log = new CreateLogFiles();
                        log.ErrorLog("=> Guest Confirmation Mail => smtp => BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);
                        Response1 = "Failure";
                    }
                }
                else
                {
                    Response1 = "Success";
                }
                #endregion

                #region
                if (All.PropertyMailChk == true)
                {
                    System.Net.Mail.MailMessage message1 = new System.Net.Mail.MailMessage();
                    System.Net.Mail.SmtpClient smtp1 = new System.Net.Mail.SmtpClient();
                    smtp1.Port = Port;
                    smtp1.Host = Host;
                    smtp1.Credentials = new System.Net.NetworkCredential(CredentialsUserName, CredentialsPassword);
                    smtp1.EnableSsl = true;
                    string MailContent = "";

                    #region
                    if (ds.Tables[0].Rows[0][8].ToString() == "Bed")
                    {

                        var ChCnt = 0;
                        var ChCntVal = "txt";

                        if (All.ResendFlag == true)
                        {
                            ChCnt = 1;
                            ChCntVal = "txt";
                        }
                        else
                        {
                            ChCnt = ds.Tables[3].Rows.Count;
                            ChCntVal = ds.Tables[3].Rows[0][4].ToString();
                        }

                        if (ChCnt > 0)
                        {
                            if (ChCntVal != "")
                            {
                                string PropertyMail = ds.Tables[3].Rows[0][4].ToString();
                                var PtyMail = PropertyMail.Split(',');
                                int cnt = PtyMail.Length;

                                if (ds.Tables[10].Rows.Count > 0)
                                {
                                    message1.From = new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][1].ToString(), "", System.Text.Encoding.UTF8);
                                }
                                else
                                {
                                    message1.From = new System.Net.Mail.MailAddress("stay@hummingbirdindia.com", "", System.Text.Encoding.UTF8);
                                }

                                if (All.ResendFlag == true)
                                {
                                    var Mail = All.PropertyGusetEmail.Split(',');
                                    for (int i = 0; i < Mail.Length; i++)
                                    {
                                        try
                                        {
                                            message1.To.Add(new System.Net.Mail.MailAddress(Mail[i].ToString()));
                                        }
                                        catch (Exception ex)
                                        {
                                            CreateLogFiles log = new CreateLogFiles();
                                            log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + Mail[i].ToString());
                                        }
                                    }
                                    if (All.UserEmail != "")
                                    {
                                        try
                                        {
                                            message1.CC.Add(new System.Net.Mail.MailAddress(All.UserEmail));
                                        }
                                        catch (Exception ex)
                                        {
                                            CreateLogFiles log = new CreateLogFiles();
                                            log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + All.UserEmail);
                                        }
                                    }


                                    message1.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));

                                }
                                else
                                {

                                    for (int i = 0; i < cnt; i++)
                                    {
                                        if (PtyMail[i].ToString() != "")
                                        {
                                            try
                                            {
                                                message1.To.Add(new System.Net.Mail.MailAddress(PtyMail[i].ToString()));
                                            }
                                            catch (Exception ex)
                                            {
                                                CreateLogFiles log = new CreateLogFiles();
                                                log.ErrorLog("=> Confirmation Email API => Bed Property Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + PtyMail[i].ToString());
                                            }
                                        }
                                    }
                                    ////for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                                    ////{
                                    ////    if (ds.Tables[3].Rows[i][2].ToString() != "")
                                    ////    {
                                    ////        try
                                    ////        {
                                    ////            message1.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[3].Rows[i][2].ToString()));
                                    ////        }
                                    ////        catch (Exception ex)
                                    ////        {
                                    ////            CreateLogFiles log = new CreateLogFiles();
                                    ////            log.ErrorLog("=> Confirmation Email API => Bed Property Email => BookingId => " + All.BookingId + " => Invaild Email => Cc =>" + ds.Tables[3].Rows[i][2].ToString());
                                    ////        }
                                    ////    }
                                    ////}
                                    ////if (ds.Tables[2].Rows[0][4].ToString() != "")
                                    ////{
                                    ////    try
                                    ////    {
                                    ////        message1.Bcc.Add(ds.Tables[2].Rows[0][4].ToString());
                                    ////    }
                                    ////    catch (Exception ex)
                                    ////    {
                                    ////        CreateLogFiles log = new CreateLogFiles();
                                    ////        log.ErrorLog("=> Confirmation Email API => Bed Property Email => BookingId => " + All.BookingId + " => Invaild Email => Bcc =>" + ds.Tables[2].Rows[0][4].ToString());
                                    ////    }
                                    ////}
                                    ////message1.Bcc.Add(new System.Net.Mail.MailAddress("bookingbcc@staysimplyfied.com"));
                                    ////message1.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));

                                }
                                message1.Bcc.Add(new System.Net.Mail.MailAddress("anbu@warblerit.com"));

                                message1.Subject = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString();


                                string Imagelocation1 = "";
                                string Imagealt1 = "";
                                string PtyType1 = ds.Tables[5].Rows[0][1].ToString();
                                if (PtyType1 == "MGH")
                                {
                                    Imagelocation1 = ds.Tables[6].Rows[0][4].ToString();
                                    Imagealt1 = ds.Tables[6].Rows[0][5].ToString();
                                    if (Imagelocation1 == "")
                                    {
                                        Imagelocation1 = ds.Tables[6].Rows[0][0].ToString();
                                        Imagealt1 = ds.Tables[6].Rows[0][3].ToString();
                                    }
                                }
                                else
                                {
                                    Imagelocation1 = ds.Tables[6].Rows[0][0].ToString();
                                    Imagealt1 = ds.Tables[6].Rows[0][3].ToString();
                                }

                                // Desk No
                                string DeskNo = "";
                                DeskNo = ds.Tables[2].Rows[0][13].ToString();


                                // Guest Mobile No.
                                string MobileNo = ds.Tables[4].Rows[0][2].ToString();
                                if (MobileNo == "")
                                {
                                    MobileNo = " - NA - ";
                                }


                                // Spl Note
                                string SplNote = ds.Tables[2].Rows[0][8].ToString();
                                if (SplNote == "")
                                {
                                    SplNote = "- NA -";
                                }

                                // View in browser Link
                                string id = ds.Tables[2].Rows[0][11].ToString();
                                string link = "http://mybooking.hummingbirdindia.com/?redirect=BookingPropertyConfirmation&B=B&R=" + id;


                                string style = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
                                                <html xmlns=""http://www.w3.org/1999/xhtml"" style=""min-height:100%;background:#f3f3f3"">
                                                <head><meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8""></head>
                                                <body style=""font-size:16px;min-width:100%;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;-moz-box-sizing:border-box;-webkit-box-sizing:border-box;box-sizing:border-box;font-family:'Circular', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;margin:0;line-height:1.3;color:#0a0a0a;text-align:left;width:100% !important"">
                                                <div class=""preheader"" style=""mso-hide:all;visibility:hidden;opacity:0;color:transparent;font-size:0px;width:0;height:0;display:none !important""></div>
                                                <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
                                                <meta name=""viewport"" content=""width=device-width"">
                                                <link href=""https://fonts.googleapis.com/css?family=Cabin&display=swap"" rel=""stylesheet"">
                                                <style data-roadie-ignore data-immutable=""true"">
                                                    @media only screen and (max-width: 596px) {
                                                      .small-float-center {
                                                        margin: 0 auto !important;
                                                        float: none !important;
                                                        text-align: center !important;
                                                      }
                                                      .small-text-center {
                                                        text-align: center !important;
                                                      }
                                                      .small-text-left {
                                                        text-align: left !important;
                                                      }
                                                      .small-text-right {
                                                        text-align: right !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .hide-for-large {
                                                        display: block !important;
                                                        width: auto !important;
                                                        overflow: visible !important;
                                                        max-height: none !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .row.hide-for-large,
                                                      table.body table.container .row.hide-for-large {
                                                        display: table !important;
                                                        width: 100% !important;
                                                      }
                                                    }
    
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .show-for-large {
                                                        display: none !important;
                                                        width: 0;
                                                        mso-hide: all;
                                                        overflow: hidden;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body img {
                                                        width: auto !important;
                                                        height: auto !important;
                                                      }
                                                      table.body center {
                                                        min-width: 0 !important;
                                                      }
                                                      table.body .container {
                                                        width: 95% !important;
                                                      }
                                                      table.body .columns,
                                                      table.body .column {
                                                        height: auto !important;
                                                        -moz-box-sizing: border-box;
                                                        -webkit-box-sizing: border-box;
                                                        box-sizing: border-box;
                                                        padding-left: 16px !important;
                                                        padding-right: 16px !important;
                                                      }
                                                      table.body .columns .column,
                                                      table.body .columns .columns,
                                                      table.body .column .column,
                                                      table.body .column .columns {
                                                        padding-left: 0 !important;
                                                        padding-right: 0 !important;
                                                      }
                                                      table.body .collapse .columns,
                                                      table.body .collapse .column {
                                                        padding-left: 0 !important;
                                                        padding-right: 0 !important;
                                                      }
                                                      td.small-1,
                                                      th.small-1 {
                                                        display: inline-block !important;
                                                        width: 8.33333% !important;
                                                      }
                                                      td.small-2,
                                                      th.small-2 {
                                                        display: inline-block !important;
                                                        width: 16.66667% !important;
                                                      }
                                                      td.small-3,
                                                      th.small-3 {
                                                        display: inline-block !important;
                                                        width: 25% !important;
                                                      }
                                                      td.small-4,
                                                      th.small-4 {
                                                        display: inline-block !important;
                                                        width: 33.33333% !important;
                                                      }
                                                      td.small-5,
                                                      th.small-5 {
                                                        display: inline-block !important;
                                                        width: 41.66667% !important;
                                                      }
                                                      td.small-6,
                                                      th.small-6 {
                                                        display: inline-block !important;
                                                        width: 50% !important;
                                                      }
                                                      td.small-7,
                                                      th.small-7 {
                                                        display: inline-block !important;
                                                        width: 58.33333% !important;
                                                      }
                                                      td.small-8,
                                                      th.small-8 {
                                                        display: inline-block !important;
                                                        width: 66.66667% !important;
                                                      }
                                                      td.small-9,
                                                      th.small-9 {
                                                        display: inline-block !important;
                                                        width: 75% !important;
                                                      }
                                                      td.small-10,
                                                      th.small-10 {
                                                        display: inline-block !important;
                                                        width: 83.33333% !important;
                                                      }
                                                      td.small-11,
                                                      th.small-11 {
                                                        display: inline-block !important;
                                                        width: 91.66667% !important;
                                                      }
                                                      td.small-12,
                                                      th.small-12 {
                                                        display: inline-block !important;
                                                        width: 100% !important;
                                                      }
                                                      .columns td.small-12,
                                                      .column td.small-12,
                                                      .columns th.small-12,
                                                      .column th.small-12 {
                                                        display: block !important;
                                                        width: 100% !important;
                                                      }
                                                      .body .columns td.small-1,
                                                      .body .column td.small-1,
                                                      td.small-1 center,
                                                      .body .columns th.small-1,
                                                      .body .column th.small-1,
                                                      th.small-1 center {
                                                        display: inline-block !important;
                                                        width: 8.33333% !important;
                                                      }
                                                      .body .columns td.small-2,
                                                      .body .column td.small-2,
                                                      td.small-2 center,
                                                      .body .columns th.small-2,
                                                      .body .column th.small-2,
                                                      th.small-2 center {
                                                        display: inline-block !important;
                                                        width: 16.66667% !important;
                                                      }
                                                      .body .columns td.small-3,
                                                      .body .column td.small-3,
                                                      td.small-3 center,
                                                      .body .columns th.small-3,
                                                      .body .column th.small-3,
                                                      th.small-3 center {
                                                        display: inline-block !important;
                                                        width: 25% !important;
                                                      }
                                                      .body .columns td.small-4,
                                                      .body .column td.small-4,
                                                      td.small-4 center,
                                                      .body .columns th.small-4,
                                                      .body .column th.small-4,
                                                      th.small-4 center {
                                                        display: inline-block !important;
                                                        width: 33.33333% !important;
                                                      }
                                                      .body .columns td.small-5,
                                                      .body .column td.small-5,
                                                      td.small-5 center,
                                                      .body .columns th.small-5,
                                                      .body .column th.small-5,
                                                      th.small-5 center {
                                                        display: inline-block !important;
                                                        width: 41.66667% !important;
                                                      }
                                                      .body .columns td.small-6,
                                                      .body .column td.small-6,
                                                      td.small-6 center,
                                                      .body .columns th.small-6,
                                                      .body .column th.small-6,
                                                      th.small-6 center {
                                                        display: inline-block !important;
                                                        width: 50% !important;
                                                      }
                                                      .body .columns td.small-7,
                                                      .body .column td.small-7,
                                                      td.small-7 center,
                                                      .body .columns th.small-7,
                                                      .body .column th.small-7,
                                                      th.small-7 center {
                                                        display: inline-block !important;
                                                        width: 58.33333% !important;
                                                      }
                                                      .body .columns td.small-8,
                                                      .body .column td.small-8,
                                                      td.small-8 center,
                                                      .body .columns th.small-8,
                                                      .body .column th.small-8,
                                                      th.small-8 center {
                                                        display: inline-block !important;
                                                        width: 66.66667% !important;
                                                      }
                                                      .body .columns td.small-9,
                                                      .body .column td.small-9,
                                                      td.small-9 center,
                                                      .body .columns th.small-9,
                                                      .body .column th.small-9,
                                                      th.small-9 center {
                                                        display: inline-block !important;
                                                        width: 75% !important;
                                                      }
                                                      .body .columns td.small-10,
                                                      .body .column td.small-10,
                                                      td.small-10 center,
                                                      .body .columns th.small-10,
                                                      .body .column th.small-10,
                                                      th.small-10 center {
                                                        display: inline-block !important;
                                                        width: 83.33333% !important;
                                                      }
                                                      .body .columns td.small-11,
                                                      .body .column td.small-11,
                                                      td.small-11 center,
                                                      .body .columns th.small-11,
                                                      .body .column th.small-11,
                                                      th.small-11 center {
                                                        display: inline-block !important;
                                                        width: 91.66667% !important;
                                                      }
                                                      table.body td.small-offset-1,
                                                      table.body th.small-offset-1 {
                                                        margin-left: 8.33333% !important;
                                                        margin-left: 8.33333% !important;
                                                      }
                                                      table.body td.small-offset-2,
                                                      table.body th.small-offset-2 {
                                                        margin-left: 16.66667% !important;
                                                        margin-left: 16.66667% !important;
                                                      }
                                                      table.body td.small-offset-3,
                                                      table.body th.small-offset-3 {
                                                        margin-left: 25% !important;
                                                        margin-left: 25% !important;
                                                      }
                                                      table.body td.small-offset-4,
                                                      table.body th.small-offset-4 {
                                                        margin-left: 33.33333% !important;
                                                        margin-left: 33.33333% !important;
                                                      }
                                                      table.body td.small-offset-5,
                                                      table.body th.small-offset-5 {
                                                        margin-left: 41.66667% !important;
                                                        margin-left: 41.66667% !important;
                                                      }
                                                      table.body td.small-offset-6,
                                                      table.body th.small-offset-6 {
                                                        margin-left: 50% !important;
                                                        margin-left: 50% !important;
                                                      }
                                                      table.body td.small-offset-7,
                                                      table.body th.small-offset-7 {
                                                        margin-left: 58.33333% !important;
                                                        margin-left: 58.33333% !important;
                                                      }
                                                      table.body td.small-offset-8,
                                                      table.body th.small-offset-8 {
                                                        margin-left: 66.66667% !important;
                                                        margin-left: 66.66667% !important;
                                                      }
                                                      table.body td.small-offset-9,
                                                      table.body th.small-offset-9 {
                                                        margin-left: 75% !important;
                                                        margin-left: 75% !important;
                                                      }
                                                      table.body td.small-offset-10,
                                                      table.body th.small-offset-10 {
                                                        margin-left: 83.33333% !important;
                                                        margin-left: 83.33333% !important;
                                                      }
                                                      table.body td.small-offset-11,
                                                      table.body th.small-offset-11 {
                                                        margin-left: 91.66667% !important;
                                                        margin-left: 91.66667% !important;
                                                      }
                                                      table.body table.columns td.expander,
                                                      table.body table.columns th.expander {
                                                        display: none !important;
                                                      }
                                                      table.body .right-text-pad,
                                                      table.body .text-pad-right {
                                                        padding-left: 10px !important;
                                                      }
                                                      table.body .left-text-pad,
                                                      table.body .text-pad-left {
                                                        padding-right: 10px !important;
                                                      }
                                                      table.menu {
                                                        width: 100% !important;
                                                      }
                                                      table.menu td,
                                                      table.menu th {
                                                        width: auto !important;
                                                        display: inline-block !important;
                                                      }
                                                      table.menu.vertical td,
                                                      table.menu.vertical th,
                                                      table.menu.small-vertical td,
                                                      table.menu.small-vertical th {
                                                        display: block !important;
                                                      }
                                                      table.menu[align=""center""] {
                                                        width: auto !important;
                                                      }
                                                      table.button.expand {
                                                        width: 100% !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      .calendar-content {
                                                        padding: 0px !important;
                                                        width: 288px !important;
                                                      }
                                                      .not-available-day,
                                                      .calendar-today,
                                                      .available-day {
                                                        height: 40px !important;
                                                        width: 40px !important;
                                                      }
                                                      .day-label {
                                                        margin-left: 10% !important;
                                                        margin-top: 0% !important;
                                                        font-size: 15px;
                                                      }
	                                                  .p
	                                                  {
	                                                  font-size:16px
	                                                  font-size:4vw
	                                                  }
                                                    }
                                                  </style>";

                                string header = "<table class=\"body\" style=\"border-spacing:0;border-collapse:collapse;vertical-align:top;-webkit-hyphens:none;-moz-hyphens:none;hyphens:none;-ms-hyphens:none;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;margin:0;text-align:left;font-size:16px;line-height:19px;background:#f3f3f3;padding:0;width:100%;height:100%;color:#0a0a0a;margin-bottom:0px !important;background-color: white\">" +
                                                "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<td class=\"center\" align=\"center\" valign=\"top\" style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                                "<center style=\"width:100%;min-width:580px\">" +
                                                "<table class=\"container\" style=\"border-spacing:0;border-collapse:collapse;padding:0;vertical-align:top;background:#fefefe;width:580px;margin:0 auto;text-align:inherit;max-width:580px;\">" +
                                                "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<td style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                                "<div style=\"padding-top:10px\">" +
                                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<th style=\"width: 40%;>" +
                                                "<a href=\"https://www.hummingbirdindia.com\" target=\"_blank\" style=\"padding:0;margin:0;text-align:left;line-height:1.3;color:#2199e8;text-decoration:none\">" +
                                                "<img align=\"center\" alt=\"" + Imagealt1 + "\" class=\"center standard-header\" src=\"" + Imagelocation1 + "\" style=\"max-width: 120px\" >" +
                                                "</a></th><th style=\"font-size:16px;padding:20px 0 20px 0;line-height:28px\"> " +
                                                "<p>HB Confirmation #: " + ds.Tables[2].Rows[0][2].ToString() + "</p>" +
                                                "</th></tr></table></div><div>" +
                                                "<div class=\"headline-body\" style=\"padding-bottom:15px\">" +
                                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;width:564px;margin:0 auto;padding-left:16px;padding-right:16px;padding-bottom:0px !important\">" +
                                                "<p class=\"body  body-lg body-link-rausch light text-left   \" style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;line-height:1.4;font-weight:300;color:#484848;font-size:24px;hyphens:none;-ms-hyphens:none;-webkit-hyphens:none;-moz-hyphens:none;text-align:left;margin-bottom:0px !important;color:#0a0a0a;\">" + ds.Tables[2].Rows[0][1].ToString();

                                if (All.QReserveFlag == true)
                                {
                                    header += "<br /><span style=\"float: right;font-size:20px;\">Quick Reservation Confirmed</span>";
                                }
                                else
                                {
                                    header += "<br /><span style=\"float: right;font-size:20px;\">Reservation Confirmed</span>";
                                }

                                header += "</p></th></tr></table></div></div>";

                                string ChkInOutDate = "";
                                if (All.LTIAPIFlag == true)
                                {
                                    ChkInOutDate = "<div><div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                     "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                     "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                     "<p class=\"body-text-lg light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important'>‌" + All.CheckinDate + " ‌</p>" +
                                     "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>Check-in " + All.CheckinTime + "‌</p>" +
                                     "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                     "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                     "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                     "<p class=\"body-text-lg light text-right\" style='padding:0;margin:0;font-size:17px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important'>‌" + All.CheckoutDate + "</p>" +
                                     "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:16px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Check-out</p>" +
                                     "</th></tr></table></div><div>" +
                                     "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                     "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                     "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                     "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                     "</th></tr></table></div>";
                                }
                                else
                                {
                                    ChkInOutDate = "<div><div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                     "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                     "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                     "<p class=\"body-text-lg light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important'>‌" + ds.Tables[0].Rows[0][10].ToString() + " ‌</p>" +
                                     "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>Check-in " + ds.Tables[0].Rows[0][9].ToString() + "‌</p>" +
                                     "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                     "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                     "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                     "<p class=\"body-text-lg light text-right\" style='padding:0;margin:0;font-size:17px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important'>‌" + ds.Tables[0].Rows[0][11].ToString() + "</p>" +
                                     "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:16px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Check-out</p>" +
                                     "</th></tr></table></div><div>" +
                                     "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                     "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                     "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                     "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                     "</th></tr></table></div>";
                                }



                                string GuestTbl = "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"col-pad-left-2 col-pad-right-2\" style =\"padding:0;margin:0;padding-left:16px;padding-right:16px\">" +
                                                  "<div style=\"margin:0;-webkit-border-radius:8px;border-radius:8px;display:block;border-color:#d9242c;border-width:2px;border-style:dotted dashed;\">" +
                                                  "<p class=\"text-center\" style='font-weight:normal;padding:0;margin:0;text-align:center;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;font-size:24px;line-height:32px;margin-bottom:0px !important'>Guest Details</p>" +
                                                  "</div></th></tr></table></div>" +
                                                  "<div style=\"padding-top:8px;padding-bottom:8px;padding-left:16px;padding-right:16px;\">" +
                                                  "<table rules=\"rows\" style =\"border:#dbdbdb\">" +
                                                  "<tr><td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Guest Name </strong></td > " +
                                                  "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Room No / Occupancy </strong ></td > " +
                                                  "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Tariff / <br> Room / Day </strong ></td > " +
                                                  "</tr><tr></tr>";
                                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                {
                                    GuestTbl += "<tr style=\"font-style:normal;font-weight:normal;\" class=\"ng-scope\">" +
                                            "<td class=\"padd ng-binding\" style=\"vertical-align:middle;text-align:center\">" + ds.Tables[0].Rows[i][12].ToString() + ". " + ds.Tables[0].Rows[i][0].ToString() + " " + ds.Tables[0].Rows[i][13].ToString() + "</td>" +
                                            "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[0].Rows[i][6].ToString() + " / " + ds.Tables[0].Rows[i][7].ToString() + "</td>" +
                                            "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">INR " + ds.Tables[0].Rows[i][3].ToString() + "</td>" +
                                            "</tr>";
                                }
                                GuestTbl += "</table>";

                                string PayMode = "<div><div class=\"row-pad-bot-1\" style=\"padding-bottom:8px !important;padding-top:6px;\"></div>" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                                  "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:18px;margin-bottom:0px !important'>Tariff Payment: " + ds.Tables[0].Rows[0][4].ToString() + "</p>" +
                                                  "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                                  "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                                  "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                                  "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:18px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Service Payment: " + ds.Tables[0].Rows[0][5].ToString() + "</p>" +
                                                  "</th></tr></table></div></div><div>" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:3px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                  "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                  "</th></tr></table></div>";

                                string TariffDtls = "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                                    "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                    "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                                    "<p class=\"body-text-lg light color-rausch text-right\" style='padding:0;margin:0;word-break:normal;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;line-height:1.2;font-size:18px;text-align:center;color:#d9242c !important;margin-bottom:0px !important'>Guest Contacts</p>" +
                                                    "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>" +
                                                     MobileNo +
                                                    "</p></th></table></div><div>" +
                                                    "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                    "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                    "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                    "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                    "</th></tr></table></div>";


                                string PropertyDtls = "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table;\">" +
                                        "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                        "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                        "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:16px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-weight: bold;\">Property Name : " + ds.Tables[1].Rows[0][5].ToString() + " </p>" +
                                        "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:13px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;\">" + ds.Tables[1].Rows[0][0].ToString() + " </p>" +
                                        "</th></tr></table>" +
                                        "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                        "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                        "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                        "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                        "</th></tr></table></div>";

                                string Note = "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                            "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:14px;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;\"><strong>Note : </strong>" + SplNote + " </p>" +
                                            "</th></tr></table>" +
                                            "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                            "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                            "</th></tr></table></div>";

                                string ContactDtls = "<div style=\"padding-top:8px;padding-bottom:8px\" >" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' > Booked by</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][3].ToString() + " </ p >" +
                                            "</th>" +
                                            "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<hr>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<hr>" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' >Client Request #</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][12].ToString() + " </ p >" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<hr>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<hr>" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' >Issues / feedbacks</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + DeskNo + "<br>" + ds.Tables[10].Rows[0][1].ToString() + " </ p >" +
                                            "</th>" +
                                            "</tr></table></div>";



                                ContactDtls += "<div>" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                            "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                            "</th></tr></table></div><div style=\"padding-top:2px\">" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;text-align:left\"><th style=\"width: 60%;\">" +
                                            "<a href=\"" + link + "\" target=\"_blank\"><span style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:10px 0 10px 16px;margin:0;text-align:left;line-height:1.3;text-decoration:none;font-weight:300;color:#d9242c !important\">Security/Cancellation Policy</span></a>" +
                                            "</th><th style=\"font-size:10px;padding:10px 16px 10px 0;line-height:28px;text-align:right;\">" +
                                            "<p>Powered by Staysimplyfied.com</p>" +
                                            "</th></tr></table></div></tr></table></div>" +
                                            "</td></tr></table></center></td></tr></table>";

                                string EndData = "</body></html>";
                                MailContent = style + header + ChkInOutDate + GuestTbl + PayMode + TariffDtls + PropertyDtls + Note + ContactDtls + EndData;
                                message1.Body = MailContent;
                                message1.IsBodyHtml = true;


                            }
                        }
                    }
                    #endregion
                    #region
                    else
                    {

                        var ChCnt = 0;
                        var ChCntVal = "txt";

                        if (All.ResendFlag == true)
                        {
                            ChCnt = 1;
                            ChCntVal = "txt";
                        }
                        else
                        {
                            ChCnt = ds.Tables[3].Rows.Count;
                            ChCntVal = ds.Tables[3].Rows[0][4].ToString();
                        }

                        if (ChCntVal != "")
                        {
                            if (ds.Tables[10].Rows.Count > 0)
                            {
                                message1.From = new System.Net.Mail.MailAddress(ds.Tables[10].Rows[0][1].ToString(), "", System.Text.Encoding.UTF8);
                            }
                            else
                            {
                                message1.From = new System.Net.Mail.MailAddress("stay@hummingbirdindia.com", "", System.Text.Encoding.UTF8);
                            }
                            if (All.ResendFlag == true)
                            {
                                var Mail = All.PropertyGusetEmail.Split(',');
                                for (int i = 0; i < Mail.Length; i++)
                                {
                                    try
                                    {
                                        message1.To.Add(new System.Net.Mail.MailAddress(Mail[i].ToString()));
                                    }
                                    catch (Exception ex)
                                    {
                                        CreateLogFiles log = new CreateLogFiles();
                                        log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + Mail[i].ToString());
                                    }
                                }
                                if (All.UserEmail != "")
                                {
                                    try
                                    {
                                        message1.CC.Add(new System.Net.Mail.MailAddress(All.UserEmail));
                                    }
                                    catch (Exception ex)
                                    {
                                        CreateLogFiles log = new CreateLogFiles();
                                        log.ErrorLog("=> Confirmation Email API => Resend Guest Email => BookingId => " + All.BookingId + " => Invaild Email => To =>" + All.UserEmail);
                                    }
                                }


                                message1.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));

                            }
                            else
                            {

                                string PropertyMail = ds.Tables[3].Rows[0][4].ToString();
                                var PtyMail = PropertyMail.Split(',');
                                int cnt = PtyMail.Length;
                                for (int i = 0; i < cnt; i++)
                                {
                                    if (PtyMail[i].ToString() != "")
                                    {
                                        try
                                        {
                                            message1.To.Add(new System.Net.Mail.MailAddress(PtyMail[i].ToString()));
                                        }
                                        catch (Exception ex)
                                        {
                                            CreateLogFiles log = new CreateLogFiles();
                                            log.ErrorLog("=> Confirmation Email API => Room Level Confirmation Property Mail => To => BookingId => " + All.BookingId + " => Invalid Email => " + PtyMail[i].ToString());
                                        }
                                    }
                                }
                                ////for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                                ////{
                                ////    if (ds.Tables[3].Rows[i][2].ToString() != "")
                                ////    {
                                ////        try
                                ////        {
                                ////            message1.CC.Add(new System.Net.Mail.MailAddress(ds.Tables[3].Rows[i][2].ToString()));
                                ////        }
                                ////        catch (Exception ex)
                                ////        {
                                ////            CreateLogFiles log = new CreateLogFiles();
                                ////            log.ErrorLog("=> Confirmation Email API => Room Level Confirmation Property Mail => Cc => BookingId => " + All.BookingId + " => Invalid Email => " + ds.Tables[3].Rows[i][2].ToString());
                                ////        }
                                ////    }
                                ////}
                                ////if (ds.Tables[2].Rows[0][4].ToString() != "")
                                ////{
                                ////    try
                                ////    {
                                ////        message1.Bcc.Add(new System.Net.Mail.MailAddress(ds.Tables[2].Rows[0][4].ToString()));
                                ////    }
                                ////    catch (Exception ex)
                                ////    {
                                ////        CreateLogFiles log = new CreateLogFiles();
                                ////        log.ErrorLog("=> Confirmation Email API => Room Level Confirmation Property Mail => Bcc => BookingId => " + All.BookingId + " => Invalid Email => " + ds.Tables[2].Rows[0][4].ToString());
                                ////    }
                                ////}

                                ////if (ds.Tables[10].Rows[0][1].ToString() != "stay@hummingbirdindia.com")
                                ////{
                                ////    message1.Bcc.Add(new System.Net.Mail.MailAddress("stay@hummingbirdindia.com"));
                                ////}
                                ////message1.Bcc.Add(new System.Net.Mail.MailAddress("hbconf17@gmail.com"));
                            }
                            message1.Bcc.Add(new System.Net.Mail.MailAddress("anbu@warblerit.com"));

                            message1.Subject = "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString();

                            string typeofpty1 = ds.Tables[4].Rows[0][8].ToString();
                            string Imagelocation1 = "";
                            string Imagealt1 = "";
                            if (typeofpty1 == "MGH")
                            {
                                Imagelocation1 = ds.Tables[6].Rows[0][4].ToString();
                                Imagealt1 = ds.Tables[6].Rows[0][5].ToString();
                                if (Imagelocation1 == "")
                                {
                                    Imagelocation1 = ds.Tables[4].Rows[0][10].ToString();
                                    Imagealt1 = ds.Tables[4].Rows[0][11].ToString();
                                }
                            }
                            else
                            {
                                Imagelocation1 = ds.Tables[4].Rows[0][10].ToString();
                                Imagealt1 = ds.Tables[4].Rows[0][11].ToString();
                            }

                            // Contact Email 
                            string DeskNo = "";
                            DeskNo = ds.Tables[2].Rows[0][14].ToString();

                            // Guest Contact No.
                            string MobileNo = ds.Tables[4].Rows[0][4].ToString();
                            if (MobileNo == "")
                            {
                                MobileNo = " - NA - ";
                            }

                            // Spl Note
                            string SplNote = ds.Tables[2].Rows[0][8].ToString();
                            if (SplNote == "")
                            {
                                SplNote = "- NA -";
                            }


                            // View in browser Link
                            string id = ds.Tables[2].Rows[0][12].ToString();
                            string link = "http://mybooking.hummingbirdindia.com/?redirect=BookingPropertyConfirmation&B=R&R=" + id;


                            string style = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
                                                <html xmlns=""http://www.w3.org/1999/xhtml"" style=""min-height:100%;background:#f3f3f3"">
                                                <head><meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8""></head>
                                                <body style=""font-size:16px;min-width:100%;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;-moz-box-sizing:border-box;-webkit-box-sizing:border-box;box-sizing:border-box;font-family:'Circular', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;margin:0;line-height:1.3;color:#0a0a0a;text-align:left;width:100% !important"">
                                                <div class=""preheader"" style=""mso-hide:all;visibility:hidden;opacity:0;color:transparent;font-size:0px;width:0;height:0;display:none !important""></div>
                                                <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
                                                <meta name=""viewport"" content=""width=device-width"">
                                                <link href=""https://fonts.googleapis.com/css?family=Cabin&display=swap"" rel=""stylesheet"">
                                                <style data-roadie-ignore data-immutable=""true"">
                                                    @media only screen and (max-width: 596px) {
                                                      .small-float-center {
                                                        margin: 0 auto !important;
                                                        float: none !important;
                                                        text-align: center !important;
                                                      }
                                                      .small-text-center {
                                                        text-align: center !important;
                                                      }
                                                      .small-text-left {
                                                        text-align: left !important;
                                                      }
                                                      .small-text-right {
                                                        text-align: right !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .hide-for-large {
                                                        display: block !important;
                                                        width: auto !important;
                                                        overflow: visible !important;
                                                        max-height: none !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .row.hide-for-large,
                                                      table.body table.container .row.hide-for-large {
                                                        display: table !important;
                                                        width: 100% !important;
                                                      }
                                                    }
    
                                                    @media only screen and (max-width: 596px) {
                                                      table.body table.container .show-for-large {
                                                        display: none !important;
                                                        width: 0;
                                                        mso-hide: all;
                                                        overflow: hidden;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      table.body img {
                                                        width: auto !important;
                                                        height: auto !important;
                                                      }
                                                      table.body center {
                                                        min-width: 0 !important;
                                                      }
                                                      table.body .container {
                                                        width: 95% !important;
                                                      }
                                                      table.body .columns,
                                                      table.body .column {
                                                        height: auto !important;
                                                        -moz-box-sizing: border-box;
                                                        -webkit-box-sizing: border-box;
                                                        box-sizing: border-box;
                                                        padding-left: 16px !important;
                                                        padding-right: 16px !important;
                                                      }
                                                      table.body .columns .column,
                                                      table.body .columns .columns,
                                                      table.body .column .column,
                                                      table.body .column .columns {
                                                        padding-left: 0 !important;
                                                        padding-right: 0 !important;
                                                      }
                                                      table.body .collapse .columns,
                                                      table.body .collapse .column {
                                                        padding-left: 0 !important;
                                                        padding-right: 0 !important;
                                                      }
                                                      td.small-1,
                                                      th.small-1 {
                                                        display: inline-block !important;
                                                        width: 8.33333% !important;
                                                      }
                                                      td.small-2,
                                                      th.small-2 {
                                                        display: inline-block !important;
                                                        width: 16.66667% !important;
                                                      }
                                                      td.small-3,
                                                      th.small-3 {
                                                        display: inline-block !important;
                                                        width: 25% !important;
                                                      }
                                                      td.small-4,
                                                      th.small-4 {
                                                        display: inline-block !important;
                                                        width: 33.33333% !important;
                                                      }
                                                      td.small-5,
                                                      th.small-5 {
                                                        display: inline-block !important;
                                                        width: 41.66667% !important;
                                                      }
                                                      td.small-6,
                                                      th.small-6 {
                                                        display: inline-block !important;
                                                        width: 50% !important;
                                                      }
                                                      td.small-7,
                                                      th.small-7 {
                                                        display: inline-block !important;
                                                        width: 58.33333% !important;
                                                      }
                                                      td.small-8,
                                                      th.small-8 {
                                                        display: inline-block !important;
                                                        width: 66.66667% !important;
                                                      }
                                                      td.small-9,
                                                      th.small-9 {
                                                        display: inline-block !important;
                                                        width: 75% !important;
                                                      }
                                                      td.small-10,
                                                      th.small-10 {
                                                        display: inline-block !important;
                                                        width: 83.33333% !important;
                                                      }
                                                      td.small-11,
                                                      th.small-11 {
                                                        display: inline-block !important;
                                                        width: 91.66667% !important;
                                                      }
                                                      td.small-12,
                                                      th.small-12 {
                                                        display: inline-block !important;
                                                        width: 100% !important;
                                                      }
                                                      .columns td.small-12,
                                                      .column td.small-12,
                                                      .columns th.small-12,
                                                      .column th.small-12 {
                                                        display: block !important;
                                                        width: 100% !important;
                                                      }
                                                      .body .columns td.small-1,
                                                      .body .column td.small-1,
                                                      td.small-1 center,
                                                      .body .columns th.small-1,
                                                      .body .column th.small-1,
                                                      th.small-1 center {
                                                        display: inline-block !important;
                                                        width: 8.33333% !important;
                                                      }
                                                      .body .columns td.small-2,
                                                      .body .column td.small-2,
                                                      td.small-2 center,
                                                      .body .columns th.small-2,
                                                      .body .column th.small-2,
                                                      th.small-2 center {
                                                        display: inline-block !important;
                                                        width: 16.66667% !important;
                                                      }
                                                      .body .columns td.small-3,
                                                      .body .column td.small-3,
                                                      td.small-3 center,
                                                      .body .columns th.small-3,
                                                      .body .column th.small-3,
                                                      th.small-3 center {
                                                        display: inline-block !important;
                                                        width: 25% !important;
                                                      }
                                                      .body .columns td.small-4,
                                                      .body .column td.small-4,
                                                      td.small-4 center,
                                                      .body .columns th.small-4,
                                                      .body .column th.small-4,
                                                      th.small-4 center {
                                                        display: inline-block !important;
                                                        width: 33.33333% !important;
                                                      }
                                                      .body .columns td.small-5,
                                                      .body .column td.small-5,
                                                      td.small-5 center,
                                                      .body .columns th.small-5,
                                                      .body .column th.small-5,
                                                      th.small-5 center {
                                                        display: inline-block !important;
                                                        width: 41.66667% !important;
                                                      }
                                                      .body .columns td.small-6,
                                                      .body .column td.small-6,
                                                      td.small-6 center,
                                                      .body .columns th.small-6,
                                                      .body .column th.small-6,
                                                      th.small-6 center {
                                                        display: inline-block !important;
                                                        width: 50% !important;
                                                      }
                                                      .body .columns td.small-7,
                                                      .body .column td.small-7,
                                                      td.small-7 center,
                                                      .body .columns th.small-7,
                                                      .body .column th.small-7,
                                                      th.small-7 center {
                                                        display: inline-block !important;
                                                        width: 58.33333% !important;
                                                      }
                                                      .body .columns td.small-8,
                                                      .body .column td.small-8,
                                                      td.small-8 center,
                                                      .body .columns th.small-8,
                                                      .body .column th.small-8,
                                                      th.small-8 center {
                                                        display: inline-block !important;
                                                        width: 66.66667% !important;
                                                      }
                                                      .body .columns td.small-9,
                                                      .body .column td.small-9,
                                                      td.small-9 center,
                                                      .body .columns th.small-9,
                                                      .body .column th.small-9,
                                                      th.small-9 center {
                                                        display: inline-block !important;
                                                        width: 75% !important;
                                                      }
                                                      .body .columns td.small-10,
                                                      .body .column td.small-10,
                                                      td.small-10 center,
                                                      .body .columns th.small-10,
                                                      .body .column th.small-10,
                                                      th.small-10 center {
                                                        display: inline-block !important;
                                                        width: 83.33333% !important;
                                                      }
                                                      .body .columns td.small-11,
                                                      .body .column td.small-11,
                                                      td.small-11 center,
                                                      .body .columns th.small-11,
                                                      .body .column th.small-11,
                                                      th.small-11 center {
                                                        display: inline-block !important;
                                                        width: 91.66667% !important;
                                                      }
                                                      table.body td.small-offset-1,
                                                      table.body th.small-offset-1 {
                                                        margin-left: 8.33333% !important;
                                                        margin-left: 8.33333% !important;
                                                      }
                                                      table.body td.small-offset-2,
                                                      table.body th.small-offset-2 {
                                                        margin-left: 16.66667% !important;
                                                        margin-left: 16.66667% !important;
                                                      }
                                                      table.body td.small-offset-3,
                                                      table.body th.small-offset-3 {
                                                        margin-left: 25% !important;
                                                        margin-left: 25% !important;
                                                      }
                                                      table.body td.small-offset-4,
                                                      table.body th.small-offset-4 {
                                                        margin-left: 33.33333% !important;
                                                        margin-left: 33.33333% !important;
                                                      }
                                                      table.body td.small-offset-5,
                                                      table.body th.small-offset-5 {
                                                        margin-left: 41.66667% !important;
                                                        margin-left: 41.66667% !important;
                                                      }
                                                      table.body td.small-offset-6,
                                                      table.body th.small-offset-6 {
                                                        margin-left: 50% !important;
                                                        margin-left: 50% !important;
                                                      }
                                                      table.body td.small-offset-7,
                                                      table.body th.small-offset-7 {
                                                        margin-left: 58.33333% !important;
                                                        margin-left: 58.33333% !important;
                                                      }
                                                      table.body td.small-offset-8,
                                                      table.body th.small-offset-8 {
                                                        margin-left: 66.66667% !important;
                                                        margin-left: 66.66667% !important;
                                                      }
                                                      table.body td.small-offset-9,
                                                      table.body th.small-offset-9 {
                                                        margin-left: 75% !important;
                                                        margin-left: 75% !important;
                                                      }
                                                      table.body td.small-offset-10,
                                                      table.body th.small-offset-10 {
                                                        margin-left: 83.33333% !important;
                                                        margin-left: 83.33333% !important;
                                                      }
                                                      table.body td.small-offset-11,
                                                      table.body th.small-offset-11 {
                                                        margin-left: 91.66667% !important;
                                                        margin-left: 91.66667% !important;
                                                      }
                                                      table.body table.columns td.expander,
                                                      table.body table.columns th.expander {
                                                        display: none !important;
                                                      }
                                                      table.body .right-text-pad,
                                                      table.body .text-pad-right {
                                                        padding-left: 10px !important;
                                                      }
                                                      table.body .left-text-pad,
                                                      table.body .text-pad-left {
                                                        padding-right: 10px !important;
                                                      }
                                                      table.menu {
                                                        width: 100% !important;
                                                      }
                                                      table.menu td,
                                                      table.menu th {
                                                        width: auto !important;
                                                        display: inline-block !important;
                                                      }
                                                      table.menu.vertical td,
                                                      table.menu.vertical th,
                                                      table.menu.small-vertical td,
                                                      table.menu.small-vertical th {
                                                        display: block !important;
                                                      }
                                                      table.menu[align=""center""] {
                                                        width: auto !important;
                                                      }
                                                      table.button.expand {
                                                        width: 100% !important;
                                                      }
                                                    }
                                                    @media only screen and (max-width: 596px) {
                                                      .calendar-content {
                                                        padding: 0px !important;
                                                        width: 288px !important;
                                                      }
                                                      .not-available-day,
                                                      .calendar-today,
                                                      .available-day {
                                                        height: 40px !important;
                                                        width: 40px !important;
                                                      }
                                                      .day-label {
                                                        margin-left: 10% !important;
                                                        margin-top: 0% !important;
                                                        font-size: 15px;
                                                      }
	                                                  .p
	                                                  {
	                                                  font-size:16px
	                                                  font-size:4vw
	                                                  }
                                                    }
                                                  </style>";

                            string header = "<table class=\"body\" style=\"border-spacing:0;border-collapse:collapse;vertical-align:top;-webkit-hyphens:none;-moz-hyphens:none;hyphens:none;-ms-hyphens:none;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;margin:0;text-align:left;font-size:16px;line-height:19px;background:#f3f3f3;padding:0;width:100%;height:100%;color:#0a0a0a;margin-bottom:0px !important;background-color: white\">" +
                                            "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<td class=\"center\" align=\"center\" valign=\"top\" style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                            "<center style=\"width:100%;min-width:580px\">" +
                                            "<table class=\"container\" style=\"border-spacing:0;border-collapse:collapse;padding:0;vertical-align:top;background:#fefefe;width:580px;margin:0 auto;text-align:inherit;max-width:580px;\">" +
                                            "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<td style=\"font-size:16px;word-wrap:break-word;-webkit-hyphens:auto;-moz-hyphens:auto;hyphens:auto;vertical-align:top;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;font-weight:normal;border-collapse:collapse !important\">" +
                                            "<div style=\"padding-top:10px\">" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th style=\"width: 40%;>" +
                                            "<a href=\"https://www.hummingbirdindia.com\" target=\"_blank\" style=\"padding:0;margin:0;text-align:left;line-height:1.3;color:#2199e8;text-decoration:none\">" +
                                            "<img align=\"center\" alt=\"" + Imagealt1 + "\" class=\"center standard-header\" src=\"" + Imagelocation1 + "\" style=\"max-width: 120px\" >" +
                                            "</a></th><th style=\"font-size:16px;padding:20px 0 20px 0;line-height:28px\"> " +
                                            "<p>HB Confirmation #: " + ds.Tables[2].Rows[0][2].ToString() + "</p>";

                            if (ds.Tables[4].Rows[0][19].ToString() != "")
                            {
                                header += "<p>Confirmed by: " + ds.Tables[4].Rows[0][19].ToString() + "</p>";
                            }



                            header += "</th></tr></table></div><div>" +
                                "<div class=\"headline-body\" style=\"padding-bottom:15px\">" +
                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;width:564px;margin:0 auto;padding-left:16px;padding-right:16px;padding-bottom:0px !important\">" +
                                "<p class=\"body  body-lg body-link-rausch light text-left   \" style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:0;margin:0;line-height:1.4;font-weight:300;color:#484848;font-size:24px;hyphens:none;-ms-hyphens:none;-webkit-hyphens:none;-moz-hyphens:none;text-align:left;margin-bottom:0px !important;color:#0a0a0a;\">" + ds.Tables[2].Rows[0][1].ToString();

                            if (All.QReserveFlag == true)
                            {
                                header += "<br /><span style=\"float: right;font-size:20px;\">Quick Reservation Confirmed</span>";
                            }
                            else
                            {
                                header += "<br /><span style=\"float: right;font-size:20px;\">Reservation Confirmed</span>";
                            }



                            header += "</p>" +
                               "</th></tr></table></div></div>";

                            string ChkInOutDate = "";
                            if (All.LTIAPIFlag == true)
                            {

                                ChkInOutDate = "<div><div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                                  "<p class=\"body-text-lg light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important'>‌" + All.CheckinDate + " ‌</p>" +
                                                  "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>Check-in " + All.CheckinTime + "‌</p>" +
                                                  "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                                  "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                                  "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                                  "<p class=\"body-text-lg light text-right\" style='padding:0;margin:0;font-size:17px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important'>‌" + All.CheckoutDate + "</p>" +
                                                  "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:16px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Check-out</p>" +
                                                  "</th></tr></table></div><div>" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                  "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                  "</th></tr></table></div>";
                            }
                            else
                            {
                                ChkInOutDate = "<div><div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                                  "<p class=\"body-text-lg light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-size:17px;margin-bottom:0px !important'>‌" + ds.Tables[0].Rows[0][9].ToString() + " ‌</p>" +
                                                  "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>Check-in " + ds.Tables[0].Rows[0][11].ToString() + "‌</p>" +
                                                  "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                                  "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                                  "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                                  "<p class=\"body-text-lg light text-right\" style='padding:0;margin:0;font-size:17px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;text-align:right;margin-bottom:0px !important'>‌" + ds.Tables[0].Rows[0][10].ToString() + "</p>" +
                                                  "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:16px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Check-out</p>" +
                                                  "</th></tr></table></div><div>" +
                                                  "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                  "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                  "<th class=\"small-12 large-12 columns first last\" style=\"padding-bottom:5px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                  "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                  "</th></tr></table></div>";
                            }






                            string GuestTbl = "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                              "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                              "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                              "<th class=\"col-pad-left-2 col-pad-right-2\" style =\"padding:0;margin:0;padding-left:16px;padding-right:16px\">" +
                                              "<div style=\"margin:0;-webkit-border-radius:8px;border-radius:8px;display:block;border-color:#d9242c;border-width:2px;border-style:dotted dashed;\">" +
                                              "<p class=\"text-center\" style='font-weight:normal;padding:0;margin:0;text-align:center;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;font-size:24px;line-height:32px;margin-bottom:0px !important'>Guest Details</p>" +
                                              "</div></th></tr></table></div>" +
                                              "<div style=\"padding-top:8px;padding-bottom:8px;padding-left:16px;padding-right:16px;\">" +
                                              "<table rules=\"rows\" style =\"border:#dbdbdb\">" +
                                              "<tr><td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Guest Name </strong></td > " +
                                              "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Room No / Occupancy </strong ></td > " +
                                              "<td style=\"font-size:13px; width:16%;\" valign=\"top\" align=\"center\" ><strong> Tariff / <br> Room / Day </strong ></td > " +
                                              "</tr><tr></tr>";
                            for (int i = 0; i < ds.Tables[11].Rows.Count; i++)
                            {

                                if ((typeofpty1 == "MGH") || (typeofpty1 == "DdP"))
                                {
                                    GuestTbl += "<tr style=\"font-style:normal;font-weight:normal;\" class=\"ng-scope\">" +
                                                "<td class=\"padd ng-binding\" style=\"vertical-align:middle;text-align:center\">" + ds.Tables[11].Rows[i][11].ToString() + "</td>" +
                                                "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[11].Rows[i][10].ToString() + " / " + ds.Tables[11].Rows[i][4].ToString() + "</td>" +
                                                "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">INR " + ds.Tables[11].Rows[i][3].ToString() + "</td>" +
                                                "</tr>";
                                }
                                else
                                {
                                    GuestTbl += "<tr style=\"font-style:normal;font-weight:normal;\" class=\"ng-scope\">" +
                                     "<td class=\"padd ng-binding\" style=\"vertical-align:middle;text-align:center\">" + ds.Tables[11].Rows[i][11].ToString() + "</td>" +
                                     "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">" + ds.Tables[4].Rows[0][12].ToString() + " / " + ds.Tables[11].Rows[i][4].ToString() + "</td>" +
                                     "<td class=\"padd ng-binding\" style=\"vertical-align: middle; text-align: center;\">INR " + ds.Tables[11].Rows[i][3].ToString() + "</td>" +
                                     "</tr>";
                                }



                            }
                            GuestTbl += "</table>";

                            string PayMode = "<div><div class=\"row-pad-bot-1\" style=\"padding-bottom:8px !important;padding-top:6px;\"></div>" +
                                              "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                              "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                              "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;width:225.66667px;padding-left:16px\">" +
                                              "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:18px;margin-bottom:0px !important'>Tariff Payment: " + ds.Tables[11].Rows[0][5].ToString() + "</p>" +
                                              "</th><th class=\"small-2 large-2 columns\" style=\"font-size:16px;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;text-align:left;line-height:1.3;padding-right:8px;width:80.66667px;padding-bottom:16px;padding-left:16px;margin:0 auto\">" +
                                              "<img alt=\"\" class=\"slash text-center\" src=\"https://endpoint887127.azureedge.net/img/slash.png\" style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;display:block;clear:both;max-width:40px;width:40px;text-align:center;float:none;margin:0 auto\">" +
                                              "</th><th class=\"small-5 large-5 columns last\" style=\"font-size:16px;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;line-height:1.3;width:225.66667px;padding-left:16px;margin:0 auto;padding-bottom:16px;padding-right:16px\">" +
                                              "<p class=\"body-text light text-right\" style='padding:0;margin:0;font-size:18px;font-weight:300;font-family:\"Circular\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;text-align:right;margin-bottom:0px !important'>Service Payment: " + ds.Tables[11].Rows[0][6].ToString() + "</p>" +
                                              "</th></tr></table></div></div><div>" +
                                              "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                              "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                              "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:3px;width:564px;padding-left:16px;padding-right:16px\">" +
                                              "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                              "</th></tr></table></div>";

                            string TariffDtls = "<div style=\"padding-top:8px;padding-bottom:8px\">" +
                                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">";
                            string Stng = ds.Tables[11].Rows[0][8].ToString();
                            if (ds.Tables[11].Rows[0][7].ToString() == "NOTBTC")
                            {
                                if (Stng != "")
                                {
                                    TariffDtls +=
                                                "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px\">" +
                                                "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0;text-align:center;font-size:18px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#d9242c !important;;word-break:normal;line-height:1.2;\">Agreed Tariff</p>" +
                                    "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>" + Stng + "</p>" +

                                                                                                        "</th>";
                                }


                            }
                            else
                            {
                                try
                                {
                                    string file = ds.Tables[4].Rows[0][1].ToString();
                                    System.Net.Mail.Attachment att = new System.Net.Mail.Attachment(file);
                                    att.Name = ds.Tables[4].Rows[0][2].ToString();
                                    message1.Attachments.Add(att);
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog(" => Room Level Property Mail => BookingId => " + All.BookingId + " => PDF Attachment => Err Msg => " + ex.Message);
                                }
                                TariffDtls += "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px\">" +
                                              "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0;text-align:center;font-size:18px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#d9242c !important;;word-break:normal;line-height:1.2;\">Agreed Tariff</p>" +
                                              "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>" + Stng + "</p>" +
                                              "</th>";
                            }
                            TariffDtls += "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                                "<p class=\"body-text-lg light color-rausch text-right\" style='padding:0;margin:0;word-break:normal;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;line-height:1.2;font-size:18px;text-align:center;color:#d9242c !important;margin-bottom:0px !important'>Guest Contacts</p>" +
                                                "<p class=\"body-text light\" style='margin:0;text-align:left;padding:0;font-weight:300;font-family:\"Cabin\", \"Helvetica\", Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.4;font-size:16px;margin-bottom:0px !important'>" +
                                                 MobileNo +
                                                "</p></th></table></div><div>" +
                                                "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                "</th></tr></table></div>";


                            string PropertyDtls = "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table;\">" +
                                  "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                  "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                  "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:16px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;font-weight: bold;\">Property Name : " + ds.Tables[1].Rows[0][5].ToString() + " </p>" +
                                   "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:13px;font-weight:300;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;\">" + ds.Tables[1].Rows[0][0].ToString() + " </p>" +
                                  "</th></tr></table>" +
                                  "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                  "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                  "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                  "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                  "</th></tr></table></div>";

                            string Note = "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                         "<tr style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                         "<th class=\"small-5 large-5 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;padding-bottom:16px;padding-left:16px\">" +
                                                         "<p class=\"body-text-lg light row-pad-bot-1\" style=\"padding:0;margin:0 0 5px 0;text-align:center;font-size:14px;font-family:'Cabin',Helvetica, Arial, sans-serif;color:#484848;word-break:normal;line-height:1.2;\"><strong>Note : </strong>" + SplNote + " </p>" +
                                                         "</th></tr></table>" +
                                                         "<div><table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                         "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                         "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                         "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                         "</th></tr></table></div>";



                            string ContactDtls = "<div style=\"padding-top:8px;padding-bottom:8px\" >" +
                                            "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                            "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' > Booked by</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][3].ToString() + " </ p >" +
                                            "</th>" +
                                            "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<hr>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<hr>" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' >Client Request #</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + ds.Tables[2].Rows[0][13].ToString() + " </ p >" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<hr>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<hr>" +
                                            "</th>" +
                                             "</tr><tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                            "<th class=\"small-7 large-7 columns first\" style=\"font-size:16px;text-align:left;line-height:1.3;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;color:#0a0a0a;padding-right:8px;margin:0 auto;width:322.33333px;padding-left:16px;vertical-align: top;\">" +
                                            "<p style = 'margin:0;text-align:left;padding:0;' >Issues / feedbacks</p>" +
                                            "<th class=\"small-5 large-5 columns last valign-top\" style=\"font-size:16px;text-align:left;line-height:1.3;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;padding:0;vertical-align:top;width:225.66667px;padding-left:16px;margin:0 auto;padding-right:16px\">" +
                                            "<p style = 'padding:0;margin:0;text-align:right;margin-bottom:0px !important' > " + DeskNo + "<br>" + ds.Tables[10].Rows[0][0].ToString() + " </ p >" +
                                            "</th>" +
                                            "</tr></table></div>";

                            ContactDtls += "<div>" +
                                                 "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;vertical-align:top;padding:0;width:100%;position:relative;display:table\">" +
                                                 "<tr class=\"\" style=\"padding:0;vertical-align:top;text-align:left\">" +
                                                 "<th class=\"small-12 large-12 columns first last\" style=\"font-size:16px;padding:0;text-align:left;color:#0a0a0a;font-family: 'Cabin', Helvetica, Arial, sans-serif;font-weight:normal;line-height:1.3;margin:0 auto;padding-bottom:16px;width:564px;padding-left:16px;padding-right:16px\">" +
                                                 "<hr class=\"full-divider\" style=\"clear:both;max-width:580px;border-right:0;border-top:0;border-left:0;margin:20px auto;border-bottom:1px solid #cacaca;background-color:#dbdbdb;height:1px;border:none;width:100%;margin-top:0;margin-bottom:0\">" +
                                                 "</th></tr></table></div><div style=\"padding-top:2px\">" +
                                                 "<table class=\"row\" style=\"border-spacing:0;border-collapse:collapse;text-align:left;padding:0;width:100%;position:relative;display:table\">" +
                                                 "<tr class=\"\" style=\"padding:0;text-align:left\"><th style=\"width: 60%;\">" +
                                                 "<a href=\"" + link + "\" target=\"_blank\"><span style=\"font-family: 'Cabin', Helvetica, Arial, sans-serif;padding:10px 0 10px 16px;margin:0;text-align:left;line-height:1.3;text-decoration:none;font-weight:300;color:#d9242c !important\">Security/Cancellation Policy</span></a>" +
                                                 "</th><th style=\"font-size:10px;padding:10px 16px 10px 0;line-height:28px;text-align:right;\">" +
                                                 "<p>Powered by Staysimplyfied.com</p>" +
                                                 "</th></tr></table></div></tr></table></div>" +
                                                 "</td></tr></table></center></td></tr></table>";

                            string EndData = "</body></html>";
                            MailContent = style + header + ChkInOutDate + GuestTbl + PayMode + TariffDtls + PropertyDtls + Note + ContactDtls + EndData;
                            message1.Body = MailContent;
                            message1.IsBodyHtml = true;
                        }

                    }
                    #endregion

                    try
                    {
                        smtp1.Send(message1);
                        Response2 = "Success";
                    }
                    catch (Exception ex)
                    {
                        CreateLogFiles log = new CreateLogFiles();
                        log.ErrorLog("=> Property Confirmation Mail => smtp => BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);
                        Response2 = "Failure";
                    }


                }
                else
                {
                    Response2 = "Success";
                }
                #endregion

                #region
                if (All.SmsChk == true)
                {
                    string PaymentMode = "";
                    string Maplink = "";
                    string WhatsappFileName = "Booking Confirmation -" + ds.Tables[2].Rows[0][2].ToString();
                    string paths123 = @"D:\home\site\wwwroot\Confirmations\";
                    var FileName = paths123 + "Booking Confirmation - " + ds.Tables[2].Rows[0][2].ToString() + ".pdf";
                    string WhatsappPdfUrl = AzureBlobPdfURl;
                    if (ds.Tables[0].Rows[0][8].ToString() == "Bed")
                    {
                        PaymentMode = ds.Tables[0].Rows[0][4].ToString();
                        Maplink = ds.Tables[1].Rows[0][13].ToString();

                    }
                    else
                    {
                        PaymentMode = ds.Tables[0].Rows[0][5].ToString();
                        Maplink = ds.Tables[1].Rows[0][13].ToString();
                    }

                    string FinalAPIUrl = "";
                    FinalAPIUrl = System.Configuration.ConfigurationManager.AppSettings["UrlShortner"] + "/API/UrlShortner/urlshort";
                    List<ConfirmationEMail> Msg = new List<ConfirmationEMail>();
                    try
                    {
                        SqlCommand command3 = new SqlCommand();
                        DataSet ds3 = new DataSet();
                        command3.CommandText = "SP_MMTBooking_Help";
                        command3.CommandType = CommandType.StoredProcedure;
                        command3.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "BookingConfirmedSMS";
                        command3.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = All.BookingId;
                        command3.Parameters.Add("@Str1", SqlDbType.NVarChar).Value = "";
                        command3.Parameters.Add("@Str2", SqlDbType.NVarChar).Value = "";
                        command3.Parameters.Add("@Id1", SqlDbType.BigInt).Value = 0;
                        command3.Parameters.Add("@Id2", SqlDbType.BigInt).Value = 0;
                        ds3 = new DBconnection().ExecuteDataSet(command3, "");
                        var myData = ds3.Tables[0].AsEnumerable().Select(r => new ConfirmationEMail
                        {
                            CityCode = r.Field<string>("CityCode"),
                            Bookingcode = r.Field<string>("PropertyId"),
                            RowId = r.Field<string>("RatePlanCode"),
                            Caretaker = r.Field<long>("Caretaker"),
                            MobileNo = r.Field<string>("MobileNo"),
                            WhatsAppMsg = r.Field<string>("WhatsAppMsg")

                        });
                        Msg = myData.ToList();
                    }
                    catch (Exception ex)
                    {
                        CreateLogFiles log = new CreateLogFiles();
                        log.ErrorLog(" => Confirmation Email API => Booking Confirmation SMS => Get data from Procedure => BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);
                    }
                    string MapLink = "";
                    String FinalMap = "";
                    if (Maplink != "" && Maplink != null && Maplink != " ")
                    {
                        MapLink = "https://www.google.co.in/maps/place/" + Maplink;
                    }

                    try
                    {
                        if (Msg.Count > 0)
                        {
                            for (int i = 0; i < Msg.Count; i++)
                            {

                                //Firebase URL Start for Cancel
                                WebClient client = new WebClient();
                                client.Headers.Add("Content-Type", "application/json");
                                string LongPath = "http://mybooking.hummingbirdindia.com/" + "?B=" + Msg[i].Bookingcode + "$R=" + Msg[i].RowId;
                                string body = "{\"longUrl\":\"" + LongPath + "\"}";
                                try
                                {
                                    string ShortURl = client.UploadString(FinalAPIUrl, "POST", body);
                                    Msg[i].ShortPath = ShortURl;
                                    Msg[i].ShortPath = Msg[i].ShortPath.Replace("\"", "");
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog(" => Confirmation Email API => Booking Confirmation SMS => Cancel Link - BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);

                                }
                                //Firebase URL End For Cancel
                                //Firebase URL Start for Map
                                WebClient client1 = new WebClient();
                                client.Headers.Add("Content-Type", "application/json");
                                string LongPath1 = MapLink;
                                string body1 = "{\"longUrl\":\"" + LongPath1 + "\"}";
                                try
                                {
                                    string ShortURl1 = client.UploadString(FinalAPIUrl, "POST", body1);
                                    FinalMap = ShortURl1;
                                    FinalMap = FinalMap.Replace("\"", "");
                                }
                                catch (Exception ex)
                                {
                                    CreateLogFiles log = new CreateLogFiles();
                                    log.ErrorLog(" => Confirmation Email API => Booking Confirmation SMS => Map Link - BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);

                                }
                                //Short URL End0

                                if (Msg[i].Caretaker == 0)
                                {
                                    if (PaymentMode == "Bill to Company (BTC)")
                                    {
                                        if (FinalMap != "")
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ".Map:" + " " + FinalMap + " .");
                                        }
                                    }
                                    else
                                    {
                                        if (FinalMap != "")
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ".Map:" + " " + FinalMap + ". To view you Booking / cancel your Booking,click the below link" + " " + Msg[i].ShortPath + " .");
                                        }
                                        else
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ". To view you Booking / cancel your Booking,click the below link" + " " + Msg[i].ShortPath + " .");
                                        }
                                    }
                                }
                                else
                                {
                                    if (PaymentMode == "Bill to Company (BTC)")
                                    {
                                        if (FinalMap != "")
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ".Map:" + " " + FinalMap + " .");
                                        }
                                    }
                                    else
                                    {
                                        if (FinalMap != "")
                                        {
                                            Msg[i].CityCode = Msg[i].CityCode.Replace("relbraw", ".Map:" + " " + FinalMap + " ."); //+ Msg[i].ShortPath removed by Pooranam
                                        }
                                    }
                                }
                                if (Msg[i].MobileNo != "" && All.GuestMailChk == true)
                                {
                                    try
                                    {
                                        WhatsappObj WhatsappData = new WhatsappObj();
                                        WhatsappData.MobileNo = Msg[i].MobileNo;
                                        WhatsappData.Msg = Msg[i].WhatsAppMsg;
                                        WhatsappData.WhatsappFileName = WhatsappFileName;
                                        WhatsappData.WhatsappPdfUrl = WhatsappPdfUrl;
                                        Task.Factory.StartNew(() => WhatsappAPI(WhatsappData));
                                    }
                                    catch (Exception Ex)
                                    {
                                        CreateLogFiles log = new CreateLogFiles();
                                        log.ErrorLog(" => Confirmation WhatsAPP API => Booking Confirmation WhatsApp => BookingId => " + All.BookingId + " => Err Msg => " + Ex.Message);

                                    }


                                }
                                WebRequest request = HttpWebRequest.Create(Msg[i].CityCode);
                                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                Stream s = (Stream)response.GetResponseStream();
                                StreamReader readStream = new StreamReader(s);
                                string dataString = readStream.ReadToEnd();
                                CreateLogFiles lognew = new CreateLogFiles();
                                lognew.ErrorLog("BookingId => " + All.BookingId + " => Status => " + dataString + " => Link " + Msg[i].CityCode);
                                response.Close();
                                s.Close();
                                readStream.Close();
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        CreateLogFiles log = new CreateLogFiles();
                        log.ErrorLog(" => Confirmation Email API => Booking Confirmation SMS => BookingId => " + All.BookingId + " => Err Msg => " + ex.Message);

                    }
                }
                #endregion

                if (Response1 == "Success" && Response2 == "Failure")
                {
                    Response = "Confirmation Email not Sent to Property.";
                }
                else if (Response1 == "Failure" && Response2 == "Success")
                {
                    Response = "Confirmation Email not Sent to Guest.";
                }
                else if (Response1 == "Failure" && Response2 == "Failure")
                {
                    Response = "Confirmation Email not Sent to Guest & Property.";
                }
                else
                {
                    Response = "Confirmation Email Sent Successfully";
                }

                return Json(new { Code = "200", EmailResponse = Response });
            }
            catch (Exception Ex)
            {
                log = new CreateLogFiles();
                log.ErrorLog(" => Confirmation Email API => BookingId => " + All.BookingId + "=>" + Ex.Message);
                return Json(new { Code = "400", EmailResponse = "Confirmation Email not Sent - " + Ex.Message });
            }
        }

        //testing

        public string WhatsappAPI(WhatsappObj Details)
        {
            Details.MobileNo = Details.MobileNo.Replace("+", "");
            if (Details.MobileNo.Length == 10)
            {
                Details.MobileNo = "91" + Details.MobileNo;
            }

            string RR1 = "Success";
            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string body = "{\"@VER\": \"1.2\"," +
        "\"USER\": {" +
    "\"@USERNAME\": \"hummingWA\"," +
    "\"@PASSWORD\": \"humng891\"," +
    "\"@UNIXTIMESTAMP\": \"\"" +
  "}," +
  "\"DLR\": {" +
  "\"@URL\": \"\"" +
  "}," +
  "\"SMS\": [" +
    "{" +
      "\"@UDH\": \"0\"," +
      "\"@CODING\": \"1\"," +
      "\"@TEXT\": \"" + Details.Msg + "\"," +
      "\"@MEDIADATA\": \"" + Details.WhatsappPdfUrl + "\"," +
      "\"@MSGTYPE\": \"3\"," +
      "\"@TYPE\": \"document~" + Details.WhatsappFileName + "\"," +
      "\"@PROPERTY\": \"0\"," +
      "\"@ID\": \"1\"," +
      "\"ADDRESS\": [" +
        "{" +
          "\"@FROM\": \"918884854455\"," +
          "\"@TO\": \"" + Details.MobileNo + "\"," +
          "\"@SEQ\": \"1\"," +
          "\"@TAG\": \"\"" +
        "}" +
      "]" +
    "}" +
  "]" +
"}";
            try
            {
                var WhatappResponse = client.UploadString("https://api.myvaluefirst.com/psms/servlet/psms.JsonEservice", "POST", body);
                log = new CreateLogFiles();
                log.ErrorLog(" => WhatappMsg Response => " + WhatappResponse+"-"+ body.ToString());
            }
            catch (Exception ex)
            {
                log = new CreateLogFiles();
                log.ErrorLog(" => WhatappMsg Response => " + ex.Message);
            }
            return RR1;
        }
    }
}
