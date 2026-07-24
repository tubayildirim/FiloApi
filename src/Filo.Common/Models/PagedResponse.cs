namespace Filo.Common.Models;

public class PagedResponse<T> : ApiResponse<PagedList<T>>
{
    public static PagedResponse<T> SuccessPagedResponse(IEnumerable<T> items, int count, int pageNumber, int pageSize, string? message = null)
    {
        var pagedList = new PagedList<T>(items, count, pageNumber, pageSize);
        return new PagedResponse<T>
        {
            Success = true,
            Message = message,
            Data = pagedList
        };
    }
}
