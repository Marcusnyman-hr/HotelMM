using System;
using System.Collections.Generic;
using System.Text;

namespace HotelMM
{
    class Room
    {
        public int RoomId { get; set; }
        public int BookingId { get; set; }
        public int RoomTypeId { get; set; }
        public int Occupants { get; set; }
        public int Discount { get; set; }
        public string GuestMessage { get; set; }
    }
}
