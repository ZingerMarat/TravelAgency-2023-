namespace Travel_Agency.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PaymentInfo")]
    public partial class PaymentInfo
    {
        [Key]
        [Required]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "ID must be 9 digits")]
        public string payer_id { get; set; }

        [StringLength(255)]
        [Required]
        [RegularExpression(@"^[a-zA-Z]{2,20} [a-zA-Z]{2,20}$", ErrorMessage = "Name must be a full name")]
        public string payer_name { get; set; }

        [StringLength(255)]
        [Required]
        [RegularExpression(@"^[0-9]{16}$", ErrorMessage = "Card number must be 16 digits")]
        public string credit_card_number { get; set; }

        [StringLength(50)]
        public string c_email { get; set; }
        [Required]
        [StringLength(3)]
        [RegularExpression(@"^[0-9]{3}$", ErrorMessage = "CVV must be 3 digits")]
        public string cvv { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
