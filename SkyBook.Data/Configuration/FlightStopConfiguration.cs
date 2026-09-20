using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyBook.Data.Models;

namespace SkyBook.Data.Configuration
{
    public class FlightStopConfiguration : IEntityTypeConfiguration<FlightStop>
    {
        public void Configure(EntityTypeBuilder<FlightStop> builder)
        {
            builder.ToTable("FlightStops");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StopOrder)
                   .IsRequired();

            builder.Property(x => x.Remarks)
                   .HasMaxLength(250);

            builder.HasOne(x => x.Flight)
                   .WithMany(x => x.Stops)
                   .HasForeignKey(x => x.FlightId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Airport)
                   .WithMany()
                   .HasForeignKey(x => x.AirportId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
