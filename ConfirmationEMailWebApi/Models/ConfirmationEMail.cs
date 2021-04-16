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
        public bool ChatAPIFlag { get; set; }
        public bool IciciAPIFlag { get; set; }

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
        public string date { get; set; }
        public string delivery_date { get; set; }
        public int payment_terms { get; set; }
        public string payment_terms_label { get; set; }
        public bool is_inclusive_tax { get; set; }
        public string notes { get; set; }
        public string terms { get; set; }
        public string Zoho_Branch_Id { get; set; }
        public int RoomCaptured { get; set; }
    }

    public class LineItemDt
    {
        public string item_id { get; set; }
        public decimal rate { get; set; }
        public int quantity { get; set; }
        public string tax_id { get; set; }
        public int item_order { get; set; }
    }

    public class Purchaseorder
    {
        public string purchaseorder_id { get; set; }
        public List<LineItem> line_items { get; set; }
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

    public class LineItem
    {
        public string line_item_id { get; set; }
        public int item_order { get; set; }
        public string item_id { get; set; }
    }

    public class ZohoPropertyDtls
    {
        public string PropertyName { get; set; }
        public string LegalName { get; set; }
        public string LegalAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postal { get; set; }
        public int CreditPeriod { get; set; }
        public string GSTNumber { get; set; }
        public Int64 PropertyId { get; set; }

    }
    public class RootObjNew
    {
        public string Code { get; set; }
        public string EmailResponse { get; set; }
    }

    public class Item
    {
        public string item_id { get; set; }
        public string name { get; set; }

    }

    public class ItemRootRes
    {
        public int code { get; set; }
        public string message { get; set; }
        public Item item { get; set; }
    }


    public class PropertyDtls
    {
        public long TrackingNo { get; set; }
        public string FirstName { get; set; }
        public string BookedDt { get; set; }
        public string PropertyName { get; set; }
        public string Deskno { get; set; }
        public string HBLogo { get; set; }
        public string UserCode { get; set; }
        public long BookingId { get; set; }
        public long StateId { get; set; }
        public string RowId { get; set; }
        public string PropertyType { get; set; }
        public string Email { get; set; }
        public string FromEmail { get; set; }
        public string ClientName { get; set; }
        public int singlecount { get; set; }
        public int doublecount { get; set; }
        public int triplecount { get; set; }
        public string Client_RequestNo { get; set; }
        public string CityName { get; set; }
        public string Notes { get; set; }
        public string Inclusions { get; set; }
        public string HBAddress { get; set; }
        public string ClientEmail { get; set; }
        public string FilePath { get; set; }
    

    }

    public class GuestDtls
    {
        public string Name { get; set; }
        public string MobileNO { get; set; }
        public string ChkInDt { get; set; }
        public string ChkOutDt { get; set; }
        public string RoomNo { get; set; }
        public decimal Tariff { get; set; }
        public string Inclu { get; set; }
        public decimal TACDetails { get; set; }
        public string TACInclu { get; set; }
        public int TACExecption { get; set; }
        public string Occupancy { get; set; }
        public string TariffPayMentMode { get; set; }
        public bool TAC { get; set; }
    }

    public class ClientGSTDtls
    {
    public string ClientName { get; set; }
    public string GSTNumber { get; set; }
    public string ClientAddress { get; set; }
   }

    public class HBGSTDtls
    {
        public string HBName { get; set; }
        public string HBGSTNumber { get; set; }
        public string HBAddress { get; set; }
    }

    public class GuestDtls01
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string GuestEmail { get; set; }
    }

} 