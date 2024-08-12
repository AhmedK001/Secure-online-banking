using System.ComponentModel.DataAnnotations;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class UserEntityTypeConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // NationalId as a Primary key
        builder.HasKey(user => user.NationalId);

        // One user could have many accounts
        builder.HasMany(user => user.Accounts).WithMany();

        // Properties
        builder.Property(user => user.NationalId)
            .IsRequired()
            .HasColumnType("int")
            .HasMaxLength(10);

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
        
        // Relations With other tables
        
        // One to one relationship with UserContactInfoTable
        builder.HasOne(user => user.UserContactInfo)
            .WithOne(contactInfo => contactInfo.User)
            .HasForeignKey<User>(user => user.NationalId);

    }
}