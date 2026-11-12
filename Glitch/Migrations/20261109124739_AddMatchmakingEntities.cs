using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glitch.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchmakingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Swipes_Profiles_FromPrifileID",
                table: "Swipes");

            migrationBuilder.DropIndex(
                name: "IX_Swipes_FromPrifileID",
                table: "Swipes");

            migrationBuilder.RenameColumn(
                name: "FromPrifileID",
                table: "Swipes",
                newName: "FromProfileID");

            migrationBuilder.CreateTable(
                name: "Conditions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsCustom = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conditions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Interests",
                columns: table => new
                {
                    InterestID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interests", x => x.InterestID);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderProfileID = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Messages_Profiles_SenderProfileID",
                        column: x => x.SenderProfileID,
                        principalTable: "Profiles",
                        principalColumn: "ProfileID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileConditions",
                columns: table => new
                {
                    Profilefield = table.Column<int>(type: "INTEGER", nullable: false),
                    ConditionID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileConditions", x => new { x.Profilefield, x.ConditionID });
                    table.ForeignKey(
                        name: "FK_ProfileConditions_Conditions_ConditionID",
                        column: x => x.ConditionID,
                        principalTable: "Conditions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileConditions_Profiles_Profilefield",
                        column: x => x.Profilefield,
                        principalTable: "Profiles",
                        principalColumn: "ProfileID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileInterests",
                columns: table => new
                {
                    ProfileID = table.Column<int>(type: "INTEGER", nullable: false),
                    InterestID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileInterests", x => new { x.ProfileID, x.InterestID });
                    table.ForeignKey(
                        name: "FK_ProfileInterests_Interests_InterestID",
                        column: x => x.InterestID,
                        principalTable: "Interests",
                        principalColumn: "InterestID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileInterests_Profiles_ProfileID",
                        column: x => x.ProfileID,
                        principalTable: "Profiles",
                        principalColumn: "ProfileID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Swipes_FromProfileID_ToProfileID",
                table: "Swipes",
                columns: new[] { "FromProfileID", "ToProfileID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderProfileID",
                table: "Messages",
                column: "SenderProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileConditions_ConditionID",
                table: "ProfileConditions",
                column: "ConditionID");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileInterests_InterestID",
                table: "ProfileInterests",
                column: "InterestID");

            migrationBuilder.AddForeignKey(
                name: "FK_Swipes_Profiles_FromProfileID",
                table: "Swipes",
                column: "FromProfileID",
                principalTable: "Profiles",
                principalColumn: "ProfileID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Swipes_Profiles_FromProfileID",
                table: "Swipes");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "ProfileConditions");

            migrationBuilder.DropTable(
                name: "ProfileInterests");

            migrationBuilder.DropTable(
                name: "Conditions");

            migrationBuilder.DropTable(
                name: "Interests");

            migrationBuilder.DropIndex(
                name: "IX_Swipes_FromProfileID_ToProfileID",
                table: "Swipes");

            migrationBuilder.RenameColumn(
                name: "FromProfileID",
                table: "Swipes",
                newName: "FromPrifileID");

            migrationBuilder.CreateIndex(
                name: "IX_Swipes_FromPrifileID",
                table: "Swipes",
                column: "FromPrifileID");

            migrationBuilder.AddForeignKey(
                name: "FK_Swipes_Profiles_FromPrifileID",
                table: "Swipes",
                column: "FromPrifileID",
                principalTable: "Profiles",
                principalColumn: "ProfileID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
