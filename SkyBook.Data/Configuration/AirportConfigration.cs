using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyBook.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Data.Configuration
{
    public class AirportConfiguration : IEntityTypeConfiguration<Airport>
    {
        public void Configure(EntityTypeBuilder<Airport> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Code)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.HasIndex(x => x.Code)
                   .IsUnique();

            builder.Property(x => x.City)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Country)
                   .IsRequired()
                   .HasMaxLength(100);
        }
    }
}
