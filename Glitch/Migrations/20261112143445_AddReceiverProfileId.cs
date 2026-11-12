using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glitch.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiverProfileId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceiverProfileID",
                table: "Messages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverProfileID",
                table: "Messages",
                column: "ReceiverProfileID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Profiles_ReceiverProfileID",
                table: "Messages",
                column: "ReceiverProfileID",
                principalTable: "Profiles",
                principalColumn: "ProfileID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Profiles_ReceiverProfileID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ReceiverProfileID",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ReceiverProfileID",
                table: "Messages");
        }
    }
}
