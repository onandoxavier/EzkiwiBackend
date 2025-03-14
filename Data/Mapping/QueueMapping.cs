using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Data.Mapping;

public class QueueMapping : IEntityTypeConfiguration<Queue>
{
    public void Configure(EntityTypeBuilder<Queue> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();

        builder.HasOne(u => u.Company)
            .WithMany()
            .HasForeignKey(u => u.CompanyId);

        builder.ToTable("Queues");
    }
}
