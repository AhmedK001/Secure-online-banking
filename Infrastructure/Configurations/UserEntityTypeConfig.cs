using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class UserEntityTypeConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Primary Key
        builder.HasKey(user => user.NationalId);
        //Required
        builder.Property(user => user.Email).IsRequired();
        builder.Property(user => user.NationalId).IsRequired();
        builder.Property(user => user.PhoneNumber).IsRequired();
        builder.Property(user => user.FullName).IsRequired();
        builder.Property(user => user.DateOfBirth).IsRequired();
    }
}