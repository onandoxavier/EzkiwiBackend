using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Data.Mapping;

public class PasswordResetFlowMapping : IEntityTypeConfiguration<PasswordResetFlow>
{
    public void Configure(EntityTypeBuilder<PasswordResetFlow> builder)
    {
        builder.HasKey(u => u.Id);
                    
        builder.Property(u => u.CodeExpiration).IsRequired();
        builder.Property(u => u.CodeHash).HasMaxLength(64).IsRequired();

        builder.Property(u => u.Ip).HasMaxLength(50).IsRequired();
        builder.Property(u => u.UserAgent).HasMaxLength(50).IsRequired();

        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId);

        builder.ToTable("PasswordResetFlows");
    }
}
