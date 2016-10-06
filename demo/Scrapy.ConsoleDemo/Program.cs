using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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
                            Name = "Description2",
                            Selector = ".description",
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
                Url = "http://www.cosmoshop.ro/moroccanoil"
            };

            var client = new ScrapyClient()
                .Dump((content) =>
                {
                    products.Add(content);
                })
                .Log((message) =>
                {
                    Console.WriteLine(message);
                });

            client.Scrape(source);

            // export
            ExportToExcel(products.ToArray(), "cosmoshop-moroccanoil");
        }

        private static void ExportToExcel(Dictionary<string, string>[] products, string name)
        {
            Array.Sort(products, new DictionaryComparer());

            var wb = new XSSFWorkbook();

            var sh = (XSSFSheet)wb.CreateSheet(name);

            for (var i = 0; i < products.Length; i++)
            {
                var product = products[i];
                IRow header = null;

                if (i == 0)
                {
                    header = sh.CreateRow(i);
                }

                var r = sh.CreateRow(i + 1);
                var j = 0;

                foreach (var key in product.Keys)
                {
                    if (i == 0)
                    {
                        var headerCell = header.CreateCell(j);
                        headerCell.SetCellValue(key);
                    }
                    var cell = r.CreateCell(j);
                    cell.SetCellValue(product[key]);
                    j++;
                }
            }

            using (var fs = new FileStream($"{name}.xlsx", FileMode.Create, FileAccess.Write))
            {
                wb.Write(fs);
            }
        }

        public class DictionaryComparer : IComparer<Dictionary<string, string>>
        {
            public DictionaryComparer()
            {
            }

            public int Compare(Dictionary<string, string> x, Dictionary<string, string> y)
            {
                return y.Keys.Count - x.Keys.Count;
            }
        }
    }
}
