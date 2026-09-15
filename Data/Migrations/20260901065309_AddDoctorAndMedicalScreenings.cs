using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoktoLink.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorAndMedicalScreenings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DoctorProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    BMDCRegistrationNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MedicalCollege = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Designation = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsBMDCVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalScreenings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestId = table.Column<int>(type: "INTEGER", nullable: false),
                    DonorId = table.Column<int>(type: "INTEGER", nullable: false),
                    DoctorId = table.Column<int>(type: "INTEGER", nullable: true),
                    WeightKg = table.Column<double>(type: "REAL", nullable: false),
                    HemoglobinLevel = table.Column<double>(type: "REAL", nullable: false),
                    SystolicBP = table.Column<int>(type: "INTEGER", nullable: false),
                    DiastolicBP = table.Column<int>(type: "INTEGER", nullable: false),
                    HasNoRecentInfection = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasNoRecentTattooOrSurgery = table.Column<bool>(type: "INTEGER", nullable: false),
                    CooldownConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DoctorRemarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScreenedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalScreenings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalScreenings_BloodRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "BloodRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalScreenings_DoctorProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MedicalScreenings_DonorProfiles_DonorId",
                        column: x => x.DonorId,
                        principalTable: "DonorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProfiles_UserId",
                table: "DoctorProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalScreenings_DoctorId",
                table: "MedicalScreenings",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalScreenings_DonorId",
                table: "MedicalScreenings",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalScreenings_RequestId",
                table: "MedicalScreenings",
                column: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalScreenings");

            migrationBuilder.DropTable(
                name: "DoctorProfiles");
        }
    }
}
