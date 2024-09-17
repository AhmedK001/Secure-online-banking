using Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class UserEntityTypeConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // NationalId as a Primary key
        builder.HasKey(user => user.NationalId);

        // Properties
        builder.Property(user => user.NationalId)
            .IsRequired()
            .HasColumnType("int")
            .HasMaxLength(10)
            .ValueGeneratedNever();

        builder.Property(user => user.FirstName)
            .IsRequired()
            .HasMaxLength(40)
            .HasColumnType("varchar(100)");

        builder.Property(user => user.LastName)
            .IsRequired()
            .HasMaxLength(40)
            .HasColumnType("varchar(100)");

        builder.Property(user => user.DateOfBirth)
            .IsRequired()
            .HasColumnType("date");

        // One-to-one relationship with UserContactInfo (Required)
        builder.HasOne(user => user.UserContactInfo)
            .WithOne(contactInfo => contactInfo.User)
            .HasForeignKey<UserContactInfo>(contactInfo => contactInfo.NationalId)
            .IsRequired();  // UserContactInfo is required

        // One-to-one relationship with BankAccount (Optional)
        builder.HasOne(user => user.Account)
            .WithOne(account => account.User)
            .HasForeignKey<BankAccount>(account => account.NationalId)
            .IsRequired(false);  // BankAccount is optional
    }
}
