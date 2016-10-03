using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrapy
{
    public class ScrapyOptions
    {
        public double RequestTimeout { get; set; }
        public int MaxDegreeOfParallelism { get; set; }

        public static ScrapyOptions GetDefault()
        {
            return new ScrapyOptions
            {
                RequestTimeout = 10000,
                MaxDegreeOfParallelism = 20
            };
        }
    }
}
