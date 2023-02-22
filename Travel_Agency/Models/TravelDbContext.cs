using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Travel_Agency.Models
{
    public partial class TravelDbContext : DbContext
    {
        public TravelDbContext()
            : base("name=TravelDbContext1")
        {
        }

        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<Flight> Flight { get; set; }
        public virtual DbSet<FlightDetails> FlightDetails { get; set; }
        public virtual DbSet<Booking> Booking { get; set; }
        public virtual DbSet<PaymentInfo> PaymentInfo { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
                .Property(e => e.c_FirstName)
                .IsUnicode(false);

            modelBuilder.Entity<Customer>()
                .Property(e => e.c_LastName)
                .IsUnicode(false);

            modelBuilder.Entity<Customer>()
                .Property(e => e.c_password)
                .IsUnicode(false);

            modelBuilder.Entity<Customer>()
                .Property(e => e.c_Email)
                .IsUnicode(false);

            modelBuilder.Entity<Flight>()
                .Property(e => e.Flight_Number)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Flight>()
                .Property(e => e.Dep_location)
                .IsUnicode(false);

            modelBuilder.Entity<Flight>()
                .Property(e => e.Dest_location)
                .IsUnicode(false);

            modelBuilder.Entity<Flight>()
                .Property(e => e.c_Email)
                .IsUnicode(false);

            modelBuilder.Entity<Flight>()
                .HasOptional(e => e.FlightDetails)
                .WithRequired(e => e.Flight)
                .WillCascadeOnDelete();

            modelBuilder.Entity<FlightDetails>()
                .Property(e => e.Flight_Number)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<FlightDetails>()
                .HasMany(e => e.Booking)
                .WithRequired(e => e.FlightDetails)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Booking>()
                .Property(e => e.c_Email)
                .IsUnicode(false);

            modelBuilder.Entity<Booking>()
                .Property(e => e.Flight_Number)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<PaymentInfo>()
                .Property(e => e.payer_id)
                .IsUnicode(false);

            modelBuilder.Entity<PaymentInfo>()
                .Property(e => e.payer_name)
                .IsUnicode(false);

            modelBuilder.Entity<PaymentInfo>()
                .Property(e => e.credit_card_number)
                .IsUnicode(false);

            modelBuilder.Entity<PaymentInfo>()
                .Property(e => e.c_email)
                .IsUnicode(false);

            modelBuilder.Entity<PaymentInfo>()
                .Property(e => e.cvv)
                .IsUnicode(false);
        }
    }
}
