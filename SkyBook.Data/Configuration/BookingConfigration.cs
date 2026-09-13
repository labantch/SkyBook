using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyBook.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Data.Configuration
{

    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.BookingReference)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasIndex(x => x.BookingReference)
                   .IsUnique();

            builder.Property(x => x.TotalPrice)
                   .HasColumnType("decimal(18,2)");

            builder.Property(x => x.BookingDate)
                   .IsRequired();

            builder.Property(x => x.Status)
                   .IsRequired();

            builder.HasOne(x => x.User)
                   .WithMany(x => x.Bookings)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Passenger)
                   .WithMany(x => x.Bookings)
                   .HasForeignKey(x => x.PassengerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Flight)
                   .WithMany(x => x.Bookings)
                   .HasForeignKey(x => x.FlightId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Seat)
                   .WithMany(x => x.Bookings)
                   .HasForeignKey(x => x.SeatId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
