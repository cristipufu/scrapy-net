using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrapy
{
    public class ScrapySource
    {
        public Dictionary<string, string> Content;

        public ScrapySource(List<ScrapyRule> rules)
        {
            Rules = rules;
            Content = new Dictionary<string, string>();
        }

        public ScrapySource(List<ScrapyRule> rules, Dictionary<string, string> dict) : this(rules)
        {
            foreach (var key in dict.Keys)
            {
                Content.Add(key, dict[key]);
            }
        }

        public ScrapySource(List<ScrapyRule> rules, ScrapySource source) : this(rules, source.Content)
        {

        }

        public void AddContent(string key, string value)
        {
            Content.Add(key, value);
        }

        public Dictionary<string, string> GetContent()
        {
            return Content;
        }

        public string Url { get; set; }
        public List<ScrapyRule> Rules { get; set; }
    }
}
