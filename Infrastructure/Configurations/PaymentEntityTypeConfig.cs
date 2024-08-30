using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class PaymentEntityTypeConfig : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        // PaymentId as a primary-key
        builder.HasKey(payment => payment.PaymentId);

        // CardId as a foreign-key
        //builder.HasAlternateKey(payment => payment.CardId);

        // Indexing for PaymentId, CardId
        builder.HasIndex(payment => payment.PaymentId);
        builder.HasIndex(payment => payment.CardId);

        // Required Properties
        builder.Property(payment => payment.CardId)
            .IsRequired()
            .HasColumnType("int");

        builder.Property(payment => payment.PaymentId).IsRequired()
            .HasColumnType("int");
        
        builder.Property(payment => payment.Amount).IsRequired()
            .HasColumnType("decimal(18,2)");
        
        builder.Property(payment => payment.DateTime).IsRequired()
            .HasColumnType("datetime");
    }
}