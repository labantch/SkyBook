using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyBook.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Data.Configuration
{

    public class PassengerConfiguration : IEntityTypeConfiguration<Passenger>
    {
        public void Configure(EntityTypeBuilder<Passenger> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FirstName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.LastName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.DateOfBirth)
                   .IsRequired();

            builder.Property(x => x.PassportNumber)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.HasIndex(x => x.PassportNumber)
                   .IsUnique();

            builder.Property(x => x.Nationality)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasMany(x => x.Bookings)
                   .WithOne(x => x.Passenger)
                   .HasForeignKey(x => x.PassengerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
