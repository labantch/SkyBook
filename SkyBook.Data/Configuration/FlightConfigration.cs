using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyBook.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Data.Configuration
{
    public class FlightConfiguration : IEntityTypeConfiguration<Flight>
    {
        public void Configure(EntityTypeBuilder<Flight> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FlightNumber)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.HasIndex(x => x.FlightNumber)
                   .IsUnique();

            builder.Property(x => x.Price)
                   .HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.DepartureAirport)
                   .WithMany(x => x.DepartingFlights)
                   .HasForeignKey(x => x.DepartureAirportId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ArrivalAirport)
                   .WithMany(x => x.ArrivingFlights)
                   .HasForeignKey(x => x.ArrivalAirportId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Aircraft)
                   .WithMany(x => x.Flights)
                   .HasForeignKey(x => x.AircraftId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
