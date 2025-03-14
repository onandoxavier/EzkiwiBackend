using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualQueueApi.Domain.Entities;

namespace VirtualQueueApi.Data.Mapping;

public class RefreshTokenMapping : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.TokenHash).HasMaxLength(200).IsRequired();

        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId);

        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder.ToTable("RefreshTokens");
    }
}

