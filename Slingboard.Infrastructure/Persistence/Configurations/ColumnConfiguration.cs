using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Slingboard.Domain.Entities;

namespace Slingboard.Infrastructure.Persistence.Configurations;

public class ColumnConfiguration : IEntityTypeConfiguration<Column>
{
    public void Configure(EntityTypeBuilder<Column> builder)
    {
        builder.ToTable("Columns");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Order).IsRequired();
        builder.Property(c => c.Limit);
    }
}