using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Scrapy
{
    public class ScrapyClient
    {
        private BlockingCollection<ScrapySource> _sources = new BlockingCollection<ScrapySource>();
        private ScrapyOptions _options;

        private Action<Dictionary<string, string>> _dump;

        public ScrapyClient() : this(ScrapyOptions.GetDefault())
        {

        }

        public ScrapyClient(ScrapyOptions options)
        {
            _options = options;
        }

        public ScrapyClient Dump(Action<Dictionary<string, string>> action)
        {
            _dump = action;

            return this;
        }

        public void Scrape(params ScrapySource[] sources)
        {
            foreach(var source in sources)
            {
                _sources.Add(source);
            }

            var tasks = new List<Task>();

            for(var i = 0; i < _options.MaxDegreeOfParallelism; i++)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    var timeout = TimeSpan.FromMilliseconds(_options.RequestTimeout);
                    ScrapySource item;
                    while (_sources.TryTake(out item, timeout))
                    {
                        ScrapeSource(item).Wait();
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private async Task ScrapeSource(ScrapySource source)
        {
            var httpClient = new HttpClient();
            var responseBody = await httpClient.GetStringAsync(source.Url);

            var dom = CsQuery.CQ.Create(responseBody);

            foreach (var rule in source.Rules)
            {
                if (rule.Selector == null) continue;

                var elements = dom.Select(rule.Selector);

                if (elements.Any())
                {
                    switch (rule.Type)
                    {
                        case ScrapyRuleType.Text:
                            source.AddContent(rule.Name, WebUtility.HtmlDecode(elements[0].InnerText));
                            break;

                        case ScrapyRuleType.Image:
                            source.AddContent(rule.Name, string.Join(", ", elements.Select(x => x.Attributes["src"]).ToArray()));
                            break;

                        case ScrapyRuleType.Source:

                            if (rule.Source == null || rule.Source.Rules == null) break;

                            foreach (var element in elements)
                            {
                                var url = element.Attributes["href"];

                                if (url == null) break;

                                var newSource = new ScrapySource(rule.Source.Rules, source);

                                newSource.Url = url;

                                _sources.TryAdd(newSource);
                            }
                            break;
                    }
                }
            }

            if (source.Rules.All(x => x.Type != ScrapyRuleType.Source))
            {
                var content = source.GetContent();

                _dump?.Invoke(content);
            }
        }
    }
}