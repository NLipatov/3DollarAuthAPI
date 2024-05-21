using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenamedAccessRefreshTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokenEvents");

            migrationBuilder.CreateTable(
                name: "UserAccessRefreshEventLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: false),
                    UserAgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IssueReason = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccessRefreshEventLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAccessRefreshEventLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccessRefreshEventLogs_UserId",
                table: "UserAccessRefreshEventLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAccessRefreshEventLogs");

            migrationBuilder.CreateTable(
                name: "RefreshTokenEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IssueReason = table.Column<int>(type: "integer", nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    UserAgentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokenEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokenEvents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokenEvents_UserId",
                table: "RefreshTokenEvents",
                column: "UserId");
        }
    }
}
