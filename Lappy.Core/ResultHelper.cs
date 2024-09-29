using Lappy.Core.Models;

namespace Lappy.Core;

public class ResultHelper
{
    public static Result Success() => new(true, 200, string.Empty);
    public static Result<T> Success<T>(T value) where T : class => new Result<T>(true, 200, string.Empty, Value: value);

    public static Result Failed(int statusCode, string message = "") => new(false, statusCode, message);
    public static Result<T> Failed<T>(int statusCode, string message = "") where T : class => new(false, statusCode, message);

    public static Results<T> List<T>(ICollection<T> Values, int Total, int Page, int PageSize)
        where T : class
        => new(Values, Total, Page, PageSize);

    public static Results<T> Empty<T>(int PageSize)
        where T : class
        => new([], 0, 1, PageSize);
}
