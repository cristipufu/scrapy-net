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
                    Selector = ".product-meta h3.name a",
                    Type = ScrapyRuleType.Source,
                    Source = new ScrapySource(new List<ScrapyRule>
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
                            Selector = ".price-gruop",
                            Type = ScrapyRuleType.Text
                        },
                        new ScrapyRule
                        {
                            Name = "Description",
                            Selector = "#tab-description p",
                            Type = ScrapyRuleType.Text
                        },
                        new ScrapyRule
                        {
                            Name = "Image",
                            Selector = ".image a img",
                            Type = ScrapyRuleType.Image
                        }
                    })
                }
            })
            {
                Url = "http://www.cosmoshop.ro/loreal-professionnel"
            };

            var dumpFile = "dump_file.csv";

            if (!File.Exists(dumpFile))
            {
                File.Create(dumpFile).Close();
            }

            var client = new ScrapyClient()
                .Dump((content) =>
                {
                    lock (locker)
                    {
                        var line = string.Join(",", content.Select(x => $"\"{x.Value}\"").ToArray());

                        File.AppendAllText(dumpFile, $"{line}{Environment.NewLine}");
                    }
                });

            client.Scrape(source);
        }
    }
}
