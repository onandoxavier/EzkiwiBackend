using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using VirtualQueueApi.Domain.Entities;

namespace VirtualQueueApi.Data.Mapping;

public class SubscriptionManagementMapping : IEntityTypeConfiguration<SubscriptionManagement>
{
    public void Configure(EntityTypeBuilder<SubscriptionManagement> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.SubscriptionId).HasMaxLength(100);
        builder.Property(c => c.SessionId).HasMaxLength(100);
        builder.Property(c => c.InvoiceId).HasMaxLength(100);

        builder.HasOne(u => u.Company)
            .WithMany()
            .HasForeignKey(u => u.CompanyId);

        builder.ToTable("SubscriptionManagements");
    }
}
