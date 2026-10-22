using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.DBAccess.Migrations.Global
{
    /// <inheritdoc />
    public partial class 修改口令种子 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AdminCommand",
                keyColumn: "Id",
                keyValue: 1,
                column: "CommandText",
                value: "我不是唐人儿");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AdminCommand",
                keyColumn: "Id",
                keyValue: 1,
                column: "CommandText",
                value: "平台管理员核对");
        }
    }
}
