﻿// <auto-generated />
using System;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240830222936_InitialDbStructure")]
    partial class InitialDbStructure
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Core.Entities.BankAccount", b =>
                {
                    b.Property<int>("NationalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("NationalId"));

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<decimal>("Balance")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(18, 2)")
                        .HasDefaultValue(0m);

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime");

                    b.HasKey("NationalId");

                    b.HasIndex("AccountNumber");

                    b.HasIndex("NationalId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("Core.Entities.BankCard", b =>
                {
                    b.Property<int>("CardId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CardId"));

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<int>("BankAccountNationalId")
                        .HasColumnType("int");

                    b.Property<string>("CVV")
                        .IsRequired()
                        .HasMaxLength(4)
                        .HasColumnType("varchar(4)");

                    b.Property<string>("CardNumber")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("varchar(16)");

                    b.Property<string>("CardType")
                        .IsRequired()
                        .HasColumnType("varchar(10)");

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("date");

                    b.Property<bool>("OpenedForOnlinePurchase")
                        .HasColumnType("bit");

                    b.Property<bool>("OpenedForPhysicalOperations")
                        .HasColumnType("bit");

                    b.HasKey("CardId");

                    b.HasAlternateKey("AccountNumber");

                    b.HasIndex("AccountNumber");

                    b.HasIndex("BankAccountNationalId");

                    b.HasIndex("CardId");

                    b.HasIndex("CardNumber");

                    b.ToTable("Cards");
                });

            modelBuilder.Entity("Core.Entities.LoginDetails", b =>
                {
                    b.Property<int>("NationalId")
                        .HasMaxLength(10)
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("varchar(70)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.HasKey("NationalId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("NationalId");

                    b.HasIndex("PhoneNumber")
                        .IsUnique();

                    b.ToTable("LoginDetails");
                });

            modelBuilder.Entity("Core.Entities.Operation", b =>
                {
                    b.Property<int>("OperationId")
                        .HasColumnType("int");

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<int?>("BankAccountNationalId")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OperationType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("OperationId");

                    b.HasAlternateKey("AccountId");

                    b.HasAlternateKey("AccountNumber");

                    b.HasIndex("AccountId");

                    b.HasIndex("AccountNumber");

                    b.HasIndex("BankAccountNationalId");

                    b.HasIndex("OperationId")
                        .IsUnique();

                    b.ToTable("Operations");
                });

            modelBuilder.Entity("Core.Entities.Payment", b =>
                {
                    b.Property<int>("PaymentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PaymentId"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("CardId")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime");

                    b.HasKey("PaymentId");

                    b.HasIndex("CardId");

                    b.HasIndex("PaymentId");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("Core.Entities.ReceiverClient", b =>
                {
                    b.Property<int>("OperationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("OperationId"));

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("OperationId");

                    b.HasIndex("AccountNumber");

                    b.HasIndex("OperationId");

                    b.ToTable("ReceiverClients");
                });

            modelBuilder.Entity("Core.Entities.User", b =>
                {
                    b.Property<int>("NationalId")
                        .HasMaxLength(10)
                        .HasColumnType("int");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("date");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("varchar(100)");

                    b.HasKey("NationalId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Core.Entities.UserContactInfo", b =>
                {
                    b.Property<int>("NationalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("NationalId"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.HasKey("NationalId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("PhoneNumber")
                        .IsUnique();

                    b.ToTable("ContactInfos");
                });

            modelBuilder.Entity("Core.Entities.BankCard", b =>
                {
                    b.HasOne("Core.Entities.BankAccount", "BankAccount")
                        .WithMany("BankCards")
                        .HasForeignKey("BankAccountNationalId");

                    b.Navigation("BankAccount");
                });

            modelBuilder.Entity("Core.Entities.LoginDetails", b =>
                {
                    b.HasOne("Core.Entities.BankAccount", "BankAccount")
                        .WithOne("LoginDetails")
                        .HasForeignKey("Core.Entities.LoginDetails", "NationalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BankAccount");
                });

            modelBuilder.Entity("Core.Entities.Operation", b =>
                {
                    b.HasOne("Core.Entities.BankAccount", null)
                        .WithMany("Operations")
                        .HasForeignKey("BankAccountNationalId");

                    b.HasOne("Core.Entities.ReceiverClient", "Receiver")
                        .WithOne("Operation")
                        .HasForeignKey("Core.Entities.Operation", "OperationId");

                    b.Navigation("Receiver");
                });

            modelBuilder.Entity("Core.Entities.Payment", b =>
                {
                    b.HasOne("Core.Entities.BankCard", "Card")
                        .WithMany("Payments")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Card");
                });

            modelBuilder.Entity("Core.Entities.User", b =>
                {
                    b.HasOne("Core.Entities.BankAccount", "Account")
                        .WithOne("User")
                        .HasForeignKey("Core.Entities.User", "NationalId");

                    b.HasOne("Core.Entities.UserContactInfo", "UserContactInfo")
                        .WithOne("User")
                        .HasForeignKey("Core.Entities.User", "NationalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("UserContactInfo");
                });

            modelBuilder.Entity("Core.Entities.BankAccount", b =>
                {
                    b.Navigation("BankCards");

                    b.Navigation("LoginDetails")
                        .IsRequired();

                    b.Navigation("Operations");

                    b.Navigation("User")
                        .IsRequired();
                });

            modelBuilder.Entity("Core.Entities.BankCard", b =>
                {
                    b.Navigation("Payments");
                });

            modelBuilder.Entity("Core.Entities.ReceiverClient", b =>
                {
                    b.Navigation("Operation")
                        .IsRequired();
                });

            modelBuilder.Entity("Core.Entities.UserContactInfo", b =>
                {
                    b.Navigation("User")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
