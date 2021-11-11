using Microsoft.EntityFrameworkCore.Migrations;

namespace SuperCRM.Migrations
{
    public partial class InteractionPermissions : Migration
    {
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			var permissions = new[]
			{
				"('IndexInteraction', 'Interaction', 'List Interactions', 0)", // general permission
				"('AddInteraction', 'Interaction', 'Add new Interaction', 0)", // general permission
				"('EditInteraction', 'Interaction', 'Modify Interaction', 1)", // instance permission
				"('DeleteInteraction', 'Interaction', 'Delete Interaction', 1)", // instance permission
			};

			var impliedPermissions = new[]
			{
				"('AddInteraction', 'IndexInteraction')",
				"('EditInteraction', 'AddInteraction')",
				"('DeleteInteraction', 'EditInteraction')",
				"('IndexContact', 'IndexInteraction')",
				"('EditContact', 'EditInteraction')",
				"('DeleteContact', 'DeleteInteraction')"
			};

			var permissionsSql = new[]
			{
				@"insert into [dbo].[Permission](PermissionCode, EntityTypeCode, Description, Kind)values" + string.Join(",\r\n", permissions),
				@"insert into [dbo].[ImpliedPermission]
				(PermissionCode, ImpliedPermissionCode)values" + string.Join(",\r\n", impliedPermissions)
			};


			foreach (var script in permissionsSql)
			{
				migrationBuilder.Sql(script);
			}
		}

		protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
