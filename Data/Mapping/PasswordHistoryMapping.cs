using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Data.Mapping;

public class PasswordHistoryMapping : IEntityTypeConfiguration<PasswordHistory>
{
    public void Configure(EntityTypeBuilder<PasswordHistory> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Value).HasMaxLength(30).IsRequired();
        builder.HasOne(c => c.Queue)
            .WithMany(q => q.PasswordHistories)
            .HasForeignKey(c => c.QueueId);

        builder.ToTable("PasswordHistories");
    }
}
