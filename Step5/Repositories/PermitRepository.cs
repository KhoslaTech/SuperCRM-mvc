using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SuperCRM.DataModels;
using SuperCRM.Infrastructure;
using ASPSecurityKit;
using SuperCRM.Security;
using SuperCRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace SuperCRM.Repositories
{
	public interface IUserPermitRepository : IPermitRepository<Guid, Guid>
	{
		Task<bool> AddPermitAsync(Guid userId, string permissionCode, Guid? entityId);

		Task<bool> AddPermitAsync(string username, string permissionCode, Guid? entityId);

		Task<bool> RemovePermitAsync(Guid userId, string permissionCode, Guid? entityId);
	}

	public class PermitRepository : IUserPermitRepository
	{
		private readonly AppDbContext dbContext;

		public PermitRepository(AppDbContext dbContext)
		{
			this.dbContext = dbContext;
		}

		public bool AddPermit(IPermit permit)
		{
			try
			{
				var model = (DbUserPermit)permit;
				this.dbContext.UserPermits.Add(model);
				this.dbContext.SaveChanges();
				return true;
			}
			catch (DbUpdateException efEx)
			{
				if (efEx.GetBaseException() is SqlException ex &&
					ex.Number.In((int)SqlErrors.KeyViolation, (int)SqlErrors.UniqueIndex))
				{
					return false;
				}

				throw;
			}
		}

		public Task<bool> AddPermitAsync(IPermit permit)
		{
			return this.AddPermitAsync(permit, CancellationToken.None);
		}

		public async Task<bool> AddPermitAsync(IPermit permit, CancellationToken cancellationToken)
		{
			try
			{
				var model = (DbUserPermit)permit;
				this.dbContext.UserPermits.Add(model);
				await this.dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
				return true;
			}
			catch (DbUpdateException efEx)
			{
				if (efEx.GetBaseException() is SqlException ex &&
					ex.Number.In((int)SqlErrors.KeyViolation, (int)SqlErrors.UniqueIndex))
				{
					return false;
				}

				throw;
			}
		}

		public async Task<bool> AddPermitAsync(Guid userId, string permissionCode, Guid? entityId)
		{
			var permit = new DbUserPermit
			{
				Id = Guid.NewGuid(),
				PermissionCode = permissionCode,
				EntityId = entityId,
				UserPermitGroupId = await dbContext.UserPermitGroups
					.Where(x => x.UserId == userId && x.GroupName == Constants.PrimaryGroup).Select(x => x.Id)
					.SingleOrDefaultAsync().ConfigureAwait(false)
			};

			return await AddPermitAsync(permit).ConfigureAwait(false);
		}

		public async Task<bool> AddPermitAsync(string username, string permissionCode, Guid? entityId)
		{
			var permit = new DbUserPermit
			{
				Id = Guid.NewGuid(),
				PermissionCode = permissionCode,
				EntityId = entityId,
				UserPermitGroupId = await dbContext.UserPermitGroups
					.Where(x => x.User.Username == username && x.GroupName == Constants.PrimaryGroup).Select(x => x.Id)
					.SingleOrDefaultAsync().ConfigureAwait(false)
			};

			return await AddPermitAsync(permit).ConfigureAwait(false);
		}

		public IEnumerable<IPermit<Guid>> GetUserPermits(Guid userId)
		{
			return this.dbContext.Permits.FromSqlRaw("UserPermits_get @userId",
				new SqlParameter[] { new SqlParameter("@userId", userId) });
		}

		public Task<IEnumerable<IPermit<Guid>>> GetUserPermitsAsync(Guid userId)
		{
			return this.GetUserPermitsAsync(userId, CancellationToken.None);
		}

		public async Task<IEnumerable<IPermit<Guid>>> GetUserPermitsAsync(Guid userId, CancellationToken cancellationToken)
		{
			return await this.dbContext.Permits.FromSqlRaw("UserPermits_get @userId",
					new SqlParameter[] { new SqlParameter("@userId", userId) })
				.ToListAsync(cancellationToken).ConfigureAwait(false);
		}

		public IEnumerable<IPermit<Guid>> GetGroupPermits(Guid permitGroupId)
		{
			return this.dbContext.Permits.FromSqlRaw("GroupPermits_get @userPermitGroupId",
				new SqlParameter[] { new SqlParameter("@userPermitGroupId", permitGroupId) });
		}

		public Task<IEnumerable<IPermit<Guid>>> GetGroupPermitsAsync(Guid permitGroupId)
		{
			return this.GetGroupPermitsAsync(permitGroupId, CancellationToken.None);
		}

		public async Task<IEnumerable<IPermit<Guid>>> GetGroupPermitsAsync(Guid permitGroupId, CancellationToken cancellationToken)
		{
			return await this.dbContext.Permits.FromSqlRaw("GroupPermits_get @userPermitGroupId",
					new SqlParameter[] { new SqlParameter("@userPermitGroupId", permitGroupId) })
				.ToListAsync(cancellationToken).ConfigureAwait(false);
		}

		public bool RemovePermit(IPermit permit)
		{
			var model = (DbUserPermit)permit;
			this.dbContext.UserPermits.Remove(model);
			this.dbContext.SaveChanges();

			return true;
		}

		public Task<bool> RemovePermitAsync(IPermit permit)
		{
			return this.RemovePermitAsync(permit, CancellationToken.None);
		}

		public async Task<bool> RemovePermitAsync(IPermit permit, CancellationToken cancellationToken)
		{
			var model = (DbUserPermit)permit;
			this.dbContext.UserPermits.Remove(model);
			await this.dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

			return true;
		}

		public async Task<bool> RemovePermitAsync(Guid userId, string permissionCode, Guid? entityId)
		{
			var permit = await this.dbContext.UserPermits
				.Where(x => x.UserPermitGroup.UserId == userId && x.PermissionCode == permissionCode &&
							x.EntityId == entityId).SingleOrDefaultAsync().ConfigureAwait(false);

			return await RemovePermitAsync(permit).ConfigureAwait(false);
		}
	}
}