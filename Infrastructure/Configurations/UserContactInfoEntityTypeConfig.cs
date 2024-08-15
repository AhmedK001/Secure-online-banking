﻿using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class UserContactInfoEntityTypeConfig : IEntityTypeConfiguration<UserContactInfo>
{
    public void Configure(EntityTypeBuilder<UserContactInfo> builder)
    {
        // NationalId as a Primary key
        builder.HasKey(user => user.NationalId);

        // NationalId as a foreign key
        builder.HasAlternateKey(user => user.NationalId);

        // Indexing for phoneNumber, Email
        builder.HasIndex(user => user.PhoneNumber)
            .IsUnique();

        builder.HasIndex(user => user.Email)
            .IsUnique();
        
        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(256)
            .HasColumnType("varchar(256)");
        
        builder.Property(user => user.PhoneNumber)
            .IsRequired()
            .HasMaxLength(15)
            .HasColumnType("varchar(15)");
    }
}