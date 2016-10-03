using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrapy
{
    class Program
    {
        static object locker = new object();

        static void Main(string[] args)
        {
            var source = new ScrapySource(new List<ScrapyRule>
            {
                new ScrapyRule
                {
                    Name = "Brand",
                    Selector = "#col-annot h1",
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
                    Selector = ".product-list .item a",
                    Type = ScrapyRuleType.Source,
                    Source = new ScrapySource(new List<ScrapyRule>
                    {
                        new ScrapyRule
                        {
                            Name = "Product",
                            Selector = "h1",
                            Type = ScrapyRuleType.Text
                        },
                        new ScrapyRule
                        {
                            Name = "Description",
                            Selector = ".description .col-l span",
                            Type = ScrapyRuleType.Text
                        },
                        new ScrapyRule
                        {
                            Name = "Image",
                            Selector = "#product-image li img",
                            Type = ScrapyRuleType.Image
                        }
                    })
                }
            })
            {
                Url = "http://www.aoro.ro/loreal-professionnel/"
            };

            //var sources = new ConcurrentBag<ScrapySource>();

            //sources.Add(source);

            //for (var i = 1; i < 15; i++)
            //{
            //    var pageSource = new ScrapySource(source.Rules)
            //    {
            //        Url = source.Url + string.Format("?f={0}-1-2-258", i)
            //    };

            //    sources.Add(pageSource);
            //}

            var dumpFile = "dump_file.txt";

            if (!File.Exists(dumpFile))
            {
                File.Create(dumpFile).Close();
            }

            var client = new ScrapyClient()
                .Dump((content) =>
                {
                    lock (locker)
                    {
                        var line = string.Join(";", content.Select(x => x.Key + "=" + x.Value).ToArray());

                        File.AppendAllText(dumpFile, $"{line}{Environment.NewLine}");
                    }
                });

            client.Scrape(source);
        }
    }
}
