using Lappy.Core.Models;

namespace Lappy.General.Helpers;

internal static class Helpers
{
    public static IQueryable<T> Paging<T>(this IQueryable<T> query, TableRequest request)
        => query.Skip(request.PageSize * (request.Page - 1)).Take(request.PageSize);


}
