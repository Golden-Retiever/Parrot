using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.DBAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddEndianProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 属性字典新增:字节序 / 字序(PropType=1 下拉)
            migrationBuilder.InsertData(
                table: "Properties",
                columns: new[] { "PropId", "PropTitle", "PropName", "PropType" },
                values: new object[,]
                {
                    { 15, "字节序", "DeviceEndian", 1 },
                    { 16, "字序", "WordOrder", 1 },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Properties",
                keyColumn: "PropId",
                keyValues: new object[] { 15, 16 });
        }
    }
}
