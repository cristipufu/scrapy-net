using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Scrapy.ConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            ConcurrentBag<Dictionary<string, string>> products = new ConcurrentBag<Dictionary<string, string>>();

            // define rules
            // TODO define rules as json object
            var itemsRule = new ScrapyRule
            {
                Selector = ".product-name a",
                Type = ScrapyRuleType.Source,
                Source = new ScrapySource(new List<ScrapyRule>
                {
                    new ScrapyRule
                    {
                        Name = "MetaKeywords",
                        Selector = "meta[name=keywords]",
                        Attribute = "content",
                        Type = ScrapyRuleType.Attribute
                    },
                    new ScrapyRule
                    {
                        Name = "MetaDescription",
                        Selector = "meta[name=description]",
                        Attribute = "content",
                        Type = ScrapyRuleType.Attribute
                    },
                    new ScrapyRule
                    {
                        Name = "Name",
                        Selector = ".product-details h1",
                        Type = ScrapyRuleType.Text
                    },
                    new ScrapyRule
                    {
                        Name = "Price",
                        Selector = ".price",
                        Type = ScrapyRuleType.Text
                    },
                    new ScrapyRule
                    {
                        Name = "Description",
                        Selector = "#tab-description",
                        Type = ScrapyRuleType.Text
                    },
                    new ScrapyRule
                    {
                        Name = "Description2",
                        Selector = "#tab-param",
                        Type = ScrapyRuleType.Text
                    },
                    new ScrapyRule
                    {
                        Name = "Image",
                        Selector = ".product-picture-big",
                        Type = ScrapyRuleType.Image
                    }
                })
            };

            var rules = new List<ScrapyRule>
            {
                new ScrapyRule
                {
                    Selector = ".list-item a",
                    Type = ScrapyRuleType.Source,
                    Source = new ScrapySource(new List<ScrapyRule>
                    {
                        new ScrapyRule
                        {
                            Selector = ".list-item.selected a",
                            Type = ScrapyRuleType.Text,
                            Name = "Category"
                        },
                        new ScrapyRule
                        {
                            Selector = ".page-next", // TODO find a way to apply this rule for each children sources
                            Type = ScrapyRuleType.Source,
                            Source = new ScrapySource(new List<ScrapyRule>
                            {
                                itemsRule
                            })
                        },
                        itemsRule
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

            if (products.Count > 0)
            {
                // export
                new ExcelBuilder(products.ToArray()).ToExcelFile(Path.Combine(path, "products.xlsx"));
            }
        }
    }
}