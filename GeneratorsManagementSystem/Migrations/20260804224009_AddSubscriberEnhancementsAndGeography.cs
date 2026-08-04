using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeneratorsManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriberEnhancementsAndGeography : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdminCommissionAmount",
                table: "Subscriptions",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdminCommissionPercentage",
                table: "Subscriptions",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DeviceCount",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeviceTypeId",
                table: "Subscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Subscriptions",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DiscountNotes",
                table: "Subscriptions",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountReasonId",
                table: "Subscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExemptBy",
                table: "Subscriptions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExemptDate",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExemptReason",
                table: "Subscriptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFullExempt",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "AddressNotes",
                table: "Subscribers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AlleyId",
                table: "Subscribers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppVersion",
                table: "Subscribers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "Subscribers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "Subscribers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GovernorateId",
                table: "Subscribers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMobileLinked",
                table: "Subscribers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMobileContact",
                table: "Subscribers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MobileActivatedAt",
                table: "Subscribers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileOS",
                table: "Subscribers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileOtp",
                table: "Subscribers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NearestLandmark",
                table: "Subscribers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NeighborhoodId",
                table: "Subscribers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotificationsEnabled",
                table: "Subscribers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DeviceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DefaultPrice = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    DefaultAmpere = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscountReasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DefaultPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Governorates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Governorates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GovernorateId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_Governorates_GovernorateId",
                        column: x => x.GovernorateId,
                        principalTable: "Governorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Neighborhoods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Neighborhoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Neighborhoods_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alleys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NeighborhoodId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alleys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alleys_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_DeviceTypeId",
                table: "Subscriptions",
                column: "DeviceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_DiscountReasonId",
                table: "Subscriptions",
                column: "DiscountReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_AlleyId",
                table: "Subscribers",
                column: "AlleyId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_DistrictId",
                table: "Subscribers",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_GovernorateId",
                table: "Subscribers",
                column: "GovernorateId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_NeighborhoodId",
                table: "Subscribers",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Alleys_Name",
                table: "Alleys",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Alleys_NeighborhoodId",
                table: "Alleys",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTypes_Name",
                table: "DeviceTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscountReasons_Name",
                table: "DiscountReasons",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Districts_GovernorateId",
                table: "Districts",
                column: "GovernorateId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_Name",
                table: "Districts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Governorates_Name",
                table: "Governorates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Neighborhoods_DistrictId",
                table: "Neighborhoods",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Neighborhoods_Name",
                table: "Neighborhoods",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscribers_Alleys_AlleyId",
                table: "Subscribers",
                column: "AlleyId",
                principalTable: "Alleys",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscribers_Districts_DistrictId",
                table: "Subscribers",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscribers_Governorates_GovernorateId",
                table: "Subscribers",
                column: "GovernorateId",
                principalTable: "Governorates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscribers_Neighborhoods_NeighborhoodId",
                table: "Subscribers",
                column: "NeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_DeviceTypes_DeviceTypeId",
                table: "Subscriptions",
                column: "DeviceTypeId",
                principalTable: "DeviceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_DiscountReasons_DiscountReasonId",
                table: "Subscriptions",
                column: "DiscountReasonId",
                principalTable: "DiscountReasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscribers_Alleys_AlleyId",
                table: "Subscribers");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscribers_Districts_DistrictId",
                table: "Subscribers");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscribers_Governorates_GovernorateId",
                table: "Subscribers");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscribers_Neighborhoods_NeighborhoodId",
                table: "Subscribers");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_DeviceTypes_DeviceTypeId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_DiscountReasons_DiscountReasonId",
                table: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Alleys");

            migrationBuilder.DropTable(
                name: "DeviceTypes");

            migrationBuilder.DropTable(
                name: "DiscountReasons");

            migrationBuilder.DropTable(
                name: "Neighborhoods");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "Governorates");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_DeviceTypeId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_DiscountReasonId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscribers_AlleyId",
                table: "Subscribers");

            migrationBuilder.DropIndex(
                name: "IX_Subscribers_DistrictId",
                table: "Subscribers");

            migrationBuilder.DropIndex(
                name: "IX_Subscribers_GovernorateId",
                table: "Subscribers");

            migrationBuilder.DropIndex(
                name: "IX_Subscribers_NeighborhoodId",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "AdminCommissionAmount",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "AdminCommissionPercentage",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeviceCount",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DeviceTypeId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DiscountNotes",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DiscountReasonId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ExemptBy",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ExemptDate",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ExemptReason",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "IsFullExempt",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "AlleyId",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "AppVersion",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "GovernorateId",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "IsMobileLinked",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "LastMobileContact",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "MobileActivatedAt",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "MobileOS",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "MobileOtp",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "NearestLandmark",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "NeighborhoodId",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "NotificationsEnabled",
                table: "Subscribers");

            migrationBuilder.AlterColumn<string>(
                name: "AddressNotes",
                table: "Subscribers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
