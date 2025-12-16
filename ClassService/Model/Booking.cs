using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace ClassService.Model
{
    public class Booking
    {
        public string UserId { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public DateTime CheckedInAt { get; set; } = DateTime.MinValue;
    }

}