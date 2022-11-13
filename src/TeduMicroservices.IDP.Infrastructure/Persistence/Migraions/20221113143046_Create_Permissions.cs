using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeduMicroservices.IDP.Infrastructure.Persistence.Migraions
{
    public partial class Create_Permissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: "0e059c4d-9057-4827-96a6-22a017a6cd9d");

            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: "a1878e54-dc14-44f9-8cc1-8c18f2ac0fb6");

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Function = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    RoleId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Command = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "Identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "4dbd077c-3f51-47fc-907b-f746e20696b0", "71b80026-c24e-42c5-aad6-d545c0f21cbf", "Administrator", "ADMINISTRATOR" });

            migrationBuilder.InsertData(
                schema: "Identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "868a6a46-45c1-45a4-955c-8d1571f4de66", "2632ffe9-681d-40cd-aaf0-9dfce690573b", "Customer", "CUSTOMER" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_RoleId_Function_Command",
                schema: "Identity",
                table: "Permissions",
                columns: new[] { "RoleId", "Function", "Command" },
                unique: true,
                filter: "[Function] IS NOT NULL AND [Command] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "Identity");

            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: "4dbd077c-3f51-47fc-907b-f746e20696b0");

            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: "868a6a46-45c1-45a4-955c-8d1571f4de66");

            migrationBuilder.InsertData(
                schema: "Identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "0e059c4d-9057-4827-96a6-22a017a6cd9d", "2491cdaa-4d4d-4ddf-84a8-1284df4a547d", "Customer", "CUSTOMER" });

            migrationBuilder.InsertData(
                schema: "Identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "a1878e54-dc14-44f9-8cc1-8c18f2ac0fb6", "c5ba8917-e219-4a07-879a-79e533627516", "Administrator", "ADMINISTRATOR" });
        }
    }
}
