using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeneratorsManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddOperatingModeAndIoTSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentFuelLevelCM",
                table: "Generators",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentFuelLiters",
                table: "Generators",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentFuelPricePerLiter",
                table: "Generators",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelConsumptionRatePerHour",
                table: "Generators",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRunning",
                table: "Generators",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStartTime",
                table: "Generators",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStopTime",
                table: "Generators",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LitersPerCM",
                table: "Generators",
                type: "decimal(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OperatingMode",
                table: "Generators",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TankHeightCM",
                table: "Generators",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualQuantity",
                table: "FuelAllocations",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AllocationBookNumber",
                table: "FuelAllocations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EnteredQuantity",
                table: "FuelAllocations",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GeneratedExpenseId",
                table: "FuelAllocations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GeneratorId",
                table: "FuelAllocations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCommercial",
                table: "FuelAllocations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyRemaining",
                table: "FuelAllocations",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficialAuthority",
                table: "FuelAllocations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "FuelAllocations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FuelRefills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefillNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GeneratorId = table.Column<int>(type: "int", nullable: false),
                    FuelAllocationId = table.Column<int>(type: "int", nullable: true),
                    RefillDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LevelBefore_CM = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    LevelBefore_Liters = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    LevelAfter_CM = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    LevelAfter_Liters = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    RefilledLiters = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PricePerLiter = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    RefilledBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelRefills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuelRefills_FuelAllocations_FuelAllocationId",
                        column: x => x.FuelAllocationId,
                        principalTable: "FuelAllocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FuelRefills_Generators_GeneratorId",
                        column: x => x.GeneratorId,
                        principalTable: "Generators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IoTDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GeneratorId = table.Column<int>(type: "int", nullable: false),
                    DeviceType = table.Column<int>(type: "int", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ApiSecret = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    MacAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FirmwareVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastReadingAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadingsCount = table.Column<long>(type: "bigint", nullable: false),
                    ReportingIntervalSeconds = table.Column<int>(type: "int", nullable: false),
                    AttachedSensors = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IoTDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IoTDevices_Generators_GeneratorId",
                        column: x => x.GeneratorId,
                        principalTable: "Generators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OperatingSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneratorId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationHours = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FuelLevelBefore_CM = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FuelLevelBefore_Liters = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FuelLevelAfter_CM = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FuelLevelAfter_Liters = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FuelConsumed_Liters = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ConsumptionRate = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PricePerLiter = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    HourlyCost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DataSource = table.Column<int>(type: "int", nullable: false),
                    FuelSource = table.Column<int>(type: "int", nullable: false),
                    StartedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StoppedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperatingSessions_Generators_GeneratorId",
                        column: x => x.GeneratorId,
                        principalTable: "Generators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SensorReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IoTDeviceId = table.Column<int>(type: "int", nullable: false),
                    GeneratorId = table.Column<int>(type: "int", nullable: false),
                    ReadingType = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    CalculatedLiters = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    SensorStatus = table.Column<int>(type: "int", nullable: false),
                    StatusMessage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReadingTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SenderIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RawData = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorReadings_Generators_GeneratorId",
                        column: x => x.GeneratorId,
                        principalTable: "Generators",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SensorReadings_IoTDevices_IoTDeviceId",
                        column: x => x.IoTDeviceId,
                        principalTable: "IoTDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FuelAllocations_GeneratorId",
                table: "FuelAllocations",
                column: "GeneratorId");

            migrationBuilder.CreateIndex(
                name: "IX_FuelRefills_FuelAllocationId",
                table: "FuelRefills",
                column: "FuelAllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_FuelRefills_GeneratorId",
                table: "FuelRefills",
                column: "GeneratorId");

            migrationBuilder.CreateIndex(
                name: "IX_FuelRefills_RefillDate",
                table: "FuelRefills",
                column: "RefillDate");

            migrationBuilder.CreateIndex(
                name: "IX_FuelRefills_RefillNumber",
                table: "FuelRefills",
                column: "RefillNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IoTDevices_ApiKey",
                table: "IoTDevices",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IoTDevices_DeviceName",
                table: "IoTDevices",
                column: "DeviceName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IoTDevices_GeneratorId",
                table: "IoTDevices",
                column: "GeneratorId");

            migrationBuilder.CreateIndex(
                name: "IX_IoTDevices_Status",
                table: "IoTDevices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OperatingSessions_GeneratorId",
                table: "OperatingSessions",
                column: "GeneratorId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatingSessions_StartTime",
                table: "OperatingSessions",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_GeneratorId_ReadingType",
                table: "SensorReadings",
                columns: new[] { "GeneratorId", "ReadingType" });

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_IoTDeviceId",
                table: "SensorReadings",
                column: "IoTDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_ReadingTime",
                table: "SensorReadings",
                column: "ReadingTime");

            migrationBuilder.AddForeignKey(
                name: "FK_FuelAllocations_Generators_GeneratorId",
                table: "FuelAllocations",
                column: "GeneratorId",
                principalTable: "Generators",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FuelAllocations_Generators_GeneratorId",
                table: "FuelAllocations");

            migrationBuilder.DropTable(
                name: "FuelRefills");

            migrationBuilder.DropTable(
                name: "OperatingSessions");

            migrationBuilder.DropTable(
                name: "SensorReadings");

            migrationBuilder.DropTable(
                name: "IoTDevices");

            migrationBuilder.DropIndex(
                name: "IX_FuelAllocations_GeneratorId",
                table: "FuelAllocations");

            migrationBuilder.DropColumn(
                name: "CurrentFuelLevelCM",
                table: "Generators");

            migrationBuilder.DropColumn(
                name: "CurrentFuelLiters",
                table: "Generators");

            migrationBuilder.DropColumn(
                name: "CurrentFuelPricePerLiter",
                table: "Generators");

            migrationBuilder.DropColumn(
                name: "FuelConsumptionRatePerHour",
                table: "Generators");

            migrationBuilder.DropColumn(
                name: "IsRunning",
                table: "Generators");

            migrationBuilder.DropColumn(
                name: "LastStartTime",
                table: "Generators");

            migrationBuilder.DropColumn(
                name: "LastStopTime",
                table: "Generators");

            migrationBuilder.DropColumn(
                name: "LitersPerCM",
                table: "Generators");

            migrationBuilder.DropColumn(
                name: "OperatingMode",
                table: "Generators");

            migrationBuilder.DropColumn(
                name: "TankHeightCM",
                table: "Generators");

            migrationBuilder.DropColumn(
                name: "ActualQuantity",
                table: "FuelAllocations");

            migrationBuilder.DropColumn(
                name: "AllocationBookNumber",
                table: "FuelAllocations");

            migrationBuilder.DropColumn(
                name: "EnteredQuantity",
                table: "FuelAllocations");

            migrationBuilder.DropColumn(
                name: "GeneratedExpenseId",
                table: "FuelAllocations");

            migrationBuilder.DropColumn(
                name: "GeneratorId",
                table: "FuelAllocations");

            migrationBuilder.DropColumn(
                name: "IsCommercial",
                table: "FuelAllocations");

            migrationBuilder.DropColumn(
                name: "MonthlyRemaining",
                table: "FuelAllocations");

            migrationBuilder.DropColumn(
                name: "OfficialAuthority",
                table: "FuelAllocations");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "FuelAllocations");
        }
    }
}
