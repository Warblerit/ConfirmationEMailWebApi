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
        public string CheckinDate { get; set; }
        public string CheckoutDate { get; set; }
        public string CheckinTime { get; set; }
        public string CheckoutTime { get; set; }
        public bool LTIAPIFlag { get; set; }
        public bool QReserveFlag { get; set; }
        public string MobileNo { get; set; }
        public string WhatsAppMsg { get; set; }

    }
    public class WhatsappObj
    {
        public string Msg { get; set; }
        public string MobileNo { get; set; }
        public string BookingConfirmationPdf { get; set; }
        public string WhatsappFileName { get; set; }
        public string WhatsappPdfUrl { get; set; }

    }
    public class URLShort
    {
        public string longUrl { get; set; }
    }
    public class Whatsapp
    {
        public string Msg { get; set; }
        public string MobileNo { get; set; }

    }

    public class PoData
    {
        public string item_id { get; set; }
        public decimal rate { get; set; }
        public int quantity { get; set; }
        public string tax_id { get; set; }
        public string vendor_id { get; set; }
        public string purchaseorder_number { get; set; }
        public string reference_number { get; set; }
        public string date  { get; set; }
        public string delivery_date { get; set; }
        public int payment_terms  { get; set; }
        public string payment_terms_label { get; set; }
        public bool is_inclusive_tax { get; set; }
        public string notes { get; set; }
        public string terms { get; set; }
        public string Zoho_Branch_Id { get; set; }
    }

   public class LineItemDt
    {
        public string item_id { get; set; } 
        public decimal rate { get; set; } 
        public int quantity { get; set; } 
        public string tax_id { get; set; }
    }

    public class Purchaseorder
    {
        public string purchaseorder_id { get; set; }
    }

   public class PORootRes
    {
        public int code { get; set; }
        public string message { get; set; }
        public Purchaseorder purchaseorder { get; set; }
    }

    public class ZohoObj
    {
        public long BookingId { get; set; }
        public string BookingCode { get; set; }
        public string PropertyName { get; set; }
    }

}