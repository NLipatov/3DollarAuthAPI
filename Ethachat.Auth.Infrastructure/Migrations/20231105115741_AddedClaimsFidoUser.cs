using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedClaimsFidoUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FidoUserRecordId",
                table: "UsersClaim",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsersClaim_FidoUserRecordId",
                table: "UsersClaim",
                column: "FidoUserRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersClaim_FidoUsers_FidoUserRecordId",
                table: "UsersClaim",
                column: "FidoUserRecordId",
                principalTable: "FidoUsers",
                principalColumn: "RecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersClaim_FidoUsers_FidoUserRecordId",
                table: "UsersClaim");

            migrationBuilder.DropIndex(
                name: "IX_UsersClaim_FidoUserRecordId",
                table: "UsersClaim");

            migrationBuilder.DropColumn(
                name: "FidoUserRecordId",
                table: "UsersClaim");
        }
    }
}
