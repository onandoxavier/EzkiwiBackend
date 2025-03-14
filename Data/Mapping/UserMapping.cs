using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Data.Mapping;

public class UserMapping : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Password).HasMaxLength(100).IsRequired();

        builder.HasOne(u => u.Company)
            .WithMany()
            .HasForeignKey(u => u.CompanyId);

        builder.ToTable("Users");
    }
}
