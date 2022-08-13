namespace Aurora.Application.Models
{
    public record SearchItem<T>
        (ContentType ContentType, string ImagePreviewUrl, string SearchItemUrl, T? Data = null)
        where T : SearchResultData;
}