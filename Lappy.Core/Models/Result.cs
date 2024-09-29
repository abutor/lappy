namespace Lappy.Core.Models;

public record Result(bool IsSuccess, int StatusCode, string Message);

public record Result<T>(bool IsSuccess, int StatusCode, string Message, T? Value = default)
    : Result(IsSuccess, StatusCode, Message) where T : class;

public record Results<T>(ICollection<T> Values, int Total, int Page, int PageSize)
    : Result<ICollection<T>>(true, 200, string.Empty, Values);
