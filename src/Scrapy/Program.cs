using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrapy
{
    class Program
    {
        static void Main(string[] args)
        {
            ConcurrentBag<Dictionary<string, string>> products = new ConcurrentBag<Dictionary<string, string>>();

            //var source = new ScrapySource(new List<ScrapyRule>
            //{
            //    new ScrapyRule
            //    {
            //        Selector = ".product-meta h3.name a",
            //        Type = ScrapyRuleType.Source,
            //        Source = new ScrapySource(new List<ScrapyRule>
            //        {
            //            new ScrapyRule
            //            {
            //                Name = "Name",
            //                Selector = ".product-view h1",
            //                Type = ScrapyRuleType.Text
            //            },
            //            new ScrapyRule
            //            {
            //                Name = "Price",
            //                Selector = ".price-gruop",
            //                Type = ScrapyRuleType.Text
            //            },
            //            new ScrapyRule
            //            {
            //                Name = "Description",
            //                Selector = "#tab-description p",
            //                Type = ScrapyRuleType.Text
            //            },
            //            new ScrapyRule
            //            {
            //                Name = "Image",
            //                Selector = ".image a img",
            //                Type = ScrapyRuleType.Image
            //            }
            //        })
            //    }
            //})
            //{
            //    Url = "http://www.cosmoshop.ro/loreal-professionnel"
            //};

            var source = new ScrapySource(new List<ScrapyRule>
            {
                new ScrapyRule
                {
                    Name = "Name",
                    Selector = ".product-view h1",
                    Type = ScrapyRuleType.Text
                },
                new ScrapyRule
                {
                    Name = "Price",
                    Selectors = new List<string> { ".price-new", ".price-gruop" },
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
                    Name = "Image",
                    Selector = ".image a img",
                    Type = ScrapyRuleType.Image
                }
            })
            {
                Url = "http://www.cosmoshop.ro/kit-intens-hranitor-pentru-par-gros-mythic-oil-sampon-masca-ulei-l-oreal-professionnel"
            };

            var client = new ScrapyClient()
                .Dump((content) =>
                {
                    products.Add(content);
                });

            client.Scrape(source);
        }
    }
}
