namespace Travel_Agency.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Customer")]
    public partial class Customer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Customer()
        {
            PaymentInfo = new HashSet<PaymentInfo>();
        }

        public int? c_ID { get; set; }

        [StringLength(20)]
        [Required]
        //validate c_FirstName is between 2 and 20 characters
        [RegularExpression(@"^[a-zA-Z]{2,20}$", ErrorMessage = "First name must be between 2 and 20 characters")]
        public string c_FirstName { get; set; }

        [StringLength(20)]
        [Required]
        //validate c_LastName is between 2 and 20 characters
        [RegularExpression(@"^[a-zA-Z]{2,20}$", ErrorMessage = "Last name must be between 2 and 20 characters")]
        public string c_LastName { get; set; }

        [StringLength(15)]
        [Required]
        //validate password is between 8-15 characters
        [RegularExpression(@"^[a-zA-Z0-9]{8,15}$", ErrorMessage = "Password must be between 8 and 15 characters")]
        public string c_password { get; set; }

        [Key]
        [StringLength(50)]
        [Required]
        //validate email is valid
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Email is not valid")]
        public string c_Email { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PaymentInfo> PaymentInfo { get; set; }
    }
}
