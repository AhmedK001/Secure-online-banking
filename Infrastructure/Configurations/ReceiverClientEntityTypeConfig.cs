using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure;

public class ReceiverClientEntityTypeConfig : IEntityTypeConfiguration<ReceiverClient>
{
    public void Configure(EntityTypeBuilder<ReceiverClient> builder)
    {
        // Primary Key
        builder.HasKey(receiverClient => receiverClient.OperationId);

        // OperationId as a foreign-key
        builder.HasAlternateKey(client => client.OperationId);

        // Indexing for OperationId, AccountNumber
        builder.HasIndex(client => client.OperationId);
        builder.HasIndex(client => client.AccountNumber);

        // Required Properties
        builder.Property(receiverClient => receiverClient.AccountNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnType("varchar(20)");
            
        builder.Property(receiverClient => receiverClient.FullName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");
    }
}