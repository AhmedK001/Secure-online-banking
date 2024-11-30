using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StockEntityTypeConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        // primary key for Stock
        builder.HasKey(s => s.StockId);

        builder.Property(s => s.StockId)
            .ValueGeneratedNever();

        // Configure properties
        builder.Property(s => s.StockName)
            .IsRequired()
            .HasMaxLength(90);

        builder.Property(s => s.StockSymbol)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(s => s.StockPrice).HasColumnType("decimal(18, 2)");

        builder.Property(s => s.NumberOfStocks)
            .IsRequired();

        builder.Property(s => s.DateOfPurchase)
            .IsRequired();

        builder.Property(s => s.Currency)
            .IsRequired();

        // Configure relationships
        builder.HasOne(s => s.BankAccount)
            .WithMany(b => b.Stocks)
            .HasForeignKey(s => s.AccountNumber)
            .HasConstraintName("FK_Stocks_BankAccount")
            .OnDelete(DeleteBehavior.Cascade);
    }
}