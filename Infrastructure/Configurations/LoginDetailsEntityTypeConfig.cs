using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class LoginDetailsEntityTypeConfig : IEntityTypeConfiguration<LoginDetails>
{
    public void Configure(EntityTypeBuilder<LoginDetails> builder)
    {
        // NationalId as a primary key
        builder.HasKey(loginDetails => loginDetails.NationalId);

        // Indexing for Email, PhoneNumber, NationalId
        builder.HasIndex(loginDetails => loginDetails.Email)
            .IsUnique();
        builder.HasIndex(loginDetails => loginDetails.PhoneNumber)
            .IsUnique();
        builder.HasIndex(loginDetails => loginDetails.NationalId);

        // Required Properties
        builder.Property(loginDetails => loginDetails.NationalId)
            .HasColumnType("int")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(loginDetails => loginDetails.Email)
            .HasColumnType("varchar(70)")
            .IsRequired();

        builder.Property(loginDetails => loginDetails.PhoneNumber)
            .IsRequired()
            .HasMaxLength(15)
            .HasColumnType("varchar(15)");

        builder.Property(loginDetails => loginDetails.Password)
            .IsRequired();

        // Relations with other tables
        builder.HasOne(loginDetails => loginDetails.BankAccount)
            .WithOne(bankAccount => bankAccount.LoginDetails)
            .HasForeignKey<BankAccount>(bankAccount => bankAccount.NationalId); // Ensure the foreign key is compatible
    }
}
