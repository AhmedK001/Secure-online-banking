using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class BankCardEntityTypeConfig : IEntityTypeConfiguration<BankCard>
{
    public void Configure(EntityTypeBuilder<BankCard> builder)
    {
        //PrimaryKey
        builder.HasKey(bankCard => bankCard.CardId);

        //Required
        builder.Property(bankCard => bankCard.AccountNumber).IsRequired();

        builder.Property(bankCard => bankCard.CardId).IsRequired();

        builder.Property(bankCard => bankCard.CardNumber).IsRequired();

        builder.Property(bankCard => bankCard.CVV).IsRequired();

        builder.Property(bankCard => bankCard.ExpiryDate).IsRequired();

        builder.Property(bankCard => bankCard.CardType).IsRequired();

        builder.Property(bankCard => bankCard.Balance).IsRequired();
    }
}