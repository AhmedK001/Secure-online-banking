using Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class BankAccountEntityTypeConfig : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        // Primary Key
        builder.HasKey(account => account.AccountNumber); // AccountNumber is the primary key

        // Indexing for NationalId, AccountNumber
        builder.HasIndex(account => account.NationalId);
        builder.HasIndex(account => account.AccountNumber).IsUnique();

        // Properties
        builder.Property(account => account.NationalId)
            .IsRequired()
            .HasColumnType("int");

        builder.Property(account => account.AccountNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnType("varchar(20)");

        builder.Property(account => account.CreationDate)
            .IsRequired()
            .HasColumnType("datetime");

        builder.Property(account => account.Balance)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnType("decimal(18, 2)");

        // Relations with other tables
        builder.HasOne(account => account.User)
       .WithOne(user => user.Account)
       .HasForeignKey<BankAccount>(account => account.NationalId)
       .IsRequired(false); // Ensure this relationship is optional

        builder.HasOne(account => account.LoginDetails)
            .WithOne(loginDetails => loginDetails.BankAccount)
            .HasForeignKey<LoginDetails>(loginDetails => loginDetails.NationalId);
    }
}
