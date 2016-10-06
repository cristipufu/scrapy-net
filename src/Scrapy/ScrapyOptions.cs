namespace Scrapy
{
    public class ScrapyOptions
    {
        public double WaitForSourceTimeout { get; set; }
        public int MaxDegreeOfParallelism { get; set; }
        public string ImagesPath { get; set; }
        public string BaseUrl { get; set; }

        public static ScrapyOptions GetDefault()
        {
            return new ScrapyOptions
            {
                BaseUrl = "http://www.profihairshop.ro/",
                WaitForSourceTimeout = 10000,
                MaxDegreeOfParallelism = 20,
                ImagesPath = @"D:\Images"
            };
        }
    }
}
