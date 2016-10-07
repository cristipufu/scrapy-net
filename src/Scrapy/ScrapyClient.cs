using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
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
        private Action<string> _log;

        public ScrapyClient() : this(ScrapyOptions.GetDefault())
        {

        }

        public ScrapyClient(ScrapyOptions options)
        {
            _options = options;

            if (!string.IsNullOrEmpty(options.Path))
            {
                if (!Directory.Exists(options.Path))
                {
                    Directory.CreateDirectory(options.Path);
                }
            }
        }

        public ScrapyClient Dump(Action<Dictionary<string, string>> action)
        {
            _dump = action;

            return this;
        }

        public ScrapyClient Log(Action<string> log)
        {
            _log = log;

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
            var responseBody = string.Empty;

            try
            {
                var httpClient = new HttpClient();
                if (source.Url.StartsWith("//"))
                    source.Url = $"http:{source.Url}";
                if (!string.IsNullOrEmpty(_options.BaseUrl) && !source.Url.StartsWith(_options.BaseUrl))
                    source.Url = $"{_options.BaseUrl}{source.Url}";

                responseBody = await httpClient.GetStringAsync(source.Url);
            }
            catch (Exception ex)
            {
                _log?.Invoke($"[Url error]: {ex.Message}");
            }

            if (string.IsNullOrEmpty(responseBody)) return;

            var parser = new HtmlParser();
            var dom = parser.Parse(responseBody);

            foreach (var rule in source.Rules)
            {
                var elements = GetElements(dom, rule);

                if (elements == null || !elements.Any()) continue;

                switch (rule.Type)
                {
                    case ScrapyRuleType.Text:
                        source.AddContent(rule.Name, WebUtility.HtmlDecode(elements[0].TextContent).Trim());
                        break;

                    case ScrapyRuleType.Attribute:
                        if (string.IsNullOrEmpty(rule.Attribute))
                            continue;
                        var firstElement = elements[0];
                        if (firstElement.HasAttribute(rule.Attribute))
                        {
                            var attr = firstElement.Attributes[rule.Attribute];
                            source.AddContent(rule.Name, attr.Value);
                        }
                        break;

                    case ScrapyRuleType.Image:
                        var imgSrcs = elements.Select(x => x.Attributes["src"].Value).ToList();
                        var imgPaths = new List<string>();
                        foreach(var imgSrc in imgSrcs)
                        {
                            var fileName = await DownloadImage(imgSrc);
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                imgPaths.Add(fileName);
                            }
                        }
                        if (imgPaths.Any())
                        {
                            source.AddContent(rule.Name, string.Join("; ", imgPaths));
                        }
                        break;

                    case ScrapyRuleType.Source:
                        if (rule.Source == null || rule.Source.Rules == null) break;
                        foreach (var element in elements)
                        {
                            var url = element.Attributes["href"].Value;
                            if (url == null) break;
                            var newSource = new ScrapySource(rule.Source.Rules, source);
                            newSource.Url = url;
                            if (_sources.TryAdd(newSource))
                            {
                                _log?.Invoke($"[Source]: {url}");
                            }
                        }
                        break;
                }
            }

            if (source.Rules.All(x => x.Type != ScrapyRuleType.Source))
            {
                var content = source.GetContent();

                _dump?.Invoke(content);
                _log?.Invoke($"[Dump]: {source.Url}");

            }
        }

        private async Task<string> DownloadImage(string url)
        {
            // TODO download byte data image
            if (url.Contains("data:image")) return "";

            try
            {
                var fileName = url.Split('/').LastOrDefault();

                var path = fileName;

                if (!string.IsNullOrEmpty(_options.Path))
                {
                    path = Path.Combine(_options.Path, path);
                }

                if (File.Exists(path))
                {
                    return fileName;
                }

                // TODO should lock here by filename
                using (HttpClient client = new HttpClient())
                {
                    using (var response = await client.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();

                        using (Stream stream = await response.Content.ReadAsStreamAsync())
                        {
                            using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write))
                            {
                                await stream.CopyToAsync(file);
                            }

                            return fileName;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _log?.Invoke($"[Image error]: {ex.Message}");
            }

            return string.Empty;
        }

        private IHtmlCollection<IElement> GetElements(IHtmlDocument dom, ScrapyRule rule)
        {
            if (!string.IsNullOrEmpty(rule.Selector))
            {
                return dom.QuerySelectorAll(rule.Selector);
            }

            if (rule.Selectors != null && rule.Selectors.Any())
            {
                IHtmlCollection<IElement> elements = null;

                foreach (var selector in rule.Selectors)
                {
                    elements = dom.QuerySelectorAll(selector);

                    if (elements.Length > 0) return elements;
                }
            }
            return null;
        }
    }
}