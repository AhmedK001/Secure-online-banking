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
    }
}