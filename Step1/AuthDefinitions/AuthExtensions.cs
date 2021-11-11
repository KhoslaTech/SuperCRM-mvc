using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SuperCRM.AuthDefinitions
{
	public static class AuthExtensions
	{
		public static async Task<List<IdReference>> ToIdReferenceListAsync(this IQueryable<List<Guid>> source)
		{
			return (await source.ToListAsync())
				.SelectMany(x => x.Select(id => new IdReference(id)))
				.ToList();
		}

		public static List<IdReference> ToIdReferenceList(this IQueryable<List<Guid>> source)
		{
			return source.ToList()
				.SelectMany(x => x.Select(id => new IdReference(id)))
				.ToList();
		}
	}
}
