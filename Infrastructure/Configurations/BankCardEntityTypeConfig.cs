using System.ComponentModel.DataAnnotations.Schema;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

[Table("Cards")]
public class BankCardEntityTypeConfig : IEntityTypeConfiguration<BankCard>
{
    public void Configure(EntityTypeBuilder<BankCard> builder)
    {
        // Primary Key
        builder.HasKey(bankCard => bankCard.CardId);
        
        // AccountNumber as a foreign-key
        builder.HasAlternateKey(card => card.AccountNumber);
        
        // Indexing for cardId, cardNumber, accountNumber
        builder.HasIndex(card => card.CardId);
        builder.HasIndex(card => card.CardNumber);
        builder.HasIndex(card => card.AccountNumber);
        
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
        
        // Relations With other tables

        // One-to-many relationship with Payments
        builder.HasMany(bankCard => bankCard.Payments)
            .WithOne(payment => payment.Card)
            //.HasForeignKey(payment => payment.CardId)
            .OnDelete(DeleteBehavior.Cascade);
        

    }
}