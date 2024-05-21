using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedColumnsForFidoUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FidoUserRecordId",
                table: "WebPushNotificationSubscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebPushNotificationSubscriptions_FidoUserRecordId",
                table: "WebPushNotificationSubscriptions",
                column: "FidoUserRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_WebPushNotificationSubscriptions_FidoUsers_FidoUserRecordId",
                table: "WebPushNotificationSubscriptions",
                column: "FidoUserRecordId",
                principalTable: "FidoUsers",
                principalColumn: "RecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebPushNotificationSubscriptions_FidoUsers_FidoUserRecordId",
                table: "WebPushNotificationSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_WebPushNotificationSubscriptions_FidoUserRecordId",
                table: "WebPushNotificationSubscriptions");

            migrationBuilder.DropColumn(
                name: "FidoUserRecordId",
                table: "WebPushNotificationSubscriptions");
        }
    }
}
