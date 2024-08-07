﻿using Microsoft.EntityFrameworkCore;
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
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SecureBankDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        new UserEntityTypeConfig().Configure(modelBuilder.Entity<User>());
        new BankAccountEntityTypeConfig().Configure(modelBuilder.Entity<BankAccount>());
        new OperationEntityTypeConfig().Configure(modelBuilder.Entity<Operation>());
        new ReceiverClientEntityTypeConfig().Configure(modelBuilder.Entity<ReceiverClient>());
        new BankCardEntityTypeConfig().Configure(modelBuilder.Entity<BankCard>());
    }

    public DbSet<User> Users { get; set; }
    public DbSet<BankAccount> Accounts { get; set; }
    public DbSet<Operation> Operations { get; set; }
    public DbSet<BankCard> BankCards { get; set; }
    public DbSet<ReceiverClient> ReceiverClients { get; set; }
}