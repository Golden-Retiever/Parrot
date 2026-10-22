using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.DBAccess.Migrations.Scenario
{
    /// <inheritdoc />
    public partial class InitScenario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AirRankings",
                columns: table => new
                {
                    RankingId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkshopName = table.Column<string>(type: "TEXT", nullable: false),
                    PlanValue = table.Column<double>(type: "REAL", nullable: false),
                    FinishedValue = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirRankings", x => x.RankingId);
                });

            migrationBuilder.CreateTable(
                name: "DeviceAlarms",
                columns: table => new
                {
                    AlarmNum = table.Column<string>(type: "TEXT", nullable: false),
                    ConfNum = table.Column<string>(type: "TEXT", nullable: true),
                    DeviceNum = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceName = table.Column<string>(type: "TEXT", nullable: false),
                    VarNum = table.Column<string>(type: "TEXT", nullable: true),
                    VarName = table.Column<string>(type: "TEXT", nullable: true),
                    AlarmValue = table.Column<string>(type: "TEXT", nullable: true),
                    AlarmContent = table.Column<string>(type: "TEXT", nullable: false),
                    RecordTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Account = table.Column<string>(type: "TEXT", nullable: false),
                    SolveTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    State = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAlarms", x => x.AlarmNum);
                });

            migrationBuilder.CreateTable(
                name: "DeviceProps",
                columns: table => new
                {
                    DevicePropId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceNum = table.Column<string>(type: "TEXT", nullable: false),
                    PropName = table.Column<string>(type: "TEXT", nullable: false),
                    PropValue = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceProps", x => x.DevicePropId);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    DeviceNum = table.Column<string>(type: "TEXT", nullable: false),
                    RefreshRate = table.Column<string>(type: "TEXT", nullable: false),
                    ComponentId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceName = table.Column<string>(type: "TEXT", nullable: false),
                    ComponentType = table.Column<string>(type: "TEXT", nullable: false),
                    X = table.Column<double>(type: "REAL", nullable: false),
                    Y = table.Column<double>(type: "REAL", nullable: false),
                    Z = table.Column<double>(type: "REAL", nullable: false),
                    Width = table.Column<double>(type: "REAL", nullable: false),
                    Height = table.Column<double>(type: "REAL", nullable: false),
                    FlowDirection = table.Column<int>(type: "INTEGER", nullable: false),
                    Rotate = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.DeviceNum);
                });

            migrationBuilder.CreateTable(
                name: "DeviceVars",
                columns: table => new
                {
                    VarNum = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceNum = table.Column<string>(type: "TEXT", nullable: false),
                    VarName = table.Column<string>(type: "TEXT", nullable: false),
                    VarAddress = table.Column<string>(type: "TEXT", nullable: false),
                    IsRawFrame = table.Column<bool>(type: "INTEGER", nullable: false),
                    VarType = table.Column<string>(type: "TEXT", nullable: false),
                    Offset = table.Column<double>(type: "REAL", nullable: false),
                    Modulus = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceVars", x => x.VarNum);
                });

            migrationBuilder.CreateTable(
                name: "DeviceWarnings",
                columns: table => new
                {
                    WarningId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceWarnings", x => x.WarningId);
                });

            migrationBuilder.CreateTable(
                name: "ManualControls",
                columns: table => new
                {
                    ControlId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceNum = table.Column<string>(type: "TEXT", nullable: false),
                    ControlName = table.Column<string>(type: "TEXT", nullable: false),
                    ControlAddress = table.Column<string>(type: "TEXT", nullable: false),
                    IsRawFrame = table.Column<bool>(type: "INTEGER", nullable: false),
                    ControlValue = table.Column<string>(type: "TEXT", nullable: true),
                    ValueType = table.Column<string>(type: "TEXT", nullable: false),
                    IsLoop = table.Column<bool>(type: "INTEGER", nullable: false),
                    ControlDelay = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManualControls", x => x.ControlId);
                });

            migrationBuilder.CreateTable(
                name: "MonitorRecords",
                columns: table => new
                {
                    RecordId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceNum = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceName = table.Column<string>(type: "TEXT", nullable: false),
                    VarNum = table.Column<string>(type: "TEXT", nullable: false),
                    VarName = table.Column<string>(type: "TEXT", nullable: false),
                    RecordValue = table.Column<decimal>(type: "TEXT", nullable: false),
                    AlarmNum = table.Column<string>(type: "TEXT", nullable: true),
                    RecordTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Account = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitorRecords", x => x.RecordId);
                });

            migrationBuilder.CreateTable(
                name: "MonitorSettings",
                columns: table => new
                {
                    SettingNum = table.Column<int>(type: "INTEGER", nullable: false),
                    Header = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DeviceNum = table.Column<string>(type: "TEXT", nullable: false),
                    VarNum = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitorSettings", x => x.SettingNum);
                });

            migrationBuilder.CreateTable(
                name: "SevenAirs",
                columns: table => new
                {
                    AirId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Day = table.Column<string>(type: "TEXT", nullable: false),
                    Air = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SevenAirs", x => x.AirId);
                });

            migrationBuilder.CreateTable(
                name: "SevenLeaks",
                columns: table => new
                {
                    LeakId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Day = table.Column<string>(type: "TEXT", nullable: false),
                    Leak = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SevenLeaks", x => x.LeakId);
                });

            migrationBuilder.CreateTable(
                name: "SevenPowers",
                columns: table => new
                {
                    PowerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Day = table.Column<string>(type: "TEXT", nullable: false),
                    Power = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SevenPowers", x => x.PowerId);
                });

            migrationBuilder.CreateTable(
                name: "TrendAxises",
                columns: table => new
                {
                    AxisNum = table.Column<string>(type: "TEXT", nullable: false),
                    TrendNum = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    IsShowTitle = table.Column<bool>(type: "INTEGER", nullable: false),
                    Minimum = table.Column<double>(type: "REAL", nullable: false),
                    Maximum = table.Column<double>(type: "REAL", nullable: false),
                    IsShowSeperator = table.Column<bool>(type: "INTEGER", nullable: false),
                    LabelFormater = table.Column<string>(type: "TEXT", nullable: false),
                    Position = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrendAxises", x => x.AxisNum);
                });

            migrationBuilder.CreateTable(
                name: "Trends",
                columns: table => new
                {
                    TrendNum = table.Column<string>(type: "TEXT", nullable: false),
                    TrendName = table.Column<string>(type: "TEXT", nullable: false),
                    IsShowLegend = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trends", x => x.TrendNum);
                });

            migrationBuilder.CreateTable(
                name: "TrendSections",
                columns: table => new
                {
                    SectionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AxisNum = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrendSections", x => x.SectionId);
                });

            migrationBuilder.CreateTable(
                name: "TrendSerieses",
                columns: table => new
                {
                    SeriesId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceNum = table.Column<string>(type: "TEXT", nullable: false),
                    VarNum = table.Column<string>(type: "TEXT", nullable: false),
                    TrendNum = table.Column<string>(type: "TEXT", nullable: false),
                    ANum = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrendSerieses", x => x.SeriesId);
                });

            migrationBuilder.CreateTable(
                name: "VarAlarmConfs",
                columns: table => new
                {
                    ConfNum = table.Column<string>(type: "TEXT", nullable: false),
                    VarNum = table.Column<string>(type: "TEXT", nullable: false),
                    Operator = table.Column<string>(type: "TEXT", nullable: false),
                    CompareValue = table.Column<string>(type: "TEXT", nullable: false),
                    AlarmContent = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VarAlarmConfs", x => x.ConfNum);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AirRankings");

            migrationBuilder.DropTable(
                name: "DeviceAlarms");

            migrationBuilder.DropTable(
                name: "DeviceProps");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "DeviceVars");

            migrationBuilder.DropTable(
                name: "DeviceWarnings");

            migrationBuilder.DropTable(
                name: "ManualControls");

            migrationBuilder.DropTable(
                name: "MonitorRecords");

            migrationBuilder.DropTable(
                name: "MonitorSettings");

            migrationBuilder.DropTable(
                name: "SevenAirs");

            migrationBuilder.DropTable(
                name: "SevenLeaks");

            migrationBuilder.DropTable(
                name: "SevenPowers");

            migrationBuilder.DropTable(
                name: "TrendAxises");

            migrationBuilder.DropTable(
                name: "Trends");

            migrationBuilder.DropTable(
                name: "TrendSections");

            migrationBuilder.DropTable(
                name: "TrendSerieses");

            migrationBuilder.DropTable(
                name: "VarAlarmConfs");
        }
    }
}
