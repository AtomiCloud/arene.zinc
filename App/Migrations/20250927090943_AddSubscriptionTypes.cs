using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
  /// <inheritdoc />
  public partial class AddSubscriptionTypes : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "SubscriptionTypes",
          columns: table => new
          {
            ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
            Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
            Desc = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_SubscriptionTypes", x => new { x.ProjectId, x.Id });
          });

      migrationBuilder.CreateIndex(
          name: "IX_SubscriptionTypes_ProjectId_Id",
          table: "SubscriptionTypes",
          columns: ["ProjectId", "Id"],
          unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "SubscriptionTypes");
    }
  }
}
