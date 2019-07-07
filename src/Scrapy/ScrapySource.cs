using System.Collections.Generic;

namespace Scrapy
{
    public class ScrapySource
    {
        private readonly Dictionary<string, string> _content;

        public ScrapySource(ScrapyRule rule) : this(new List<ScrapyRule> { rule })
        {

        }

        public ScrapySource(List<ScrapyRule> rules)
        {
            Rules = rules;
            _content = new Dictionary<string, string>();
        }

        public ScrapySource(List<ScrapyRule> rules, Dictionary<string, string> dict) : this(rules)
        {
            foreach (var key in dict.Keys)
            {
                _content.Add(key, dict[key]);
            }
        }

        public ScrapySource(List<ScrapyRule> rules, ScrapySource source) : this(rules, source._content) { }

        public void AddContent(string key, string value)
        {
            _content.Add(key, value);
        }

        public Dictionary<string, string> GetContent()
        {
            return _content;
        }

        public string Name { get; set; }
        public string Url { get; set; }
        public List<ScrapyRule> Rules { get; set; }
    }
}
