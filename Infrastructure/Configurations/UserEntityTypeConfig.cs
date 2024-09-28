using Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class UserEntityTypeConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // NationalId as a Primary key
        builder.HasKey(user => user.NationalId);
        
        // Indexing
        builder.HasIndex(user => user.NationalId).IsUnique();
        builder.HasIndex(user => user.PhoneNumber).IsUnique();
        builder.HasIndex(user => user.Email).IsUnique();
        
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
        
        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(256)
            .HasColumnType("varchar(256)");

        builder.Property(user => user.PhoneNumber)
            .IsRequired()
            .HasMaxLength(15)
            .HasColumnType("varchar(15)");

        builder.Property(user => user.DateOfBirth)
            .IsRequired()
            .HasColumnType("date");

        // One-to-one relationship with BankAccount (Optional)
        builder.HasOne(user => user.Account)
            .WithOne(account => account.User)
            .HasForeignKey<BankAccount>(account => account.UserId)
            .IsRequired(false);  // BankAccount is optional
    }
}
