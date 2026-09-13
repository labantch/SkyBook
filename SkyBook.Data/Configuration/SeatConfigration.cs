using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyBook.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Data.Configuration
{
    public class SeatConfiguration : IEntityTypeConfiguration<Seat>
    {
        public void Configure(EntityTypeBuilder<Seat> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SeatNumber)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.HasIndex(x => new
            {
                x.AircraftId,
                x.SeatNumber
            }).IsUnique();

            builder.HasOne(x => x.Aircraft)
                   .WithMany(x => x.Seats)
                   .HasForeignKey(x => x.AircraftId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
