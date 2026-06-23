using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VietNhatHospital.Migrations
{
    /// <inheritdoc />
    public partial class SyncLatestSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrugInteractions_Drugs_DrugAID",
                table: "DrugInteractions");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugInteractions_Drugs_DrugBID",
                table: "DrugInteractions");

            migrationBuilder.DropTable(
                name: "Contraindications");

            migrationBuilder.DropTable(
                name: "Diseases");

            migrationBuilder.DropColumn(
                name: "DrugName",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "WarningLevel",
                table: "DrugInteractions");

            migrationBuilder.RenameColumn(
                name: "DrugID",
                table: "Drugs",
                newName: "DrugId");

            migrationBuilder.RenameColumn(
                name: "UsageGuide",
                table: "Drugs",
                newName: "DrugGroup");

            migrationBuilder.RenameColumn(
                name: "DrugBID",
                table: "DrugInteractions",
                newName: "DrugId2");

            migrationBuilder.RenameColumn(
                name: "DrugAID",
                table: "DrugInteractions",
                newName: "DrugId1");

            migrationBuilder.RenameColumn(
                name: "InteractionID",
                table: "DrugInteractions",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_DrugInteractions_DrugBID",
                table: "DrugInteractions",
                newName: "IX_DrugInteractions_DrugId2");

            migrationBuilder.RenameIndex(
                name: "IX_DrugInteractions_DrugAID",
                table: "DrugInteractions",
                newName: "IX_DrugInteractions_DrugId1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Drugs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Drugs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Drugs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Drugs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "DrugInteractions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "DrugInteractions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Recommendation",
                table: "DrugInteractions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Height",
                table: "AspNetUsers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "AspNetUsers",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Conditions",
                columns: table => new
                {
                    ConditionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IcdCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conditions", x => x.ConditionId);
                });

            migrationBuilder.CreateTable(
                name: "DrugContraindications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    ConditionId = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alternative = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugContraindications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugContraindications_Conditions_ConditionId",
                        column: x => x.ConditionId,
                        principalTable: "Conditions",
                        principalColumn: "ConditionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrugContraindications_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "DrugId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientConditions",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConditionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientConditions", x => new { x.UserId, x.ConditionId });
                    table.ForeignKey(
                        name: "FK_PatientConditions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientConditions_Conditions_ConditionId",
                        column: x => x.ConditionId,
                        principalTable: "Conditions",
                        principalColumn: "ConditionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrugContraindications_ConditionId",
                table: "DrugContraindications",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugContraindications_DrugId",
                table: "DrugContraindications",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientConditions_ConditionId",
                table: "PatientConditions",
                column: "ConditionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DrugInteractions_Drugs_DrugId1",
                table: "DrugInteractions",
                column: "DrugId1",
                principalTable: "Drugs",
                principalColumn: "DrugId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugInteractions_Drugs_DrugId2",
                table: "DrugInteractions",
                column: "DrugId2",
                principalTable: "Drugs",
                principalColumn: "DrugId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrugInteractions_Drugs_DrugId1",
                table: "DrugInteractions");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugInteractions_Drugs_DrugId2",
                table: "DrugInteractions");

            migrationBuilder.DropTable(
                name: "DrugContraindications");

            migrationBuilder.DropTable(
                name: "PatientConditions");

            migrationBuilder.DropTable(
                name: "Conditions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "DrugInteractions");

            migrationBuilder.DropColumn(
                name: "Recommendation",
                table: "DrugInteractions");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "DrugId",
                table: "Drugs",
                newName: "DrugID");

            migrationBuilder.RenameColumn(
                name: "DrugGroup",
                table: "Drugs",
                newName: "UsageGuide");

            migrationBuilder.RenameColumn(
                name: "DrugId2",
                table: "DrugInteractions",
                newName: "DrugBID");

            migrationBuilder.RenameColumn(
                name: "DrugId1",
                table: "DrugInteractions",
                newName: "DrugAID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "DrugInteractions",
                newName: "InteractionID");

            migrationBuilder.RenameIndex(
                name: "IX_DrugInteractions_DrugId2",
                table: "DrugInteractions",
                newName: "IX_DrugInteractions_DrugBID");

            migrationBuilder.RenameIndex(
                name: "IX_DrugInteractions_DrugId1",
                table: "DrugInteractions",
                newName: "IX_DrugInteractions_DrugAID");

            migrationBuilder.AddColumn<string>(
                name: "DrugName",
                table: "Drugs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "DrugInteractions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "WarningLevel",
                table: "DrugInteractions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Diseases",
                columns: table => new
                {
                    DiseaseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiseaseName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diseases", x => x.DiseaseID);
                });

            migrationBuilder.CreateTable(
                name: "Contraindications",
                columns: table => new
                {
                    ContraID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiseaseID = table.Column<int>(type: "int", nullable: false),
                    DrugID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarningLevel = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contraindications", x => x.ContraID);
                    table.ForeignKey(
                        name: "FK_Contraindications_Diseases_DiseaseID",
                        column: x => x.DiseaseID,
                        principalTable: "Diseases",
                        principalColumn: "DiseaseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contraindications_Drugs_DrugID",
                        column: x => x.DrugID,
                        principalTable: "Drugs",
                        principalColumn: "DrugID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contraindications_DiseaseID",
                table: "Contraindications",
                column: "DiseaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Contraindications_DrugID",
                table: "Contraindications",
                column: "DrugID");

            migrationBuilder.AddForeignKey(
                name: "FK_DrugInteractions_Drugs_DrugAID",
                table: "DrugInteractions",
                column: "DrugAID",
                principalTable: "Drugs",
                principalColumn: "DrugID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugInteractions_Drugs_DrugBID",
                table: "DrugInteractions",
                column: "DrugBID",
                principalTable: "Drugs",
                principalColumn: "DrugID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
