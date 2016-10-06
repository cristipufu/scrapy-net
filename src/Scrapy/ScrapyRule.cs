using System.Collections.Generic;

namespace Scrapy
{
    public class ScrapyRule
    {
        public string Name { get; set; }
        public string Selector { get; set; }
        public string Attribute { get; set; }
        public List<string> Selectors { get; set; }
        public ScrapyRuleType Type { get; set; }
        public ScrapySource Source { get; set; }
    }
}
