using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SuperCRM.DataModels;
using SuperCRM.Infrastructure;
using SuperCRM.Security;
using ASPSecurityKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace SuperCRM.Repositories
{
	public class UserRepository : IUserRepository<Guid>
	{
		private readonly AppDbContext dbContext;

		public UserRepository(AppDbContext dbContext)
		{
			this.dbContext = dbContext;
		}

		public IUser GetNewUser()
		{
			return new DbUser();
		}

		public IUser GetUserById(Guid id)
		{
			return dbContext.Users.SingleOrDefault(x => x.Id == id);
		}

		public Task<IUser> GetUserByIdAsync(Guid id)
		{
			return this.GetUserByIdAsync(id, CancellationToken.None);
		}

		public async Task<IUser> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
		{
			return await dbContext.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
		}

		public IUser GetUserByUsername(string username)
		{
			return dbContext.Users.SingleOrDefault(x => x.Username == username);
		}

		public Task<IUser> GetUserByUsernameAsync(string username)
		{
			return this.GetUserByUsernameAsync(username, CancellationToken.None);
		}

		public async Task<IUser> GetUserByUsernameAsync(string username, CancellationToken cancellationToken)
		{
			return await dbContext.Users.SingleOrDefaultAsync(x => x.Username == username, cancellationToken).ConfigureAwait(false);
		}

		public IUser LoadUser(string username)
		{
			return dbContext.Users
				.SingleOrDefault(x => x.Username == username);
		}

		public Task<IUser> LoadUserAsync(string username)
		{
			return this.LoadUserAsync(username, CancellationToken.None);
		}

		public async Task<IUser> LoadUserAsync(string username, CancellationToken cancellationToken)
		{
			return await dbContext.Users
				.SingleOrDefaultAsync(x => x.Username == username, cancellationToken)
				.ConfigureAwait(false);
		}

		public IUser LoadUser(Guid id)
		{
			return dbContext.Users
				.SingleOrDefault(x => x.Id == id);
		}

		public Task<IUser> LoadUserAsync(Guid id)
		{
			return this.LoadUserAsync(id, CancellationToken.None);
		}

		public async Task<IUser> LoadUserAsync(Guid id, CancellationToken cancellationToken)
		{
			return await dbContext.Users
				.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
				.ConfigureAwait(false);
		}

		public IUser LoadFakeUser(IAuthDetails auth)
		{
			return new DbUser();
		}

		public Task<IUser> LoadFakeUserAsync(IAuthDetails auth)
		{
			return this.LoadFakeUserAsync(auth, CancellationToken.None);
		}

		public async Task<IUser> LoadFakeUserAsync(IAuthDetails auth, CancellationToken cancellationToken)
		{
			return await Task.FromResult(new DbUser()).ConfigureAwait(false);
		}

		public IUser GetUserByVerificationToken(string token)
		{
			return dbContext.Users.SingleOrDefault(x => x.VerificationToken == token);
		}

		public Task<IUser> GetUserByVerificationTokenAsync(string token)
		{
			return this.GetUserByVerificationTokenAsync(token, CancellationToken.None);
		}

		public async Task<IUser> GetUserByVerificationTokenAsync(string token, CancellationToken cancellationToken)
		{
			return await dbContext.Users.SingleOrDefaultAsync(x => x.VerificationToken == token, cancellationToken).ConfigureAwait(false);
		}

		public IUser GetUserByPasswordResetToken(string token)
		{
			return dbContext.Users.SingleOrDefault(x => x.PasswordResetToken == token);
		}

		public Task<IUser> GetUserByPasswordResetTokenAsync(string token)
		{
			return this.GetUserByPasswordResetTokenAsync(token, CancellationToken.None);
		}

		public async Task<IUser> GetUserByPasswordResetTokenAsync(string token, CancellationToken cancellationToken)
		{
			return await dbContext.Users.SingleOrDefaultAsync(x => x.PasswordResetToken == token, cancellationToken).ConfigureAwait(false);
		}

		public bool AddUser(IUser model)
		{
			try
			{
				var user = (DbUser)model;
				this.dbContext.Users.Add(user);


				var permitGroup = new DbUserPermitGroup
				{
					Id = Guid.NewGuid(),
					GroupName = Constants.PrimaryGroup,
					UserId = user.Id
				};
				dbContext.UserPermitGroups.Add(permitGroup);

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

		public Task<bool> AddUserAsync(IUser model)
		{
			return this.AddUserAsync(model, CancellationToken.None);
		}

		public async Task<bool> AddUserAsync(IUser model, CancellationToken cancellationToken)
		{
			try
			{
				var user = (DbUser)model;
				this.dbContext.Users.Add(user);


				var permitGroup = new DbUserPermitGroup
				{
					Id = Guid.NewGuid(),
					GroupName = Constants.PrimaryGroup,
					UserId = user.Id
				};
				dbContext.UserPermitGroups.Add(permitGroup);

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

		public bool UpdateUser(IUser model)
		{
			try
			{
				// The entity is already attached.
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

		public Task<bool> UpdateUserAsync(IUser model)
		{
			return this.UpdateUserAsync(model, CancellationToken.None);
		}

		public async Task<bool> UpdateUserAsync(IUser model, CancellationToken cancellationToken)
		{
			try
			{
				// The entity is already attached.
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
	}
}