﻿using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace Aurora.Domain.Entities;

public class SearchResult
{
    [Key]
    public Guid Id { get; set; }
    public string ImagePreviewUrl { get; set; } = "";
    public string SearchItemUrl { get; set; } = "";
    public DateTime FoundTimeUtc { get; set; }
    public JObject? AdditionalData { get; set; } = null;

    public Guid SearchOptionSnapshotId { get; set; }
    public SearchOptionSnapshot SearchOptionSnapshot { get; set; } = null!;
}
