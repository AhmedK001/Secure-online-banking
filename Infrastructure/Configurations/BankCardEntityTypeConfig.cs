using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class BankCardEntityTypeConfig : IEntityTypeConfiguration<BankCard>
{
    public void Configure(EntityTypeBuilder<BankCard> builder)
    {
        // Primary Key
        builder.HasKey(bankCard => bankCard.CardId);

        // Required Properties
        builder.Property(bankCard => bankCard.CardId)
            .IsRequired()
            .HasColumnType("int");

        builder.Property(bankCard => bankCard.AccountNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnType("varchar(20)"); 
            
        builder.Property(bankCard => bankCard.CardNumber)
            .IsRequired()
            .HasMaxLength(16) 
            .HasColumnType("varchar(16)"); 

        builder.Property(bankCard => bankCard.CVV)
            .IsRequired()
            .HasMaxLength(4)
            .HasColumnType("varchar(4)");
            
        builder.Property(bankCard => bankCard.ExpiryDate)
            .IsRequired()
            .HasColumnType("date"); 

        builder.Property(bankCard => bankCard.CardType)
            .IsRequired()
            .HasColumnType("varchar(10)"); 

        builder.Property(bankCard => bankCard.Balance)
            .IsRequired()
            .HasColumnType("decimal(18, 2)"); 
    }
}