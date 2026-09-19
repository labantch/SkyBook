using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyBook.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Data.Configuration
{
    public class PaymentConfigration : IEntityTypeConfiguration<Payment>
    {
           public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.TransactionId)
                   .HasMaxLength(100);

            builder.Property(x => x.PaymentMethod)
                   .IsRequired();

            builder.Property(x => x.PaymentStatus)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            builder.HasOne(x => x.Booking)
                   .WithOne(x => x.Payment)
                   .HasForeignKey<Payment>(x => x.BookingId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}


