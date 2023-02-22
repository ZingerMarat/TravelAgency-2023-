namespace Travel_Agency.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Booking")]
    public partial class Booking
    {
        public int? b_Price { get; set; }

        public int? numSeats { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string c_Email { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(5)]
        public string Flight_Number { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Dep_date { get; set; }

        public virtual FlightDetails FlightDetails { get; set; }
    }
}
