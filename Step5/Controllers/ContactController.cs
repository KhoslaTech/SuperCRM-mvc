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
    public class ContactController : ServiceControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IMapper mapper;

        public ContactController(AppDbContext dbContext, IMapper mapper, IUserService<Guid, Guid, DbUser> userService,
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
        public async Task<ActionResult> List(int jtStartIndex, int jtPageSize)
        {
            return await SecureJsonAction(async () =>
            {
                Expression<Func<DbContact, bool>> predicate = c => c.OwnerId == this.UserService.CurrentUser.OwnerUserId;

                var result = new
                {
	                Total = this.dbContext.Contacts.Count(predicate),
	                ThisPage = await this.dbContext.Contacts.Where(predicate)
		                .OrderBy(p => p.Name).Skip(jtStartIndex).Take(jtPageSize)
		                .AsQueryable()
		                .ProjectTo<Contact>(mapper.ConfigurationProvider)
		                .ToListAsync()
                };
                
                return Json(ApiResponse.List(result.ThisPage, result.Total));
            });
        }

        [HttpPost]
        [PossessesPermissionCode, AuthAction("Index")]
        public async Task<ActionResult> GetAll()
        {
	        var result = await this.dbContext.Contacts.Where(c => c.OwnerId == this.UserService.CurrentUser.OwnerUserId)
		        .OrderBy(p => p.Name)
		        .Select(x => new { Value = x.Id, DisplayText = x.Name })
		        .ToListAsync();

	        return Json(new { Options = result, Result = "OK" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [PossessesPermissionCode]
        public async Task<ActionResult> Add(Contact model)
        {
            return await SecureJsonAction(async () =>
            {
                if (ModelState.IsValid)
                {
                    var entity = mapper.Map<DbContact>(model);
                    entity.OwnerId = this.UserService.CurrentUser.OwnerUserId;
                    entity.CreatedById = this.UserService.CurrentUserId;
                    this.dbContext.Contacts.Add(entity);
                    await this.dbContext.SaveChangesAsync();
                    return Json(ApiResponse.Single(mapper.Map<Contact>(entity)));
                }

                throw new OpException(OpResult.InvalidInput);
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Contact model)
        {
            return await SecureJsonAction(async () =>
            {
                if (ModelState.IsValid)
                {
                    var entity = await this.dbContext.Contacts.FindAsync(model.Id);
                    if (entity == null)
                        throw new OpException(OpResult.DoNotExist, "Contact not found.");

                    mapper.Map(model, entity);
                    await this.dbContext.SaveChangesAsync();
                    return Json(ApiResponse.Single(mapper.Map<Contact>(entity)));
                }

                throw new OpException(OpResult.InvalidInput);
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Guid id)
        {
            return await SecureJsonAction(async () =>
            {
                var entity = await this.dbContext.Contacts.Include(x => x.Interactions)
                .SingleOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new OpException(OpResult.DoNotExist, "Contact not found.");

                this.dbContext.Remove(entity);
                await this.dbContext.SaveChangesAsync();
                return Json(ApiResponse.Success());
            });
        }
    }
}
