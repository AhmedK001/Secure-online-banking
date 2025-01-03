﻿using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OperationEntityTypeConfig : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        // Primary Key
        builder.HasKey(operation => operation.OperationId);

        // Indexing
        builder.HasIndex(operation => operation.OperationId);

        // Required Properties
        builder.Property(operation => operation.OperationId)
            .IsRequired()
            .HasColumnType("int")
            .ValueGeneratedNever();

        builder.Property(operation => operation.AccountNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnType("varchar(20)");

        builder.Property(operation => operation.DateTime)
            .IsRequired()
            .HasColumnType("datetime");

        builder.Property(operation => operation.OperationType)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        builder.Property(operation => operation.Amount)
            .IsRequired()
            .HasColumnType("decimal(18, 2)");

        // relationship with ReceiverClient (Nullable)
        builder.HasOne(operation => operation.Receiver)
            .WithOne(receiver => receiver.Operation)
            .HasForeignKey<ReceiverClient>(receiver => receiver.OperationId)
            .IsRequired(false);
    }
}