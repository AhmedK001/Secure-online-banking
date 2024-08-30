using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialDbStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    NationalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.NationalId);
                });

            migrationBuilder.CreateTable(
                name: "ContactInfos",
                columns: table => new
                {
                    NationalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactInfos", x => x.NationalId);
                });

            migrationBuilder.CreateTable(
                name: "ReceiverClients",
                columns: table => new
                {
                    OperationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiverClients", x => x.OperationId);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    CardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    CardNumber = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    CVV = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "date", nullable: false),
                    CardType = table.Column<string>(type: "varchar(10)", nullable: false),
                    OpenedForOnlinePurchase = table.Column<bool>(type: "bit", nullable: false),
                    OpenedForPhysicalOperations = table.Column<bool>(type: "bit", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BankAccountNationalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.CardId);
                    table.UniqueConstraint("AK_Cards_AccountNumber", x => x.AccountNumber);
                    table.ForeignKey(
                        name: "FK_Cards_Accounts_BankAccountNationalId",
                        column: x => x.BankAccountNationalId,
                        principalTable: "Accounts",
                        principalColumn: "NationalId");
                });

            migrationBuilder.CreateTable(
                name: "LoginDetails",
                columns: table => new
                {
                    NationalId = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "varchar(70)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginDetails", x => x.NationalId);
                    table.ForeignKey(
                        name: "FK_LoginDetails_Accounts_NationalId",
                        column: x => x.NationalId,
                        principalTable: "Accounts",
                        principalColumn: "NationalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    NationalId = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    FirstName = table.Column<string>(type: "varchar(100)", maxLength: 40, nullable: false),
                    LastName = table.Column<string>(type: "varchar(100)", maxLength: 40, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.NationalId);
                    table.ForeignKey(
                        name: "FK_Users_Accounts_NationalId",
                        column: x => x.NationalId,
                        principalTable: "Accounts",
                        principalColumn: "NationalId");
                    table.ForeignKey(
                        name: "FK_Users_ContactInfos_NationalId",
                        column: x => x.NationalId,
                        principalTable: "ContactInfos",
                        principalColumn: "NationalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    OperationId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    AccountNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    OperationType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankAccountNationalId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.OperationId);
                    table.UniqueConstraint("AK_Operations_AccountId", x => x.AccountId);
                    table.UniqueConstraint("AK_Operations_AccountNumber", x => x.AccountNumber);
                    table.ForeignKey(
                        name: "FK_Operations_Accounts_BankAccountNationalId",
                        column: x => x.BankAccountNationalId,
                        principalTable: "Accounts",
                        principalColumn: "NationalId");
                    table.ForeignKey(
                        name: "FK_Operations_ReceiverClients_OperationId",
                        column: x => x.OperationId,
                        principalTable: "ReceiverClients",
                        principalColumn: "OperationId");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountNumber",
                table: "Accounts",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_NationalId",
                table: "Accounts",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_AccountNumber",
                table: "Cards",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_BankAccountNationalId",
                table: "Cards",
                column: "BankAccountNationalId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CardId",
                table: "Cards",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CardNumber",
                table: "Cards",
                column: "CardNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfos_Email",
                table: "ContactInfos",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfos_PhoneNumber",
                table: "ContactInfos",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoginDetails_Email",
                table: "LoginDetails",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoginDetails_NationalId",
                table: "LoginDetails",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginDetails_PhoneNumber",
                table: "LoginDetails",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Operations_AccountId",
                table: "Operations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_AccountNumber",
                table: "Operations",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_BankAccountNationalId",
                table: "Operations",
                column: "BankAccountNationalId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_OperationId",
                table: "Operations",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CardId",
                table: "Payments",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentId",
                table: "Payments",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiverClients_AccountNumber",
                table: "ReceiverClients",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiverClients_OperationId",
                table: "ReceiverClients",
                column: "OperationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginDetails");

            migrationBuilder.DropTable(
                name: "Operations");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ReceiverClients");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "ContactInfos");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
