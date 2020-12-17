using System;
using System.Collections.Generic;
using System.Text;

namespace HotelMM
{
    class ResolvedInvoice
    {
        public int ResolvedInvoiceId { get; set; }
        public int InvoiceNumberId { get; set; }
        public int RoomId { get; set; }
        public int TypeId { get; set; }
        public int Amount { get; set; }
        public string DateResolved { get; set; }
    }
}
