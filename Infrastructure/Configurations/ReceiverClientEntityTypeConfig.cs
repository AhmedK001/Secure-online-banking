using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure;

public class ReceiverClientEntityTypeConfig : IEntityTypeConfiguration<ReceiverClient>
{
    public void Configure(EntityTypeBuilder<ReceiverClient> builder)
    {
        //PrimaryKey
        builder.HasKey(receiverClient => receiverClient.AccountNumber);
        //Required
        builder.Property(receiverClient => receiverClient.AccountNumber).IsRequired();

        builder.Property(receiverClient => receiverClient.FullName).IsRequired();
    }
}