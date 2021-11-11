using System;
using AutoMapper;
using SuperCRM.DataModels;
using SuperCRM.Models;

namespace SuperCRM
{
public class WebAppProfile : Profile
    {
        public WebAppProfile()
        {
            CreateMap<DbContact, Contact>()
                .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedBy.Name));

            CreateMap<Contact, DbContact>()
                .ForMember(d => d.Id, o =>
                {
                    o.PreCondition((s, d, rc) => d.Id == Guid.Empty);
                    o.MapFrom(s => Guid.NewGuid());
                })
                .ForMember(d => d.OwnerId, o => o.Ignore())
                .ForMember(d => d.CreatedById, o => o.Ignore())
                .ForMember(d => d.CreatedDate, o =>
                {
                    o.PreCondition((s, d, rc) => d.CreatedDate == DateTime.MinValue);
                    o.MapFrom(s => DateTime.UtcNow);
                });

        }
    }
}