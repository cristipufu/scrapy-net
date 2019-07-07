namespace Scrapy
{
    public class ScrapyOptions
    {
        public double WaitForSourceTimeout { get; set; }
        public int MaxDegreeOfParallelism { get; set; }
        public string Path { get; set; }
        public string BaseUrl { get; set; }

        public static ScrapyOptions Default { get; } = new ScrapyOptions
        {
            WaitForSourceTimeout = 2000,
            MaxDegreeOfParallelism = 20
        };
    }
}