using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.DBAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIsAdminToRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAdmin",
                table: "SysUsers",
                newName: "Role");

            //老数据订正:旧bool true存的是1,新语义里1=二级;老管理员应是2(一级)
            migrationBuilder.Sql("UPDATE \"SysUsers\" SET \"Role\" = 2 WHERE \"Role\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"SysUsers\" SET \"Role\" = 1 WHERE \"Role\" = 2");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "SysUsers",
                newName: "IsAdmin");
        }
    }
}
