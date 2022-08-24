namespace Aurora.Scrapers.ResultData;

public sealed class FootfetishBooruResultData : SearchResultData
{
    public FootfetishBooruResultData(string[] tags, int score, string rating)
    {
        Tags = tags;
        Score = score;
        Rating = rating;
    }

    public string[] Tags { get; set; }
    public int Score { get; }
    public string Rating { get; }
}