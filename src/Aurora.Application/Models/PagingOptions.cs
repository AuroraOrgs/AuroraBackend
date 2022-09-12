namespace Aurora.Application.Models;

public record PagingOptions(int PageNumber, int PageSize)
{
    public static PagingOptions? Create(int? pageNumber, int? pageSize)
    {
        PagingOptions? paging;
        if (pageNumber is null && pageSize is null)
        {
            paging = null;
        }
        else
        {
            if (pageNumber is null)
            {
                pageNumber = 0;
            }
            else
            {
                if (pageSize is null)
                {
                    pageSize = 25;
                }
            }
            paging = new PagingOptions(pageNumber.Value!, pageSize!.Value);
        }
        return paging;
    }
}