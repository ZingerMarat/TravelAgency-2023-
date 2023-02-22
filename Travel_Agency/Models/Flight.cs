namespace Travel_Agency.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Flight")]
    public partial class Flight
    {
        public int? f_ID { get; set; }

        [Key]
        [StringLength(5)]
        public string Flight_Number { get; set; }

        [StringLength(40)]
        public string Dep_location { get; set; }

        [StringLength(40)]
        public string Dest_location { get; set; }

        public DateTime? Dep_date { get; set; }

        public DateTime? Arr_date { get; set; }

        public int? Max_seat { get; set; }

        public int? Seat_Price { get; set; }

        public int? Duration { get; set; }

        [StringLength(50)]
        public string c_Email { get; set; }

        public virtual FlightDetails FlightDetails { get; set; }
    }
}
