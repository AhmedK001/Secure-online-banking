﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User,Role,Guid>
{

    private readonly IConfiguration _configuration;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_configuration["DbContextAzure"]);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        new BankAccountEntityTypeConfig().Configure(modelBuilder.Entity<BankAccount>());
        new OperationEntityTypeConfig().Configure(modelBuilder.Entity<Operation>());
        new ReceiverClientEntityTypeConfig().Configure(modelBuilder.Entity<ReceiverClient>());
        new BankCardEntityTypeConfig().Configure(modelBuilder.Entity<Card>());
        new PaymentEntityTypeConfig().Configure(modelBuilder.Entity<Payment>());
        new StockEntityTypeConfiguration().Configure(modelBuilder.Entity<Stock>());
    }

    public DbSet<BankAccount> Accounts { get; set; }
    public DbSet<Operation> Operations { get; set; }
    public DbSet<Card> BankCards { get; set; }
    public DbSet<ReceiverClient> ReceiverClients { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Stock> Stocks { get; set; }
}