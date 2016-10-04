using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrapy
{
    public class ScrapyRule
    {
        public string Name { get; set; }
        public string Selector { get; set; }
        public List<string> Selectors { get; set; }
        public ScrapyRuleType Type { get; set; }
        public ScrapySource Source { get; set; }
    }
}
