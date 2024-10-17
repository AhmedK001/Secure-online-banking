using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class
    ReceiverClientEntityTypeConfig : IEntityTypeConfiguration<ReceiverClient>
{
    public void Configure(EntityTypeBuilder<ReceiverClient> builder)
    {
        // Primary Key
        builder.HasKey(receiverClient => receiverClient.OperationId);

        // Indexing for OperationId, AccountNumber, ReceiverId
        builder.HasIndex(receiverClient => receiverClient.OperationId);
        builder.HasIndex(receiverClient => receiverClient.ReceiverAccountNumber);
        builder.HasIndex(receiverClient => receiverClient.ReceiverId);

        // Required Properties
        builder.Property(receiverClient => receiverClient.OperationId)
            .IsRequired();

        builder.Property(receiverClient => receiverClient.ReceiverAccountNumber)
            .IsRequired().HasMaxLength(20).HasColumnType("varchar(20)");

        builder.Property(receiver => receiver.ReceivedAmount).IsRequired()
            .HasColumnType("decimal(18, 2)");

        builder.Property(receiverClient => receiverClient.FullName).IsRequired()
            .HasMaxLength(100).HasColumnType("varchar(100)");
    }
}