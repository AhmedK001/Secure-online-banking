using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class BankAccountEntityTypeConfig : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        // Primary Key
        builder.HasKey(account => account.NationalId);
        
        // NationalId as a foreign-key 
        builder.HasAlternateKey(account => account.NationalId);
        
        // Indexing for NationalId, AccountNumber
        builder.HasIndex(account => account.NationalId);
        builder.HasIndex(account => account.AccountNumber);
        
        // Properties
        builder.Property(account => account.NationalId)
            .IsRequired()
            .HasColumnType("int");
            
        builder.Property(account => account.AccountNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnType("varchar(20)");
            
        builder.Property(account => account.Password)
            .IsRequired()
            .HasMaxLength(64)
            .HasColumnType("varchar(64)");
            
        builder.Property(account => account.CreationDate)
            .IsRequired()
            .HasColumnType("datetime");
            
        builder.Property(account => account.Balance)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnType("decimal(18, 2)");
        
        // Relations With other tables

        // One-to-many relationship with BankCardsTable
        builder.HasMany(account => account.BankCards)
            .WithOne(bankCard => bankCard.BankAccount)
            .IsRequired(false);
        
    }
}