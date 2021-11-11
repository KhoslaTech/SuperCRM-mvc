using System.Collections.Generic;

namespace SuperCRM.Migrations
{
	public class DbLogic
	{
		public static IEnumerable<string> GetCreateScripts()
		{
			return new List<string>
			{

				#region function GetEntityIdsByParent

				@"
create function [dbo].[GetEntityIdsByParent]
(@entityTypeCode nvarchar(30), 
@parentEntityTypeCode nvarchar(30),
@parentEntityId uniqueidentifier
)
returns @t table(id uniqueidentifier)
as
begin
-- The primary purpose of this function is to return corresponding entities for a parent entity when you have hierarchy of implied permissions.
-- However, due to requirement of cross apply in sql, this function is always called; therefore following code is written to handle cases not related to obtaining child entities given a parent.
if (@parentEntityId is null or @entityTypeCode = @parentEntityTypeCode)
begin -- If the entity type is the same for both, we must return one row with the parent entity id provided.
-- However, regardless of entity type, If we don't have a specific parent entity id, we know it is a general permission so just return null.
insert into @t values(@parentEntityId);
end
else if (@entityTypeCode = 'ADMIN')
begin -- Entity type 'ADMIN' has only general permissions so just return one row with id = null.
insert into @t values(null);
end
else if (@entityTypeCode in ('USER', 'ADMINUSER')) -- ADMINUSER is just an alias to explicitly control admin privileges on user entities regardless of user hierarchy.
begin -- User related permissions must imply from either other higher-level user related permissions or from admin. In either case, returning a row with parent entity id provided is just fine.
insert into @t values(@parentEntityId);
end
-- For each entity type specific to your project that has permissions defined, return all entity ids of type @entityTypeCode that are associated with @parentEntityId
-- else if (@entityTypeCode = 'xyz')
-- begin
-- insert into @t
-- select Id from xyz where ParentId = @parentEntityId;
-- end

return;
end
",

				#endregion

				#region type IdList

				@"
CREATE TYPE [dbo].[IdList] AS TABLE(
	[id] [uniqueidentifier] NULL
)
",

				#endregion


				#region procedure UserPermits_get

				@"
create procedure [dbo].[UserPermits_get]
(
@userId uniqueidentifier
)
as
begin
declare @uPerms table(EntityId uniqueidentifier, PermissionCode nvarchar(40));
with AllPermissions
as
(select pa.EntityId, pa.PermissionCode, perm.EntityTypeCode
from UserPermit pa
join UserPermitGroup upg on upg.Id = pa.UserPermitGroupId
join Permission perm on pa.PermissionCode = perm.PermissionCode
where upg.UserId = @userId
union all
select pa.EntityId, ia.ImpliedPermissionCode, imPerm.EntityTypeCode
from ImpliedPermission ia 
join AllPermissions pa on pa.PermissionCode = ia.PermissionCode
join Permission imPerm on ia.ImpliedPermissionCode = imPerm.PermissionCode
)
insert into @uPerms
select distinct EntityId, PermissionCode from AllPermissions;

with Descendants
as
(select Id from [User]
where ParentId = @userId
union all
select u.Id as Id
from [User] u
join Descendants d on u.ParentId = d.Id
)
select Id as EntityId, PermissionCode 
from Descendants
join Permission perm on perm.EntityTypeCode = 'USER'
where perm.Kind between 1 and 2
union
select * from @uPerms

end
",

				#endregion

				#region procedure GroupPermits_get

				@"
create procedure [dbo].[GroupPermits_get]
(
@userPermitGroupId uniqueidentifier
)
as
begin
with AllPermissions
as
(select pa.EntityId, pa.PermissionCode, perm.EntityTypeCode
from UserPermit pa
join UserPermitGroup upg on upg.Id = pa.UserPermitGroupId
join Permission perm on pa.PermissionCode = perm.PermissionCode
where upg.Id = @userPermitGroupId
union all
select pa.EntityId, ia.ImpliedPermissionCode, imPerm.EntityTypeCode
from ImpliedPermission ia 
join AllPermissions pa on pa.PermissionCode = ia.PermissionCode
join Permission imPerm on ia.ImpliedPermissionCode = imPerm.PermissionCode
)
select distinct EntityId, PermissionCode from AllPermissions;

end
",

				#endregion




				#region trigger AddNewPermissionToSuperAdmin

				@"
create trigger [dbo].[AddNewPermissionToSuperAdmin]
on [dbo].[Permission]
after insert
as
begin
insert into ImpliedPermission
(PermissionCode, ImpliedPermissionCode)
select 'SuperAdmin', PermissionCode
from inserted
where PermissionCode != 'SuperAdmin'
end
"

				#endregion

			};
		}

		public static IEnumerable<string> GetDropScripts()
		{
			return new List<string>
		{
			@"IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[AddNewPermissionToSuperAdmin]'))
DROP TRIGGER [dbo].[AddNewPermissionToSuperAdmin]",



			@"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPermits_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UserPermits_get]
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GroupPermits_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GroupPermits_get]",







				@"
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetEntityIdsByParent]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetEntityIdsByParent]

IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'IdList' AND ss.name = N'dbo')
DROP TYPE [dbo].[IdList]
"};
		}

		public static IEnumerable<string> GetInsertScripts()
		{
			var permissions = new[]
			{
				// SuperAdmin must always be the first permission to be inserted as there's a trigger that automatically makes every new permission it's implied permission.
				"('SuperAdmin', 'ADMIN', 'Super administrator permission', 0)", // general permission


			};


			return new[]
			{
				@"insert into [dbo].[Permission]
(PermissionCode, EntityTypeCode, Description, Kind)
values" + string.Join(",\r\n", permissions),
			};
		}
	}
}