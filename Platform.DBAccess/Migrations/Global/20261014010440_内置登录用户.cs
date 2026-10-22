using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.DBAccess.Migrations.Global
{
    /// <inheritdoc />
    public partial class 内置登录用户 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SysUsers",
                columns: new[] { "UserId", "Account", "Department", "Gender", "Password", "Phone", "RealName", "Role" },
                values: new object[] { 1, "你好唐姆", "初始账号", "男", "e10adc3949ba59abbe56e057f20f883e", "", "唐姆", 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SysUsers",
                keyColumn: "UserId",
                keyValue: 1);
        }
    }
}
