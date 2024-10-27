using System.ComponentModel.DataAnnotations.Schema;
using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

[Table("Cards")]
public class BankCardEntityTypeConfig : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        // Primary Key
        builder.HasKey(bankCard => bankCard.CardId);

        // Foreign key to BankAccount (BankAccount is now linked by navigation property)
        builder.HasOne(card => card.BankAccount)
            .WithMany(account => account.BankCards)
            .HasForeignKey("AccountNumber")
            .OnDelete(DeleteBehavior.Cascade); // Delete cards if the bank account is deleted

        // Required Properties
        builder.Property(bankCard => bankCard.CardId)
            .IsRequired()
            .ValueGeneratedNever(); // Specify if you're handling CardId manually

        builder.Property(bankCard => bankCard.Cvv)
            .IsRequired();

        builder.Property(bankCard => bankCard.ExpiryDate)
            .IsRequired()
            .HasColumnType("date");

        // Enum mapping properly (int or string)
        builder.Property(bankCard => bankCard.CardType)
            .IsRequired()
            .HasConversion<string>() // Storing as string in DB
            .HasMaxLength(20);

        builder.Property(bankCard => bankCard.Balance)
            .IsRequired()
            .HasColumnType("decimal(18, 2)");

        // initialize card currency as SAR if not selected.
        builder.Property(c => c.Currency).HasDefaultValue(EnumCurrency.SAR).HasConversion<int>();

        // Relationships with Payments
        builder.HasMany(bankCard => bankCard.Payments)
            .WithOne(payment => payment.Card)
            .HasForeignKey(payment => payment.CardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
