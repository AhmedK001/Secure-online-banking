using Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class UserContactInfoEntityTypeConfig : IEntityTypeConfiguration<UserContactInfo>
{
    public void Configure(EntityTypeBuilder<UserContactInfo> builder)
    {
        // NationalId as a Primary key
        builder.HasKey(contactInfo => contactInfo.NationalId);

        // NationalId as a foreign key to User
        builder.HasOne(contactInfo => contactInfo.User)
            .WithOne(user => user.UserContactInfo)
            .HasForeignKey<UserContactInfo>(contactInfo => contactInfo.NationalId);

        // Indexing for PhoneNumber, Email
        builder.HasIndex(contactInfo => contactInfo.PhoneNumber).IsUnique();
        builder.HasIndex(contactInfo => contactInfo.Email).IsUnique();

        // Properties
        builder.Property(contactInfo => contactInfo.Email)
            .IsRequired()
            .HasMaxLength(256)
            .HasColumnType("varchar(256)");

        builder.Property(contactInfo => contactInfo.PhoneNumber)
            .IsRequired()
            .HasMaxLength(15)
            .HasColumnType("varchar(15)");
    }
}
