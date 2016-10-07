using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrapy.ConsoleDemo
{
    public class ExcelBuilder
    {
        Dictionary<string, string>[] products;

        public ExcelBuilder(Dictionary<string, string>[] products)
        {
            this.products = products;
        }

        public void ToExcelFile(string path)
        {
            Array.Sort(products, new DictionaryComparer());

            var wb = new XSSFWorkbook();
            var sh = (XSSFSheet)wb.CreateSheet("Products");

            var keys = products[0].Keys;

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

                foreach (var key in keys)
                {
                    if (i == 0)
                    {
                        var headerCell = header.CreateCell(j);
                        headerCell.SetCellValue(key);
                    }

                    var cell = r.CreateCell(j);
                    var value = string.Empty;

                    if (product.ContainsKey(key))
                    {
                        value = product[key];
                    }

                    cell.SetCellValue(value);

                    j++;
                }
            }

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                wb.Write(fs);
            }
        }

        public class DictionaryComparer : IComparer<Dictionary<string, string>>
        {
            public DictionaryComparer() { }

            public int Compare(Dictionary<string, string> x, Dictionary<string, string> y)
            {
                return y.Keys.Count - x.Keys.Count;
            }
        }
    }
}
