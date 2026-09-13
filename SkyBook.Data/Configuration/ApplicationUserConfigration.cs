using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkyBook.Data.Models;
namespace SkyBook.Data.Configuration;
public class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.FullName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Country)
               .IsRequired()
               .HasMaxLength(100);
        builder.Property(x => x.ImageUrl)
       .HasMaxLength(500);

        builder.HasMany(x => x.Bookings)
               .WithOne(x => x.User)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}