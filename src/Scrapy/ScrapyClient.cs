using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
            foreach (var source in sources)
            {
                _sources.Add(source);
            }

            var tasks = new List<Task>();

            for (var i = 0; i < _options.MaxDegreeOfParallelism; i++)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    var timeout = TimeSpan.FromMilliseconds(_options.WaitForSourceTimeout);
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
                var elements = GetElements(dom, rule);

                if (elements == null || !elements.Any()) continue;

                switch (rule.Type)
                {
                    case ScrapyRuleType.Text:
                        HtmlAgilityPack.HtmlDocument contentDoc = new HtmlAgilityPack.HtmlDocument();
                        contentDoc.LoadHtml(elements[0].InnerHTML);
                        source.AddContent(rule.Name, WebUtility.HtmlDecode(contentDoc.DocumentNode.InnerText));
                        break;

                    case ScrapyRuleType.Image:
                        var imgSrc = elements.Select(x => x.Attributes["src"]).FirstOrDefault();
                        if (!string.IsNullOrEmpty(imgSrc))
                        {
                            source.AddContent(rule.Name, imgSrc);
                            await DownloadImage(imgSrc);
                        }

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

            if (source.Rules.All(x => x.Type != ScrapyRuleType.Source))
            {
                var content = source.GetContent();

                _dump?.Invoke(content);
            }
        }

        private async Task DownloadImage(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (var response = await client.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();

                        using (Stream stream = await response.Content.ReadAsStreamAsync())
                        {
                            var fileName = url.Split('/').Last();

                            using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                            {
                                await stream.CopyToAsync(file);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        private CsQuery.CQ GetElements(CsQuery.CQ dom, ScrapyRule rule)
        {
            if (!string.IsNullOrEmpty(rule.Selector))
            {
                return dom.Select(rule.Selector);
            }

            if (rule.Selectors != null && rule.Selectors.Any())
            {
                CsQuery.CQ elements = null;

                foreach (var selector in rule.Selectors)
                {
                    elements = dom.Select(selector);

                    if (elements.Length > 0) return elements;
                }
            }

            return null;
        }
    }
}