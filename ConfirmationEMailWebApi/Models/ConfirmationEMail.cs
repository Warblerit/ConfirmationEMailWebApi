using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfirmationEMailWebApi.Models
{
    public class ConfirmationEMail
    {
        public long BookingId { get; set; }
        public bool GuestMailChk { get; set; }
        public bool PropertyMailChk { get; set; }
        public bool SmsChk { get; set; }
        public string CityCode { get; set; } 
        public string Bookingcode { get; set; }
        public string RowId { get; set; } 
        public long? Caretaker { get; set; }
        public string ShortPath { get; set; }
        public bool ResendFlag { get; set; }
        public string PropertyGusetEmail { get; set; }
        public string UserEmail { get; set; }
    }
    public class URLShort
    {
        public string longUrl { get; set; }
    }
}