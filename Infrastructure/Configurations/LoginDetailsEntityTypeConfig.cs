using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class LoginDetailsEntityTypeConfig : IEntityTypeConfiguration<LoginDetails>
{
    public void Configure(EntityTypeBuilder<LoginDetails> builder)
    {
        // NationalId as a primary-key
        builder.HasKey(LoginDetails => LoginDetails.NationalId);

        // Indexing for Email, PhoneNumber, NationalId
        builder.HasIndex(LoginDetails => LoginDetails.Email)
            .IsUnique();
        builder.HasIndex(LoginDetails => LoginDetails.PhoneNumber)
            .IsUnique();
        builder.HasIndex(LoginDetails => LoginDetails.NationalId);
        
        // Required Properties
        builder.Property( LoginDetails => LoginDetails.NationalId)
            .HasColumnType("int").HasMaxLength(10)
            .IsRequired();
        
        builder.Property(LoginDetails => LoginDetails.Email)
            .HasColumnType("varchar(70)")
            .IsRequired();
        
        builder.Property(LoginDetails => LoginDetails.PhoneNumber)
            .IsRequired()
            .HasMaxLength(15)
            .HasColumnType("varchar(15)");
        
        builder.Property(LoginDetails => LoginDetails.Password)
            .IsRequired();

    }
}