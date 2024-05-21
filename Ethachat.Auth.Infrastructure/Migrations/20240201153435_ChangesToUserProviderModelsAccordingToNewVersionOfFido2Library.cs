using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangesToUserProviderModelsAccordingToNewVersionOfFido2Library : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredCredentials_PublicKeyCredentialDescriptor_DescriptorId",
                table: "StoredCredentials");

            migrationBuilder.DropTable(
                name: "PublicKeyCredentialDescriptor");

            migrationBuilder.DropIndex(
                name: "IX_StoredCredentials_DescriptorId",
                table: "StoredCredentials");

            migrationBuilder.AlterColumn<byte[]>(
                name: "DescriptorId",
                table: "StoredCredentials",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);

            migrationBuilder.AddColumn<int[]>(
                name: "DescriptorTransports",
                table: "StoredCredentials",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AddColumn<int>(
                name: "DescriptorType",
                table: "StoredCredentials",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptorTransports",
                table: "StoredCredentials");

            migrationBuilder.DropColumn(
                name: "DescriptorType",
                table: "StoredCredentials");

            migrationBuilder.AlterColumn<byte[]>(
                name: "DescriptorId",
                table: "StoredCredentials",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea");

            migrationBuilder.CreateTable(
                name: "PublicKeyCredentialDescriptor",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "bytea", nullable: false),
                    Transports = table.Column<int[]>(type: "integer[]", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicKeyCredentialDescriptor", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoredCredentials_DescriptorId",
                table: "StoredCredentials",
                column: "DescriptorId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredCredentials_PublicKeyCredentialDescriptor_DescriptorId",
                table: "StoredCredentials",
                column: "DescriptorId",
                principalTable: "PublicKeyCredentialDescriptor",
                principalColumn: "Id");
        }
    }
}
