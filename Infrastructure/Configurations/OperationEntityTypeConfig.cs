using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class OperationEntityTypeConfig : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        //PrimaryKey
        builder.HasKey(operation => operation.OperationId);

        //Required
        builder.Property(operation => operation.AccountId).IsRequired();

        builder.Property(operation => operation.AccountNumber).IsRequired();

        builder.Property(operation => operation.OperationId).IsRequired();

        builder.Property(operation => operation.DateTime).IsRequired();

        builder.Property(operation => operation.OperationType).IsRequired();

        builder.Property(operation => operation.Amount).IsRequired();
    }
}