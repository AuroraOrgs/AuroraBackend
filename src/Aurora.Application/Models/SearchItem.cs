namespace Aurora.Application.Models;

public record SearchItem(ContentType ContentType, string ImagePreviewUrl, string SearchItemUrl, SearchResultData? Data = null);
