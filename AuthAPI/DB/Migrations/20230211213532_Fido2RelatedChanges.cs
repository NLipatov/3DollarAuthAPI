using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class Fido2RelatedChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserClaim_Users_UserId",
                table: "UserClaim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserClaim",
                table: "UserClaim");

            migrationBuilder.RenameTable(
                name: "UserClaim",
                newName: "UsersClaim");

            migrationBuilder.RenameIndex(
                name: "IX_UserClaim_UserId",
                table: "UsersClaim",
                newName: "IX_UsersClaim_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersClaim",
                table: "UsersClaim",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "FidoUsers",
                columns: table => new
                {
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<byte[]>(type: "bytea", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FidoUsers", x => x.RecordId);
                });

            migrationBuilder.CreateTable(
                name: "PublicKeyCredentialDescriptor",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "bytea", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: true),
                    Transports = table.Column<int[]>(type: "integer[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicKeyCredentialDescriptor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoredCredentials",
                columns: table => new
                {
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<byte[]>(type: "bytea", nullable: false),
                    DescriptorId = table.Column<byte[]>(type: "bytea", nullable: true),
                    PublicKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    UserHandle = table.Column<byte[]>(type: "bytea", nullable: false),
                    SignatureCounter = table.Column<long>(type: "bigint", nullable: false),
                    CredType = table.Column<string>(type: "text", nullable: false),
                    RegDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AaGuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredCredentials", x => x.RecordId);
                    table.ForeignKey(
                        name: "FK_StoredCredentials_PublicKeyCredentialDescriptor_DescriptorId",
                        column: x => x.DescriptorId,
                        principalTable: "PublicKeyCredentialDescriptor",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoredCredentials_DescriptorId",
                table: "StoredCredentials",
                column: "DescriptorId");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersClaim_Users_UserId",
                table: "UsersClaim",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersClaim_Users_UserId",
                table: "UsersClaim");

            migrationBuilder.DropTable(
                name: "FidoUsers");

            migrationBuilder.DropTable(
                name: "StoredCredentials");

            migrationBuilder.DropTable(
                name: "PublicKeyCredentialDescriptor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersClaim",
                table: "UsersClaim");

            migrationBuilder.RenameTable(
                name: "UsersClaim",
                newName: "UserClaim");

            migrationBuilder.RenameIndex(
                name: "IX_UsersClaim_UserId",
                table: "UserClaim",
                newName: "IX_UserClaim_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserClaim",
                table: "UserClaim",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserClaim_Users_UserId",
                table: "UserClaim",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
