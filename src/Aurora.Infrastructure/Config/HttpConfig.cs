namespace Aurora.Infrastructure.Config
{
    [ConfigSection("Http")]
    public class HttpConfig
    {
        public int RetryCount { get; set; } = 3;
        public int WaitFactorMs { get; set; } = 100;
    }
}
