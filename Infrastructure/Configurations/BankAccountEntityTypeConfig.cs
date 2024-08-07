using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class BankAccountEntityTypeConfig : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        //Primary Key
        builder.HasKey(account => account.NationalId);
        
        //Required
        builder.Property(account => account.NationalId).IsRequired();
        builder.Property(account => account.Balance).IsRequired();
        builder.Property(account => account.CreationDate).IsRequired();
        builder.Property(account => account.AccountNumber).IsRequired();
    }
}