using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLoginDetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_LoginDetails_NationalId",
                table: "Accounts");

            migrationBuilder.DropTable(
                name: "LoginDetails");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_NationalId",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_NationalId",
                table: "Accounts",
                column: "NationalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_NationalId",
                table: "Accounts");

            migrationBuilder.CreateTable(
                name: "LoginDetails",
                columns: table => new
                {
                    NationalId = table.Column<int>(type: "int", maxLength: 10, nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "varchar(70)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginDetails", x => x.NationalId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_NationalId",
                table: "Accounts",
                column: "NationalId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_LoginDetails_NationalId",
                table: "Accounts",
                column: "NationalId",
                principalTable: "LoginDetails",
                principalColumn: "NationalId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
