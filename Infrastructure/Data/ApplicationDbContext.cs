using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.Configurations;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext
{

    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Data Source=TUF;Initial Catalog=BankDatabase;Integrated Security=True;Trust Server Certificate=True");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        new UserEntityTypeConfig().Configure(modelBuilder.Entity<User>());
        new BankAccountEntityTypeConfig().Configure(modelBuilder.Entity<BankAccount>());
        new OperationEntityTypeConfig().Configure(modelBuilder.Entity<Operation>());
        new ReceiverClientEntityTypeConfig().Configure(modelBuilder.Entity<ReceiverClient>());
        new BankCardEntityTypeConfig().Configure(modelBuilder.Entity<BankCard>());
        new UserContactInfoEntityTypeConfig().Configure(modelBuilder.Entity<UserContactInfo>());
        new PaymentEntityTypeConfig().Configure(modelBuilder.Entity<Payment>());
        new LoginDetailsEntityTypeConfig().Configure(modelBuilder.Entity<LoginDetails>());
    }

    public DbSet<User> Users { get; set; }
    public DbSet<BankAccount> Accounts { get; set; }
    public DbSet<Operation> Operations { get; set; }
    public DbSet<BankCard> BankCards { get; set; }
    public DbSet<UserContactInfo> ContactInfos { get; set; }
    public DbSet<ReceiverClient> ReceiverClients { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<LoginDetails> LoginDetails { get; set; }
}