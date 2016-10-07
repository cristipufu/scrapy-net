# scrapy.net

```c#
var rules = new List<ScrapyRule>
{
    new ScrapyRule
    {
        Selector = ".list-item a", // categories
        Type = ScrapyRuleType.Source,
        Source = new ScrapySource(new List<ScrapyRule>
        {
            new ScrapyRule
            {
                Selector = ".list-item.selected a", // category name
                Type = ScrapyRuleType.Text,
                Name = "Category"
            },
            new ScrapyRule
            {
                Selector = ".page-next", // pagination
                Type = ScrapyRuleType.Source,
                Source = new ScrapySource(new List<ScrapyRule>
                {
                    new ScrapyRule
                    {
                        Selector = ".product-name a", // product
                        Type = ScrapyRuleType.Source,
                        Source = new ScrapySource(new List<ScrapyRule>
                        {
                            new ScrapyRule
                            {
                                Name = "MetaKeywords", // meta keywords 
                                Selector = "meta[name=keywords]",
                                Attribute = "content",
                                Type = ScrapyRuleType.Attribute
                            },
                            new ScrapyRule
                            {
                                Name = "Name", // product name
                                Selector = ".product-details h1",
                                Type = ScrapyRuleType.Text
                            },
                            new ScrapyRule
                            {
                                Name = "Price", // product price
                                Selector = ".price",
                                Type = ScrapyRuleType.Text
                            }
                      }
                })
            }
        })
    }
};

var source = new ScrapySource(rules)
{
    Name = "profihairshop-nioxin",
    Url = "http://www.profihairshop.ro/nioxin"
};

var path = $@"D:\Scrapy\{source.Name}";

// init client
var client = new ScrapyClient(new ScrapyOptions
{
    BaseUrl = "http://www.profihairshop.ro/",
    WaitForSourceTimeout = 10000,
    MaxDegreeOfParallelism = 20,
    Path = path
})
.Dump((content) =>
{
    products.Add(content);
})
.Log((message) =>
{
    Console.WriteLine(message);
});

// start scraping
client.Scrape(source);
```
