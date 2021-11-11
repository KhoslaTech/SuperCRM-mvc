using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using ASPSecurityKit;
using SuperCRM.Models;
using ASPSecurityKit.AuthProviders;
using Microsoft.EntityFrameworkCore;

namespace SuperCRM.DataModels
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions options) : base(options)
		{
		}

		public DbSet<DbUser> Users { get; set; }
		public DbSet<DbPermission> Permissions { get; set; }
		public DbSet<DbImpliedPermission> ImpliedPermissions { get; set; }
		public DbSet<DbUserPermit> UserPermits { get; set; }
		public DbSet<DbUserSession> UserSessions { get; set; }
		public DbSet<DbUserPermitGroup> UserPermitGroups { get; set; }


		// models for views (SP responses)
		public DbSet<Permit> Permits { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<DbUser>()
				.HasKey(x => x.Id);

			modelBuilder.Entity<DbUser>()
				.HasIndex(x => x.Username)
				.IsUnique();

			modelBuilder.Entity<DbUserSession>()
				.HasIndex(x => x.Secret)
				.IsUnique();



			modelBuilder.Entity<DbUserPermitGroup>()
				.HasIndex(x => new { x.UserId, x.GroupName })
				.HasName("UK_UserPermitGroup_UserId_GroupName")
				.IsUnique();

			modelBuilder.Entity<DbUserPermit>()
				.HasIndex(x => new { x.UserPermitGroupId, x.PermissionCode, x.EntityId })
				.HasName("UK_UserPermit_UserPermitGroupId_PermissionCode_EntityId")
				.IsUnique()
				.HasFilter(null);

			modelBuilder.Entity<DbImpliedPermission>()
				.HasKey(x => new { x.PermissionCode, x.ImpliedPermissionCode });


			modelBuilder.Entity<Permit>().HasNoKey().ToView(nameof(Permit));

			// Set cascade deletion to do nothing (consequently deletion fails if there's a dependent record)
			// because we should delete the dependent stuff explicitly to avoid unexpected deletion.
			foreach (var relationship in modelBuilder.Model.GetEntityTypes()
				.SelectMany(e => e.GetForeignKeys()))
			{
				relationship.DeleteBehavior = DeleteBehavior.NoAction;
			}
		}
	}

	[Table("User")]
	public class DbUser : IUser<Guid>
	{
		[Key]
		public Guid Id { get; set; }

		[MaxLength(60)]
		[Required]
		public string Name { get; set; }

		[MaxLength(128)]
		[Required]
		public string HashedPassword { get; set; }

		[MaxLength(100)]
		[Required]
		public string Username { get; set; }

		[Required]
		public bool Verified { get; set; }

		[MaxLength(36)]
		[Required]
		public string VerificationToken { get; set; }

		[MaxLength(36)]
		public string PasswordResetToken { get; set; }

		public DateTime? PasswordResetTokenExpiredAt { get; set; }

		public DateTime CreatedDate { get; set; }

		[ForeignKey("Parent")]
		public Guid? ParentId { get; set; }
		public DbUser Parent { get; set; }


		public bool Suspended { get; set; }
		public DateTime? SuspensionDate { get; set; }
		public string SuspensionReason { get; set; }

		public bool PasswordBlocked { get; set; }
		public DateTime? PasswordBlockedDate { get; set; }
		public string PasswordBlockedReason { get; set; }

		public DateTime? PasswordExpiredAt { get; set; }

		[NotMapped]
		public bool MFANotRequired => false;


		[NotMapped]
		public IUserMultiFactor MultiFactor
		{
			get
			{
				return null;
			}
		}

		[NotMapped]
		object IUser.Id { get => this.Id; set => this.Id = (Guid)value; }


		public IList<DbUserPermitGroup> PermitGroups { get; set; }

		public IList<DbUserSession> Sessions { get; set; }
	}


	[Table("UserSession")]
	public class DbUserSession
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[ForeignKey("User")]
		public Guid UserId { get; set; }
		public DbUser User { get; set; }

		[Required]
		[MaxLength(44)]
		public string Secret { get; set; }

		public DateTime EffectiveFrom { get; set; }
		public DateTime? ExpiredOn { get; set; }


		public Guid? ImpersonatedUserId { get; set; }

		public bool SlidingExpiration { get; set; }

		public int? SlideByDurationInMinutes { get; set; }

		[MaxLength(100)]
		public string Device { get; set; }
		[MaxLength(40)]
		public string Ip { get; set; }
		[MaxLength(100)]
		public string Location { get; set; }
	}



	[Table("UserPermitGroup")]
	public class DbUserPermitGroup
	{
		[Key]
		public Guid Id { get; set; }

		[ForeignKey("User")]
		public Guid UserId { get; set; }
		public DbUser User { get; set; }

		[Required, MaxLength(100)]
		public string GroupName { get; set; }

		public IList<DbUserPermit> Permits { get; set; }
	}

	[Table("UserPermit")]
	public class DbUserPermit : IPermit<Guid>
	{
		[Key]
		public Guid Id { get; set; }

		[ForeignKey("UserPermitGroup")]
		public Guid UserPermitGroupId { get; set; }
		public DbUserPermitGroup UserPermitGroup { get; set; }

		[MaxLength(40)]
		[Required]
		[ForeignKey("Permission")]
		public string PermissionCode { get; set; }
		public DbPermission Permission { get; set; }

		public Guid? EntityId { get; set; }

		object IPermit.EntityId
		{
			get => this.EntityId;
			set => this.EntityId = (Guid?)value;
		}
	}

	[Table("Permission")]
	public class DbPermission
	{
		[MaxLength(40)]
		[Required]
		[Key]
		public string PermissionCode { get; set; }

		[MaxLength(30)]
		[Required]
		public string EntityTypeCode { get; set; }

		public PermissionKind Kind { get; set; }

		[MaxLength(100)]
		[Required]
		public string Description { get; set; }

		[InverseProperty("Permission")]
		public IList<DbImpliedPermission> MyImplied { get; set; }

		[InverseProperty("Implied")]
		public IList<DbImpliedPermission> IAmImplied { get; set; }

		public IList<DbUserPermit> Permits { get; set; }
	}

	[Table("ImpliedPermission")]
	public class DbImpliedPermission
	{
		[MaxLength(40)]
		[Required]
		[Key, Column(Order = 0)]
		[ForeignKey("Permission")]
		public string PermissionCode { get; set; }
		public DbPermission Permission { get; set; }

		[MaxLength(40)]
		[Required]
		[Key, Column(Order = 1)]
		[ForeignKey("Implied")]
		public string ImpliedPermissionCode { get; set; }
		public DbPermission Implied { get; set; }
	}


	}