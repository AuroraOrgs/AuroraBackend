namespace Aurora.Scrapers.ResultData;

public sealed class FootfetishBooruResultData : SearchResultData
{
    public FootfetishBooruResultData(string[] tags)
    {
        Tags = tags;
    }

    public string[] Tags { get; set; }
}