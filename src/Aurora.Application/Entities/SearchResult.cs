using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace Aurora.Application.Entities;

public class SearchResult
{
    [Key]
    public Guid Id { get; set; }
    public string ImagePreviewUrl { get; set; } = "";
    public string SearchItemUrl { get; set; } = "";
    public DateTime FoundTimeUtc { get; set; }
    public JObject? AdditionalData { get; set; } = null;

    public Guid SearchRequestId { get; set; }
    public SearchRequest SearchRequest { get; set; } = null!;
}
