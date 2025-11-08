using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAProject.Migrations
{
    /// <inheritdoc />
    public partial class AddFailedLoginCountToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LoginCount",
                table: "AspNetUsers",
                newName: "FailedLoginCount");

            migrationBuilder.RenameColumn(
                name: "LastLoginDate",
                table: "AspNetUsers",
                newName: "LastFailedLoginDate");

            migrationBuilder.RenameColumn(
                name: "ForceMfaAfterLoginCount",
                table: "AspNetUsers",
                newName: "ForceMfaAfterFailedAttempts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastFailedLoginDate",
                table: "AspNetUsers",
                newName: "LastLoginDate");

            migrationBuilder.RenameColumn(
                name: "ForceMfaAfterFailedAttempts",
                table: "AspNetUsers",
                newName: "ForceMfaAfterLoginCount");

            migrationBuilder.RenameColumn(
                name: "FailedLoginCount",
                table: "AspNetUsers",
                newName: "LoginCount");
        }
    }
}
