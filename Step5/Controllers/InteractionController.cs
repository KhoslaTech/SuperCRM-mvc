using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ASPSecurityKit;
using ASPSecurityKit.Net;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SuperCRM.DataModels;
using SuperCRM.Models;

namespace SuperCRM.Controllers
{
    public class InteractionController : ServiceControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IMapper mapper;

        public InteractionController(AppDbContext dbContext, IMapper mapper, IUserService<Guid, Guid, DbUser> userService,
            INetSecuritySettings securitySettings, ISecurityUtility securityUtility, ILogger logger) :
            base(userService, securitySettings, securityUtility, logger)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        [PossessesPermissionCode]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [PossessesPermissionCode, AuthAction("Index")]
        public async Task<ActionResult> List(int jtStartIndex, int jtPageSize, Guid? contactId = null)
        {
            return await SecureJsonAction(async () =>
            {
                Expression<Func<DbInteraction, bool>> predicate;
                if (contactId.HasValue)
	                predicate = i => i.ContactId == contactId.Value;
                else
	                predicate = i => i.Contact.OwnerId == this.UserService.CurrentUser.OwnerUserId;

                var result = new
                {
	                Total = this.dbContext.Interactions.Count(predicate),
					ThisPage = await this.dbContext.Interactions.Where(predicate)
						.OrderByDescending(p => p.InteractionDate).Skip(jtStartIndex).Take(jtPageSize).AsQueryable()
						.ProjectTo<Interaction>(mapper.ConfigurationProvider).ToListAsync()
				};

                return Json(ApiResponse.List(result.ThisPage, result.Total));
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(Interaction model)
        {
            return await SecureJsonAction(async () =>
            {
                if (ModelState.IsValid)
                {
                    var entity = mapper.Map<DbInteraction>(model);
                    entity.CreatedById = this.UserService.CurrentUserId;
                    this.dbContext.Interactions.Add(entity);
                    await this.dbContext.SaveChangesAsync();
                    return Json(ApiResponse.Single(mapper.Map<Interaction>(entity)));
                }

                throw new OpException(OpResult.InvalidInput);
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Interaction model)
        {
            return await SecureJsonAction(async () =>
            {
                if (ModelState.IsValid)
                {
                    var entity = await this.dbContext.Interactions.FindAsync(model.Id);
                    if (entity == null)
                        throw new OpException(OpResult.DoNotExist, "Interaction not found.");

                    mapper.Map(model, entity);
                    await this.dbContext.SaveChangesAsync();
                    return Json(ApiResponse.Single(mapper.Map<Interaction>(entity)));
                }

                throw new OpException(OpResult.InvalidInput);
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Guid id)
        {
            return await SecureJsonAction(async () =>
            {
                var entity = await this.dbContext.Interactions.FindAsync(id);
                if (entity == null)
                    throw new OpException(OpResult.DoNotExist, "Interaction not found.");

                this.dbContext.Remove(entity);
                await this.dbContext.SaveChangesAsync();
                return Json(ApiResponse.Success());
            });
        }
    }
}
