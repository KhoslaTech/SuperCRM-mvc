using Microsoft.EntityFrameworkCore.Migrations;

namespace SuperCRM.Migrations
{
    public partial class ContactPermissions : Migration
    {
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			var permissions = new[]
			{
				"('IndexContact', 'Contact', 'List Contacts', 0)", // general permission
				"('AddContact', 'Contact', 'Add new Contact', 0)", // general permission
				"('EditContact', 'Contact', 'Modify Contact', 1)", // instance permission
				"('DeleteContact', 'Contact', 'Delete Contact', 1)", // instance permission
				"('Customer', 'User', 'Manager Customer permissions', 0)" // general permission
			};

			var impliedPermissions = new[]
			{
				"('AddContact', 'IndexContact')",
				"('EditContact', 'AddContact')",
				"('DeleteContact', 'EditContact')",
				"('Customer', 'DeleteContact')"
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
