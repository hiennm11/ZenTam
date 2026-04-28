using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZenTam.Api.Infrastructure.Migrations;

/// <summary>
/// Migration: AddGenderScopeAndTierToActionRuleMapping
/// Drops GenderConstraint column and adds GenderScope, Tier, Priority columns.
/// </summary>
public partial class AddGenderScopeAndTierToActionRuleMapping : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "GenderConstraint",
            table: "ActionRuleMappings");

        migrationBuilder.AddColumn<int>(
            name: "GenderScope",
            table: "ActionRuleMappings",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "Tier",
            table: "ActionRuleMappings",
            nullable: false,
            defaultValue: 3);

        migrationBuilder.AddColumn<int>(
            name: "Priority",
            table: "ActionRuleMappings",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "GenderScope",
            table: "ActionRuleMappings");

        migrationBuilder.DropColumn(
            name: "Tier",
            table: "ActionRuleMappings");

        migrationBuilder.DropColumn(
            name: "Priority",
            table: "ActionRuleMappings");

        migrationBuilder.AddColumn<int>(
            name: "GenderConstraint",
            table: "ActionRuleMappings",
            nullable: true);
    }
}
