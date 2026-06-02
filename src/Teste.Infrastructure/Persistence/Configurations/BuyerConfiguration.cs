using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Teste.Domain.Entities;

namespace Teste.Infrastructure.Persistence.Configurations;

public sealed class BuyerConfiguration : IEntityTypeConfiguration<Buyer>
{
    public void Configure(EntityTypeBuilder<Buyer> builder)
    {
        builder.ToTable("Buyers");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(b => b.Email)
            .IsUnique();

        builder.Property(b => b.CreatedAt)
            .IsRequired();
    }
}
