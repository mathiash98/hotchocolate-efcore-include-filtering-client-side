using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1;

using System.Linq;
using HotChocolate.Data;

public class GraphQLQuery
{
    [UseFiltering]
    [UseSorting]
    public IQueryable<Person> GetPersons(AppContext dbContext) => dbContext.Persons
        .Where(d => d.DeletedUtcDateTime == null)
        .Include(i => i.Actions
            .Where(a => a.DeletedUtcDateTime == null)
        )
        .AsNoTracking();

    [UseFiltering]
    [UseSorting]
    public IQueryable<Person> GetPersonWithValidEfCoreQuery(AppContext dbContext) => dbContext.Persons
        .Where(d => d.DeletedUtcDateTime == null)
        .Include(i => i.Actions
            .Where(a => a.DeletedUtcDateTime == null)
            .OrderByDescending(a => a.DateTimeUtc)
            .Take(1)
        )
        .AsNoTracking();
}
