using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Scrapy.ConsoleDemo
{
    class Program
    {
       static async Task Main(string[] args)
       {
            ServicePointManager.DefaultConnectionLimit = 20;

            var products = new ConcurrentBag<Dictionary<string, string>>();

            // TODO import rules from a json file
            var rule = new ScrapyRule
            {
                Selector = ".page-title a",
                Type = ScrapyRuleType.Source,
                Source = new ScrapySource(new List<ScrapyRule>
                {
                    new ScrapyRule
                    {
                        Name = "Name",
                        Selector = ".country-name",
                        Type = ScrapyRuleType.Text
                    },
                    new ScrapyRule
                    {
                        Name = "Capital",
                        Selector = ".country-info .country-capital",
                        Type = ScrapyRuleType.Text
                    },
                    new ScrapyRule
                    {
                        Name = "Population",
                        Selector = ".country-info .country-population",
                        Type = ScrapyRuleType.Text
                    },
                    new ScrapyRule
                    {
                        Name = "Area",
                        Selector = ".country-info .country-area",
                        Type = ScrapyRuleType.Text
                    }
                })
            };

            var source = new ScrapySource(rule)
            {
                Name = "countries",
                Url = "https://scrapethissite.com/pages/"
            };

            var path = $@"C:\Scrapy\{source.Name}";

            // init client
            var client = new ScrapyClient(new ScrapyOptions
            {
                BaseUrl = "https://scrapethissite.com/",
                WaitForSourceTimeout = 500,
                MaxDegreeOfParallelism = 10,
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
            var sw = Stopwatch.StartNew();

            await client.ScrapeAsync(source);

            sw.Stop();

            Console.WriteLine($"ElapsedMilliseconds: {sw.ElapsedMilliseconds}");

            if (products.Count > 0)
            {
                // export
                new ExcelBuilder(products.ToArray())
                    .Export(Path.Combine(path, "products.xlsx"));
            }

            Console.ReadLine();
        }
    }
}