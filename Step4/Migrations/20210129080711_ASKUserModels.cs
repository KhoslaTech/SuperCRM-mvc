using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SuperCRM.Migrations
{
	public partial class ASKUserModels : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Permission",
				columns: table => new
				{
					PermissionCode = table.Column<string>(maxLength: 40, nullable: false),
					EntityTypeCode = table.Column<string>(maxLength: 30, nullable: false),
					Kind = table.Column<int>(nullable: false),
					Description = table.Column<string>(maxLength: 100, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Permission", x => x.PermissionCode);
				});

			migrationBuilder.CreateTable(
				name: "User",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					Name = table.Column<string>(maxLength: 60, nullable: false),
					HashedPassword = table.Column<string>(maxLength: 128, nullable: false),
					Username = table.Column<string>(maxLength: 100, nullable: false),
					Verified = table.Column<bool>(nullable: false),
					VerificationToken = table.Column<string>(maxLength: 36, nullable: false),
					PasswordResetToken = table.Column<string>(maxLength: 36, nullable: true),
					PasswordResetTokenExpiredAt = table.Column<DateTime>(nullable: true),
					CreatedDate = table.Column<DateTime>(nullable: false),
					ParentId = table.Column<Guid>(nullable: true),
					Suspended = table.Column<bool>(nullable: false),
					SuspensionDate = table.Column<DateTime>(nullable: true),
					SuspensionReason = table.Column<string>(nullable: true),
					PasswordBlocked = table.Column<bool>(nullable: false),
					PasswordBlockedDate = table.Column<DateTime>(nullable: true),
					PasswordBlockedReason = table.Column<string>(nullable: true),
					PasswordExpiredAt = table.Column<DateTime>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_User", x => x.Id);
					table.ForeignKey(
						name: "FK_User_User_ParentId",
						column: x => x.ParentId,
						principalTable: "User",
						principalColumn: "Id");
				});

			migrationBuilder.CreateTable(
				name: "ImpliedPermission",
				columns: table => new
				{
					PermissionCode = table.Column<string>(maxLength: 40, nullable: false),
					ImpliedPermissionCode = table.Column<string>(maxLength: 40, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ImpliedPermission", x => new { x.PermissionCode, x.ImpliedPermissionCode });
					table.ForeignKey(
						name: "FK_ImpliedPermission_Permission_ImpliedPermissionCode",
						column: x => x.ImpliedPermissionCode,
						principalTable: "Permission",
						principalColumn: "PermissionCode");
					table.ForeignKey(
						name: "FK_ImpliedPermission_Permission_PermissionCode",
						column: x => x.PermissionCode,
						principalTable: "Permission",
						principalColumn: "PermissionCode");
				});

			migrationBuilder.CreateTable(
				name: "UserPermitGroup",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					UserId = table.Column<Guid>(nullable: false),
					GroupName = table.Column<string>(maxLength: 100, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserPermitGroup", x => x.Id);
					table.ForeignKey(
						name: "FK_UserPermitGroup_User_UserId",
						column: x => x.UserId,
						principalTable: "User",
						principalColumn: "Id");
				});

			migrationBuilder.CreateTable(
				name: "UserSession",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					UserId = table.Column<Guid>(nullable: false),
					Secret = table.Column<string>(maxLength: 44, nullable: false),
					EffectiveFrom = table.Column<DateTime>(nullable: false),
					ExpiredOn = table.Column<DateTime>(nullable: true),
					ImpersonatedUserId = table.Column<Guid>(nullable: true),
					SlidingExpiration = table.Column<bool>(nullable: false),
					SlideByDurationInMinutes = table.Column<int>(nullable: true),
					Device = table.Column<string>(maxLength: 100, nullable: true),
					Ip = table.Column<string>(maxLength: 40, nullable: true),
					Location = table.Column<string>(maxLength: 100, nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserSession", x => x.Id);
					table.ForeignKey(
						name: "FK_UserSession_User_UserId",
						column: x => x.UserId,
						principalTable: "User",
						principalColumn: "Id");
				});

			migrationBuilder.CreateTable(
				name: "UserPermit",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					UserPermitGroupId = table.Column<Guid>(nullable: false),
					PermissionCode = table.Column<string>(maxLength: 40, nullable: false),
					EntityId = table.Column<Guid>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserPermit", x => x.Id);
					table.ForeignKey(
						name: "FK_UserPermit_Permission_PermissionCode",
						column: x => x.PermissionCode,
						principalTable: "Permission",
						principalColumn: "PermissionCode");
					table.ForeignKey(
						name: "FK_UserPermit_UserPermitGroup_UserPermitGroupId",
						column: x => x.UserPermitGroupId,
						principalTable: "UserPermitGroup",
						principalColumn: "Id");
				});

			migrationBuilder.CreateIndex(
				name: "IX_ImpliedPermission_ImpliedPermissionCode",
				table: "ImpliedPermission",
				column: "ImpliedPermissionCode");

			migrationBuilder.CreateIndex(
				name: "IX_User_ParentId",
				table: "User",
				column: "ParentId");

			migrationBuilder.CreateIndex(
				name: "IX_User_Username",
				table: "User",
				column: "Username",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_UserPermit_PermissionCode",
				table: "UserPermit",
				column: "PermissionCode");

			migrationBuilder.CreateIndex(
				name: "UK_UserPermit_UserPermitGroupId_PermissionCode_EntityId",
				table: "UserPermit",
				columns: new[] { "UserPermitGroupId", "PermissionCode", "EntityId" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "UK_UserPermitGroup_UserId_GroupName",
				table: "UserPermitGroup",
				columns: new[] { "UserId", "GroupName" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_UserSession_Secret",
				table: "UserSession",
				column: "Secret",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_UserSession_UserId",
				table: "UserSession",
				column: "UserId");

			CreateProcedures(migrationBuilder);
			InsertPermissions(migrationBuilder);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			DropProcedures(migrationBuilder);

			migrationBuilder.DropTable(
				name: "ImpliedPermission");

			migrationBuilder.DropTable(
				name: "UserPermit");

			migrationBuilder.DropTable(
				name: "UserSession");

			migrationBuilder.DropTable(
				name: "Permission");

			migrationBuilder.DropTable(
				name: "UserPermitGroup");

			migrationBuilder.DropTable(
				name: "User");
		}

		private void InsertPermissions(MigrationBuilder migrationBuilder)
		{
			foreach (var script in DbLogic.GetInsertScripts())
			{
				migrationBuilder.Sql(script);
			}
		}

		private void CreateProcedures(MigrationBuilder migrationBuilder)
		{
			foreach (var script in DbLogic.GetCreateScripts())
			{
				migrationBuilder.Sql(string.Format(script, "dbo"));
			}
		}

		private void DropProcedures(MigrationBuilder migrationBuilder)
		{
			foreach (var script in DbLogic.GetDropScripts())
			{
				migrationBuilder.Sql(string.Format(script, "dbo"));
			}
		}
	}
}
