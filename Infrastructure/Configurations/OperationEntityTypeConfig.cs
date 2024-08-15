using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class OperationEntityTypeConfig : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        // Primary Key
        builder.HasKey(operation => operation.OperationId);

        // AccountNumber, AccountId as a foreign-keys
        builder.HasAlternateKey(operation => operation.AccountNumber);
        builder.HasAlternateKey(operation => operation.AccountId);

        // Indexing for OperationId, AccountId, AccountNumber
        builder.HasIndex(operation => operation.OperationId);
        builder.HasIndex(operation => operation.AccountId);
        builder.HasIndex(operation => operation.AccountNumber);

        // Required Properties
        builder.Property(operation => operation.OperationId)
            .IsRequired()
            .HasColumnType("int");

        builder.Property(operation => operation.AccountId)
            .IsRequired()
            .HasColumnType("int"); 
            
        builder.Property(operation => operation.AccountNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnType("varchar(20)"); 
            
        builder.Property(operation => operation.DateTime)
            .IsRequired()
            .HasColumnType("datetime");
            
        builder.Property(operation => operation.OperationType)
            .IsRequired()
            .HasMaxLength(50) 
            .HasColumnType("varchar(50)");
            
        builder.Property(operation => operation.Amount)
            .IsRequired()
            .HasColumnType("decimal(18, 2)");
        
        // Relations With other tables
        
        // One-to-one relationship with Receiver
        builder.HasOne(operation => operation.Receiver)
            .WithOne(receiver => receiver.Operation)
            .HasForeignKey<Operation>(operation => operation.OperationId)
            .IsRequired(false);
    }
}