namespace Travel_Agency.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class FlightDetails
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FlightDetails()
        {
            Booking = new HashSet<Booking>();
        }

        public int? fd_ID { get; set; }

        [Key]
        [StringLength(5)]
        public string Flight_Number { get; set; }

        public DateTime? Dep_date { get; set; }

        public DateTime? Arr_date { get; set; }

        public int? Price { get; set; }

        public int? Available_Seats { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Booking> Booking { get; set; }

        public virtual Flight Flight { get; set; }
    }
}
